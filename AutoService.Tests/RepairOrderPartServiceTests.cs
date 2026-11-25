using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Core.Services;
using Moq;
using Xunit;

namespace AutoService.Tests
{
    public class RepairOrderPartServiceTests
    {
        [Fact]
        public async Task InvalidQuantityDoesNotTouchRepository()
        {
            var repo = new Mock<IRepairOrderPartRepository>();
            var service = new RepairOrderPartService(repo.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddToOrderAsync(1, 1, -5));

            repo.Verify(x => x.AddToOrderAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task StockConflictIsReportedToController()
        {
            var repo = new Mock<IRepairOrderPartRepository>();
            repo.Setup(x => x.AddToOrderAsync(1, 2, 100))
                .ThrowsAsync(new InvalidOperationException("Insufficient stock"));

            var service = new RepairOrderPartService(repo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddToOrderAsync(1, 2, 100));
        }

        [Fact]
        public async Task ValidPartIsReturned()
        {
            var expected = new RepairOrderPart
            {
                Id = 5, RepairOrderId = 1, PartId = 2, Quantity = 2, UnitPrice = 45m
            };

            var repo = new Mock<IRepairOrderPartRepository>();
            repo.Setup(x => x.AddToOrderAsync(1, 2, 2)).ReturnsAsync(expected);
            var service = new RepairOrderPartService(repo.Object);

            var result = await service.AddToOrderAsync(1, 2, 2);

            Assert.Same(expected, result);
            repo.Verify(x => x.AddToOrderAsync(1, 2, 2), Times.Once);
        }
    }
}
