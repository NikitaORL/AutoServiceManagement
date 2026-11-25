using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Core.Services;
using Moq;
using Xunit;

namespace AutoService.Tests
{
    public class RepairOrderServiceTests
    {
        private static (
            RepairOrderService service,
            Mock<IRepairOrderRepository> repository,
            Mock<IOrderStatusNotifier> notifier) Create()
        {
            var existing = new RepairOrder
            {
                Id = 1,
                CarId = 1,
                Description = "Brake repair",
                Status = "Pending"
            };

            var repository = new Mock<IRepairOrderRepository>();
            repository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);

            var cars = new Mock<ICarRepository>();
            cars.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Car { Id = 1, Brand = "VW" });

            var notifier = new Mock<IOrderStatusNotifier>();
            var service = new RepairOrderService(
                repository.Object, cars.Object, notifier.Object);

            return (service, repository, notifier);
        }

        [Fact]
        public async Task ChangingStatusSendsNotification()
        {
            var (service, repository, notifier) = Create();
            var request = new RepairOrder
            {
                Description = "Brake repair",
                Status = "Completed"
            };

            var updated = await service.UpdateAsync(1, request);

            Assert.True(updated);
            repository.Verify(x => x.UpdateAsync(It.IsAny<RepairOrder>()), Times.Once);
            notifier.Verify(
                x => x.NotifyStatusChangedAsync(It.Is<RepairOrder>(r => r.Status == "Completed")),
                Times.Once);
        }

        [Fact]
        public async Task SameStatusDoesNotSendNotification()
        {
            var (service, _, notifier) = Create();

            await service.UpdateAsync(1, new RepairOrder
            {
                Description = "Brake repair",
                Status = "Pending"
            });

            notifier.Verify(
                x => x.NotifyStatusChangedAsync(It.IsAny<RepairOrder>()),
                Times.Never);
        }

        [Fact]
        public async Task InvalidStatusIsRejected()
        {
            var (service, repository, _) = Create();

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(
                1, new RepairOrder { Description = "Brake repair", Status = "Unknown" }));

            repository.Verify(x => x.UpdateAsync(It.IsAny<RepairOrder>()), Times.Never);
        }

        [Fact]
        public async Task CompletedStatusSetsCompletionDate()
        {
            var (service, repository, _) = Create();

            await service.UpdateAsync(1, new RepairOrder
            {
                Description = "Brake repair",
                Status = "Completed"
            });

            repository.Verify(x => x.UpdateAsync(
                It.Is<RepairOrder>(r => r.CompletedAt != null)),
                Times.Once);
        }

        [Fact]
        public async Task MissingCarPreventsOrderCreation()
        {
            var repository = new Mock<IRepairOrderRepository>();
            var cars = new Mock<ICarRepository>();
            var notifier = new Mock<IOrderStatusNotifier>();

            var service = new RepairOrderService(
                repository.Object, cars.Object, notifier.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(new RepairOrder
                {
                    CarId = 999, Description = "Repair"
                }));

            repository.Verify(x => x.AddAsync(It.IsAny<RepairOrder>()), Times.Never);
        }
    }
}
