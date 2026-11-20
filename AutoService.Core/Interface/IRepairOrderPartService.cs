
using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IRepairOrderPartService
    {
        Task<RepairOrderPart> AddToOrderAsync(
            int repairOrderId,
            int partId,
            int quantity);

        Task<List<RepairOrderPart>> GetByOrderIdAsync(
            int repairOrderId);
    }
}
