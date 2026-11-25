
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IWorkLogService
    {
        Task<List<WorkLog>> GetByOrderIdAsync(int repairOrderId);

        Task<WorkLog?> GetByIdAsync(int id);

        Task<WorkLog> CreateAsync(WorkLog workLog);
    }
}
