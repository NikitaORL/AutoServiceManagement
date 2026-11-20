
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class RepairOrderPartService : IRepairOrderPartService
    {
        private readonly IRepairOrderPartRepository _repository;

        public RepairOrderPartService(
            IRepairOrderPartRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RepairOrderPart>> GetByOrderIdAsync(
            int repairOrderId)
        {
            if (repairOrderId <= 0)
                throw new ArgumentException("Invalid repair order ID");

            return await _repository.GetByOrderIdAsync(repairOrderId);
        }

        public async Task<RepairOrderPart> AddToOrderAsync(
            int repairOrderId, int partId, int quantity)
        {
            if (repairOrderId <= 0)
                throw new ArgumentException("Invalid repair order ID");

            if (partId <= 0)
                throw new ArgumentException("Invalid part ID");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            return await _repository.AddToOrderAsync(
                repairOrderId, partId, quantity);
        }
    }
}
