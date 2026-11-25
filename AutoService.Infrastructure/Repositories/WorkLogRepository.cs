
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Infrastructure.Repositories
{
    public class WorkLogRepository : IWorkLogRepository
    {
        private readonly AppDbContext _context;

        public WorkLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkLog>> GetByOrderIdAsync(
            int repairOrderId)
        {
            return await _context.WorkLogs
                .AsNoTracking()
                .Where(w => w.RepairOrderId == repairOrderId)
                .ToListAsync();
        }

        public async Task<WorkLog?> GetByIdAsync(int id)
        {
            return await _context.WorkLogs.FindAsync(id);
        }

        public async Task AddAsync(WorkLog workLog)
        {
            await _context.WorkLogs.AddAsync(workLog);
            await _context.SaveChangesAsync();
        }
    }
}
