using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Core.Services;
using Moq;
using Xunit;

namespace AutoService.Tests
{
    public class WorkLogServiceTests
    {
        private static (WorkLogService service, Mock<IWorkLogRepository> logs) Create(bool orderExists = true)
        {
            var logs = new Mock<IWorkLogRepository>();
            var orders = new Mock<IRepairOrderRepository>();
            orders.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(orderExists ? new RepairOrder { Id = 1 } : null);
            return (new WorkLogService(logs.Object, orders.Object), logs);
        }

        private static WorkLog ValidLog() => new()
        {
            RepairOrderId = 1,
            MechanicUserId = "42",
            Description = "Replace brake pads",
            HoursWorked = 2m,
            HourlyRate = 35m
        };

        [Fact]
        public async Task ValidWorkLogIsSavedOnce()
        {
            var (service, logs) = Create();
            var item = ValidLog();

            var result = await service.CreateAsync(item);

            Assert.Same(item, result);
            logs.Verify(x => x.AddAsync(It.IsAny<WorkLog>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public async Task WorkLogRejectsNonPositiveHours(int hours)
        {
            var (service, logs) = Create();
            var item = ValidLog();
            item.HoursWorked = hours;

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(item));
            logs.Verify(x => x.AddAsync(It.IsAny<WorkLog>()), Times.Never);
        }

        [Fact]
        public async Task WorkLogRejectsZeroHourlyRate()
        {
            var (service, _) = Create();
            var item = ValidLog();
            item.HourlyRate = 0m;
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(item));
        }

        [Fact]
        public async Task WorkLogRequiresDescription()
        {
            var (service, _) = Create();
            var item = ValidLog();
            item.Description = " ";
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(item));
        }

        [Fact]
        public async Task WorkLogRequiresMechanicId()
        {
            var (service, _) = Create();
            var item = ValidLog();
            item.MechanicUserId = "";
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(item));
        }

        [Fact]
        public async Task WorkLogRequiresExistingOrder()
        {
            var (service, logs) = Create(orderExists: false);
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(ValidLog()));
            logs.Verify(x => x.AddAsync(It.IsAny<WorkLog>()), Times.Never);
        }

        [Fact]
        public async Task WorkLogSetsDateOnServer()
        {
            var (service, _) = Create();
            var item = ValidLog();
            item.WorkDate = new DateTime(2000, 1, 1);

            var result = await service.CreateAsync(item);

            Assert.True(result.WorkDate > new DateTime(2020, 1, 1));
        }
    }
}
