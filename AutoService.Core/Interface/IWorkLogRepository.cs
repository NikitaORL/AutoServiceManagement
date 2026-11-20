
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IWorkLogRepository
    {
        Task<List<WorkLog>> GetByOrderIdAsync(int repairOrderId);

        Task<WorkLog?> GetByIdAsync(int id);

        Task AddAsync(WorkLog workLog);
    }
}
