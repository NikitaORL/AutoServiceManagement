
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Infrastructure.Repositories
{
    public class RepairOrderPartRepository : IRepairOrderPartRepository
    {
        private readonly AppDbContext _context;

        public RepairOrderPartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RepairOrderPart>> GetByOrderIdAsync(
            int repairOrderId)
        {
            return await _context.RepairOrderParts
                .AsNoTracking()
                .Where(p => p.RepairOrderId == repairOrderId)
                .ToListAsync();
        }

        public async Task<RepairOrderPart> AddToOrderAsync(
            int repairOrderId, int partId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero");

            var order = await _context.RepairOrders
                .FindAsync(repairOrderId);

            if (order == null)
                throw new ArgumentException("Repair order not found");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var part = await _context.Parts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == partId);

            if (part == null)
                throw new ArgumentException("Part not found");

            int updated = await _context.Parts
                .Where(p => p.Id == partId &&
                            p.StockQuantity >= quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        p => p.StockQuantity,
                        p => p.StockQuantity - quantity));

            if (updated == 0)
                throw new InvalidOperationException(
                    "Not enough parts in stock");

            var orderPart = new RepairOrderPart
            {
                RepairOrderId = repairOrderId,
                PartId = partId,
                Quantity = quantity,
                UnitPrice = part.Price
            };

            await _context.RepairOrderParts.AddAsync(orderPart);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return orderPart;
        }
    }
}
