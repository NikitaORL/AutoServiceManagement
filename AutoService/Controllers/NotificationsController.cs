using AutoService.Core.Entities;
using AutoService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers
{
    [ApiController]
    [Authorize(Roles = "Service")]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NotificationsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("pending-ready")]
        public async Task<IActionResult> PendingReady()
        {
            var orderIds = await _db.RepairOrders.AsNoTracking()
                .Where(o => o.Status == "Completed")
                .Select(o => o.Id)
                .ToListAsync();

            var notifiedKeys = await _db.Notifications.AsNoTracking()
                .Where(n => n.Key.StartsWith("ready-"))
                .Select(n => n.Key)
                .ToListAsync();

            var notified = notifiedKeys.ToHashSet();
            var pending = orderIds.Where(id => !notified.Contains($"ready-{id}"))
                .Select(id => new { id });

            return Ok(pending);
        }

        [HttpPost("order-ready/{id:int}")]
        public async Task<IActionResult> OrderReady(int id)
        {
            var order = await _db.RepairOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.Status != "Completed")
                return BadRequest(new { message = "Order is not completed" });

            var car = await _db.Cars.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == order.CarId);
            if (car == null) return NotFound();

            string key = $"ready-{id}";
            return await SaveNotification(key, car.CustomerId, $"Repair order {id} is completed.");
        }

        [HttpPost("maintenance/{carId:int}")]
        public async Task<IActionResult> Maintenance(int carId)
        {
            var car = await _db.Cars.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null) return NotFound();

            if (car.NextServiceDate == null || car.NextServiceDate.Value.Date > DateTime.UtcNow.Date.AddDays(14))
                return BadRequest(new { message = "No maintenance due within 14 days" });

            string key = $"maintenance-{carId}-{car.NextServiceDate.Value:yyyyMMdd}";
            return await SaveNotification(key, car.CustomerId,
                $"Maintenance for {car.Brand} {car.Model} is due on {car.NextServiceDate.Value:yyyy-MM-dd}.");
        }

        private async Task<IActionResult> SaveNotification(string key, int customerId, string message)
        {
            if (await _db.Notifications.AnyAsync(n => n.Key == key))
                return Ok(new { message = "Already notified" });

            _db.Notifications.Add(new Notification
            {
                Key = key,
                CustomerId = customerId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // The unique Key index makes parallel Worker retries idempotent.
                if (await _db.Notifications.AsNoTracking().AnyAsync(n => n.Key == key))
                    return Ok(new { message = "Already notified" });
                throw;
            }

            return Ok(new { message = "Notification created" });
        }
    }
}
