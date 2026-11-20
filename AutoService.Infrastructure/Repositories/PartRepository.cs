
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Infrastructure.Repositories
{
    public class PartRepository : IPartRepository
    {
        private readonly AppDbContext _context;

        public PartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Part>> GetAllAsync()
        {
            return await _context.Parts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Part?> GetByIdAsync(int id)
        {
            return await _context.Parts.FindAsync(id);
        }

        public async Task AddAsync(Part part)
        {
            await _context.Parts.AddAsync(part);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Part part)
        {
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var part = await _context.Parts.FindAsync(id);

            if (part == null)
                return;

            bool isUsed = await _context.RepairOrderParts
                .AnyAsync(p => p.PartId == id);

            if (isUsed)
                throw new InvalidOperationException(
                    "Cannot delete a part used in repair orders.");

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
        }
    }
}
