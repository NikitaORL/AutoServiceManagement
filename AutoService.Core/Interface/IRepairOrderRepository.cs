
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IRepairOrderRepository
    {
        Task<List<RepairOrder>> GetAllAsync();

        Task<RepairOrder?> GetByIdAsync(int id);

        Task<List<RepairOrder>> GetByCarIdAsync(int carId);

        Task AddAsync(RepairOrder order);

        Task UpdateAsync(RepairOrder order);

        Task DeleteAsync(int id);
    }
}
