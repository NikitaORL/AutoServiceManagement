using AutoService.Core.Entities;

namespace AutoService.Core.Interfaces
{
    public interface IOrderStatusNotifier
    {
        Task NotifyStatusChangedAsync(RepairOrder order);
    }
}
