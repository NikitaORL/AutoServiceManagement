using System.Security.Claims;
using AutoService.Core.DTO;
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers
{
    [Authorize(Roles = "Admin,Mechanic")]
    [ApiController]
    [Route("api/repair-orders")]
    public class RepairOrdersController : ControllerBase
    {
        private readonly IRepairOrderService _service;
        private readonly IRepairOrderPartService _partService;
        private readonly IWorkLogService _workLogService;
        private readonly IInvoiceService _invoiceService;
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public RepairOrdersController(
            IRepairOrderService service,
            IRepairOrderPartService partService,
            IWorkLogService workLogService,
            IInvoiceService invoiceService,
            AppDbContext db,
            IConfiguration configuration)
        {
            _service = service;
            _partService = partService;
            _workLogService = workLogService;
            _invoiceService = invoiceService;
            _db = db;
            _configuration = configuration;
        }

        private bool CanAccess(RepairOrder order)
        {
            if (!User.IsInRole("Mechanic"))
                return true;

            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)
                && order.AssignedMechanicUserId == userId;
        }

        private static RepairOrderResponse ToResponse(RepairOrder order)
        {
            return new RepairOrderResponse(
                order.Id,
                order.CarId,
                order.Description,
                order.Status,
                order.CreatedAt,
                order.CompletedAt,
                order.AssignedMechanicUserId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string sort = "date",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || page > 100000 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "Invalid page or pageSize" });

            if (sort != "date" && sort != "status" && sort != "id")
                return BadRequest(new { message = "Unsupported sort field" });

            var orders = await _service.GetAllAsync();
            var query = orders.Where(CanAccess).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o =>
                    o.Status.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.Description.Contains(search, StringComparison.OrdinalIgnoreCase));

            query = sort switch
            {
                "date" => query.OrderByDescending(o => o.CreatedAt).ThenBy(o => o.Id),
                "status" => query.OrderBy(o => o.Status).ThenBy(o => o.Id),
                _ => query.OrderBy(o => o.Id)
            };

            int total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).Select(ToResponse).ToList();

            return Ok(new { items, total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null || !CanAccess(order))
                return NotFound();

            return Ok(ToResponse(order));
        }

        [HttpGet("car/{carId:int}")]
        public async Task<IActionResult> GetByCarId(int carId)
        {
            var orders = await _service.GetByCarIdAsync(carId);
            return Ok(orders.Where(CanAccess).OrderByDescending(o => o.CreatedAt).Select(ToResponse));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(RepairOrderRequest request)
        {
            try
            {
                var order = await _service.CreateAsync(new RepairOrder
                {
                    CarId = request.CarId,
                    Description = request.Description
                });

                return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, RepairOrderRequest request)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, new RepairOrder
                {
                    CarId = request.CarId,
                    Description = request.Description,
                    Status = request.Status
                });

                return updated ? NoContent() : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, RepairStatusRequest request)
        {
            var existing = await _service.GetByIdAsync(id);

            if (existing == null || !CanAccess(existing))
                return NotFound();

            try
            {
                await _service.UpdateAsync(id, new RepairOrder
                {
                    CarId = existing.CarId,
                    Description = existing.Description,
                    Status = request.Status
                });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/assign")]
        public async Task<IActionResult> AssignMechanic(int id, AssignMechanicRequest request)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            bool isMechanic = await _db.Users.AnyAsync(u =>
                u.Id == request.MechanicUserId && u.Role == "Mechanic");

            if (!isMechanic)
                return BadRequest(new { message = "Mechanic account not found" });

            order.AssignedMechanicUserId = request.MechanicUserId;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return await _service.DeleteAsync(id) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/parts")]
        public async Task<IActionResult> AddPart(
            int id,
            [FromQuery] int partId,
            [FromQuery] int quantity)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null || !CanAccess(order))
                return NotFound();

            try
            {
                var result = await _partService.AddToOrderAsync(id, partId, quantity);

                return Created($"/api/repair-orders/{id}/parts", new
                {
                    result.Id,
                    result.RepairOrderId,
                    result.PartId,
                    result.Quantity,
                    result.UnitPrice
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/parts")]
        public async Task<IActionResult> GetParts(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null || !CanAccess(order))
                return NotFound();

            var parts = await _partService.GetByOrderIdAsync(id);
            return Ok(parts.Select(p => new
            {
                p.Id,
                p.RepairOrderId,
                p.PartId,
                p.Quantity,
                p.UnitPrice
            }));
        }

        [Authorize(Roles = "Mechanic")]
        [HttpPost("{id:int}/worklogs")]
        public async Task<IActionResult> AddWorkLog(
            int id,
            [FromQuery] string description,
            [FromQuery] decimal hoursWorked)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null || !CanAccess(order))
                return NotFound();

            var workLog = new WorkLog
            {
                RepairOrderId = id,
                MechanicUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                Description = description,
                HoursWorked = hoursWorked,
                HourlyRate = _configuration.GetValue<decimal>("Labor:DefaultHourlyRate")
            };

            try
            {
                var result = await _workLogService.CreateAsync(workLog);

                return Created($"/api/repair-orders/{id}/worklogs", new
                {
                    result.Id,
                    result.RepairOrderId,
                    result.MechanicUserId,
                    result.Description,
                    result.HoursWorked,
                    result.HourlyRate,
                    result.WorkDate
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/worklogs")]
        public async Task<IActionResult> GetWorkLogs(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null || !CanAccess(order))
                return NotFound();

            var logs = await _workLogService.GetByOrderIdAsync(id);
            return Ok(logs.Select(w => new
            {
                w.Id,
                w.RepairOrderId,
                w.MechanicUserId,
                w.Description,
                w.HoursWorked,
                w.HourlyRate,
                w.WorkDate,
                Total = w.HoursWorked * w.HourlyRate
            }));
        }

        [HttpGet("{id:int}/invoice")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null || !CanAccess(order))
                return NotFound();

            var invoice = await _invoiceService.GetInvoiceAsync(id);
            return invoice == null ? NotFound() : Ok(invoice);
        }
    }
}
