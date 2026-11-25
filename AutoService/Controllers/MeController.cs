using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers
{
    [ApiController]
    [Authorize(Roles = "Customer")]
    [Route("api/me")]
    public class MeController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IInvoiceService _invoices;

        public MeController(AppDbContext db, IInvoiceService invoices)
        {
            _db = db;
            _invoices = invoices;
        }

        private int? CustomerId()
        {
            return int.TryParse(User.FindFirst("customerId")?.Value, out var id)
                ? id : null;
        }

        [HttpGet("cars")]
        public async Task<IActionResult> MyCars()
        {
            var customerId = CustomerId();
            if (customerId == null) return Forbid();

            var cars = await _db.Cars.AsNoTracking()
                .Where(c => c.CustomerId == customerId)
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.Brand, c.Model, c.Year, c.LicensePlate })
                .ToListAsync();

            return Ok(cars);
        }

        [HttpGet("repair-orders")]
        public async Task<IActionResult> MyOrders()
        {
            var customerId = CustomerId();
            if (customerId == null) return Forbid();

            var orders = await _db.RepairOrders.AsNoTracking()
                .Where(o => _db.Cars.Any(c => c.Id == o.CarId && c.CustomerId == customerId))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new { o.Id, o.CarId, o.Description, o.Status, o.CreatedAt, o.CompletedAt })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("cars/{id:int}/history")]
        public async Task<IActionResult> MyCarHistory(int id)
        {
            var customerId = CustomerId();
            if (customerId == null) return Forbid();

            var owned = await _db.Cars.AnyAsync(c => c.Id == id && c.CustomerId == customerId);
            if (!owned) return NotFound();

            var history = await _db.RepairOrders.AsNoTracking()
                .Where(o => o.CarId == id)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new { o.Id, o.CarId, o.Description, o.Status, o.CreatedAt, o.CompletedAt })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("repair-orders/{id:int}/invoice")]
        public async Task<IActionResult> MyInvoice(int id)
        {
            var customerId = CustomerId();
            if (customerId == null) return Forbid();

            bool owned = await _db.RepairOrders.AnyAsync(o => o.Id == id &&
                _db.Cars.Any(c => c.Id == o.CarId && c.CustomerId == customerId));

            if (!owned) return NotFound();

            var invoice = await _invoices.GetInvoiceAsync(id);
            if (invoice == null) return NotFound();

            return Ok(invoice);
        }
    }
}
