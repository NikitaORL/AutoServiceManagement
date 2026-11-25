using Microsoft.AspNetCore.Authorization;
﻿
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers
{
    [Authorize(Roles = "Admin,Mechanic,Service")]
    [ApiController]
    [Route("api/repair-orders")]
    public class RepairOrdersController : ControllerBase
    {


        private readonly IRepairOrderService _service;
        private readonly IRepairOrderPartService _partService;
        private readonly IWorkLogService _workLogService;
        private readonly IInvoiceService _invoiceService;

        public RepairOrdersController(
            IRepairOrderService service,
            IRepairOrderPartService partService,
            IWorkLogService workLogService,
            IInvoiceService invoiceService)
        {
            _service = service;
            _partService = partService;
            _workLogService = workLogService;
            _invoiceService = invoiceService;
        }



        // GET: api/repair-orders
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string sort = "date",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "Page must be positive and pageSize must be 1-100" });

            if (sort != "date" && sort != "status" && sort != "id")
                return BadRequest(new { message = "Unsupported sort field" });

            var orders = await _service.GetAllAsync();
            var query = orders.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.Status.Contains(search, StringComparison.OrdinalIgnoreCase) || o.Description.Contains(search, StringComparison.OrdinalIgnoreCase));

            query = sort switch
            {
                "date" => query.OrderBy(o => o.CreatedAt).ThenBy(x => x.Id),
                "status" => query.OrderBy(o => o.Status).ThenBy(x => x.Id),
                _ => query.OrderBy(x => x.Id)
            };

            int total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { items, total, page, pageSize, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
        }

        // GET: api/repair-orders/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RepairOrder>> GetById(int id)
        {
            var order = await _service.GetByIdAsync(id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // GET: api/repair-orders/car/1
        [HttpGet("car/{carId:int}")]
        public async Task<ActionResult<List<RepairOrder>>> GetByCarId(int carId)
        {
            var orders = await _service.GetByCarIdAsync(carId);

            return Ok(orders);
        }


        // POST: api/repair-orders
        [HttpPost]
        public async Task<ActionResult<RepairOrder>> Create(
            [FromBody] RepairOrder order)
        {
            try
            {
                var createdOrder = await _service.CreateAsync(order);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdOrder.Id },
                    createdOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // PUT: api/repair-orders/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] RepairOrder order)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, order);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // DELETE: api/repair-orders/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }


        // POST: api/repair-orders/1/parts
        [HttpPost("{id:int}/parts")]
        public async Task<IActionResult> AddPart(
            int id,
            [FromQuery] int partId,
            [FromQuery] int quantity)
        {
            try
            {
                var result = await _partService.AddToOrderAsync(
                    id, partId, quantity);

                return Created(
                    $"/api/repair-orders/{id}/parts",
                    new
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

        // GET: api/repair-orders/1/parts
        [HttpGet("{id:int}/parts")]
        public async Task<IActionResult> GetParts(int id)
        {
            var order = await _service.GetByIdAsync(id);

            if (order == null)
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


        // POST: api/repair-orders/1/worklogs
        [HttpPost("{id:int}/worklogs")]
        public async Task<IActionResult> AddWorkLog(
            int id,
            [FromQuery] string mechanicUserId,
            [FromQuery] string description,
            [FromQuery] decimal hoursWorked,
            [FromQuery] decimal hourlyRate)
        {
            var workLog = new WorkLog
            {
                RepairOrderId = id,
                MechanicUserId = mechanicUserId,
                Description = description,
                HoursWorked = hoursWorked,
                HourlyRate = hourlyRate
            };

            try
            {
                var result = await _workLogService.CreateAsync(workLog);

                return Created(
                    $"/api/repair-orders/{id}/worklogs",
                    new
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


        // GET: api/repair-orders/1/worklogs
        [HttpGet("{id:int}/worklogs")]
        public async Task<IActionResult> GetWorkLogs(int id)
        {
            var order = await _service.GetByIdAsync(id);

            if (order == null)
                return NotFound();

            var workLogs = await _workLogService.GetByOrderIdAsync(id);

            return Ok(workLogs.Select(w => new
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




        // GET: api/repair-orders/1/invoice
        [HttpGet("{id:int}/invoice")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _invoiceService.GetInvoiceAsync(id);

            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }

    }
}
