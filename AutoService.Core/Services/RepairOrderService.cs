
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly IRepairOrderRepository _repository;
        private readonly ICarRepository _carRepository;

        public RepairOrderService(
            IRepairOrderRepository repository,
            ICarRepository carRepository)
        {
            _repository = repository;
            _carRepository = carRepository;
        }

        public async Task<List<RepairOrder>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<RepairOrder?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<RepairOrder>> GetByCarIdAsync(int carId)
        {
            return await _repository.GetByCarIdAsync(carId);
        }

        public async Task<RepairOrder> CreateAsync(RepairOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.Description))
                throw new ArgumentException("Description is required");

            var car = await _carRepository.GetByIdAsync(order.CarId);

            if (car == null)
                throw new ArgumentException("Car not found");

            order.Status = "Pending";
            order.CreatedAt = DateTime.UtcNow;
            order.CompletedAt = null;

            await _repository.AddAsync(order);

            return order;
        }

        public async Task<bool> UpdateAsync(int id, RepairOrder order)
        {
            var existingOrder = await _repository.GetByIdAsync(id);

            if (existingOrder == null)
                return false;

            if (string.IsNullOrWhiteSpace(order.Description))
                throw new ArgumentException("Description is required");

            if (order.Status != "Pending" &&
                order.Status != "InProgress" &&
                order.Status != "Completed")
                throw new ArgumentException("Invalid status");

            existingOrder.Description = order.Description;
            existingOrder.Status = order.Status;

            if (order.Status == "Completed")
                existingOrder.CompletedAt = DateTime.UtcNow;
            else
                existingOrder.CompletedAt = null;

            await _repository.UpdateAsync(existingOrder);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }
    }
}
