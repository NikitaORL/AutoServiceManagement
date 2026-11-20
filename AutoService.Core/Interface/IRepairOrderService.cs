
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IRepairOrderService
    {
        Task<List<RepairOrder>> GetAllAsync();

        Task<RepairOrder?> GetByIdAsync(int id);

        Task<List<RepairOrder>> GetByCarIdAsync(int carId);

        Task<RepairOrder> CreateAsync(RepairOrder order);

        Task<bool> UpdateAsync(int id, RepairOrder order);

        Task<bool> DeleteAsync(int id);
    }
}
