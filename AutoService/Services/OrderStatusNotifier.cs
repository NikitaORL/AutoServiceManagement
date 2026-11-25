using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Hubs;
using AutoService.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services
{
    public class OrderStatusNotifier : IOrderStatusNotifier
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<OrderHub> _hub;
        private readonly ILogger<OrderStatusNotifier> _logger;

        public OrderStatusNotifier(
            AppDbContext db,
            IHubContext<OrderHub> hub,
            ILogger<OrderStatusNotifier> logger)
        {
            _db = db;
            _hub = hub;
            _logger = logger;
        }

        public async Task NotifyStatusChangedAsync(RepairOrder order)
        {
            var customerId = await _db.Cars.AsNoTracking()
                .Where(c => c.Id == order.CarId)
                .Select(c => (int?)c.CustomerId)
                .FirstOrDefaultAsync();

            if (customerId == null)
                return;

            try
            {
                await _hub.Clients.Group($"customer-{customerId}")
                    .SendAsync("OrderStatusChanged", new
                    {
                        orderId = order.Id,
                        order.CarId,
                        order.Status,
                        order.CompletedAt
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR notification failed for repair order {OrderId}", order.Id);
            }
        }
    }
}
