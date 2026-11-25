using AutoService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers
{
    [ApiController]
    [Authorize(Roles = "Service")]
    [Route("api/cars")]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MaintenanceController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/cars/service-due?days=14
        [HttpGet("service-due")]
        public async Task<IActionResult> ServiceDue([FromQuery] int days = 14)
        {
            if (days < 1 || days > 365)
                return BadRequest(new { message = "Days must be between 1 and 365" });

            var until = DateTime.UtcNow.Date.AddDays(days);

            var cars = await _db.Cars.AsNoTracking()
                .Where(c => c.NextServiceDate.HasValue && c.NextServiceDate.Value <= until)
                .OrderBy(c => c.NextServiceDate)
                .Select(c => new { id = c.Id, c.NextServiceDate })
                .ToListAsync();

            return Ok(cars);
        }
    }
}
