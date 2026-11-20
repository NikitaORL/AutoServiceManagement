
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Infrastructure.Repositories
{
    public class RepairOrderRepository : IRepairOrderRepository
    {
        private readonly AppDbContext _context;

        public RepairOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RepairOrder>> GetAllAsync()
        {
            return await _context.RepairOrders.ToListAsync();
        }

        public async Task<RepairOrder?> GetByIdAsync(int id)
        {
            return await _context.RepairOrders.FindAsync(id);
        }

        public async Task<List<RepairOrder>> GetByCarIdAsync(int carId)
        {
            return await _context.RepairOrders
                .Where(r => r.CarId == carId)
                .ToListAsync();
        }

        public async Task AddAsync(RepairOrder order)
        {
            await _context.RepairOrders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RepairOrder order)
        {
            _context.RepairOrders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.RepairOrders.FindAsync(id);

            if (order == null)
                return;

            bool hasParts = await _context.RepairOrderParts
                .AnyAsync(p => p.RepairOrderId == id);

            bool hasWorkLogs = await _context.WorkLogs
                .AnyAsync(w => w.RepairOrderId == id);

            if (hasParts || hasWorkLogs)
                throw new InvalidOperationException(
                    "Cannot delete an order with parts or work logs.");

            _context.RepairOrders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
