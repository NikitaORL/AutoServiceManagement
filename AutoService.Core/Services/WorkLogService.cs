
using AutoService.Core.Entities;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class WorkLogService : IWorkLogService
    {
        private readonly IWorkLogRepository _repository;
        private readonly IRepairOrderRepository _orderRepository;

        public WorkLogService(
            IWorkLogRepository repository,
            IRepairOrderRepository orderRepository)
        {
            _repository = repository;
            _orderRepository = orderRepository;
        }

        public async Task<List<WorkLog>> GetByOrderIdAsync(
            int repairOrderId)
        {
            return await _repository.GetByOrderIdAsync(repairOrderId);
        }

        public async Task<WorkLog?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<WorkLog> CreateAsync(WorkLog workLog)
        {
            var order = await _orderRepository.GetByIdAsync(
                workLog.RepairOrderId);

            if (order == null)
                throw new ArgumentException("Repair order not found");

            if (string.IsNullOrWhiteSpace(workLog.Description))
                throw new ArgumentException("Description is required");

            if (string.IsNullOrWhiteSpace(workLog.MechanicUserId))
                throw new ArgumentException("Mechanic ID is required");

            if (workLog.HoursWorked <= 0)
                throw new ArgumentException("Hours must be positive");

            if (workLog.HourlyRate <= 0)
                throw new ArgumentException("Hourly rate must be positive");

            workLog.Id = 0;
            workLog.WorkDate = DateTime.UtcNow;

            await _repository.AddAsync(workLog);

            return workLog;
        }
    }
}
