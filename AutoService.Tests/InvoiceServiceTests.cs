using AutoService.Core.Entities;
using AutoService.Core.Interfaces;
using AutoService.Core.Services;
using Moq;
using Xunit;

namespace AutoService.Tests
{
    public class InvoiceServiceTests
    {
        private static InvoiceService Create(
            List<RepairOrderPart>? parts = null,
            List<WorkLog>? workLogs = null,
            decimal vat = 0.24m,
            bool orderExists = true)
        {
            var orders = new Mock<IRepairOrderRepository>();
            orders.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(orderExists ? new RepairOrder { Id = 1 } : null);

            var partService = new Mock<IRepairOrderPartService>();
            partService.Setup(x => x.GetByOrderIdAsync(1))
                .ReturnsAsync(parts ?? new List<RepairOrderPart>());

            var logs = new Mock<IWorkLogService>();
            logs.Setup(x => x.GetByOrderIdAsync(1))
                .ReturnsAsync(workLogs ?? new List<WorkLog>());

            return new InvoiceService(orders.Object, partService.Object, logs.Object, vat);
        }

        [Fact]
        public async Task Invoice_AddsPartsAndLabor()
        {
            var service = Create(
                new List<RepairOrderPart> { new() { Quantity = 2, UnitPrice = 50m } },
                new List<WorkLog> { new() { HoursWorked = 4m, HourlyRate = 35m } });

            var result = await service.GetInvoiceAsync(1);

            Assert.NotNull(result);
            Assert.Equal(100m, result.PartsSubtotal);
            Assert.Equal(140m, result.LaborSubtotal);
            Assert.Equal(240m, result.Subtotal);
            Assert.Equal(57.60m, result.VatAmount);
            Assert.Equal(297.60m, result.Total);
        }

        [Fact]
        public async Task Invoice_EmptyOrderHasZeroTotal()
        {
            var result = await Create().GetInvoiceAsync(1);
            Assert.NotNull(result);
            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public async Task Invoice_ReturnsNullForUnknownRepair()
        {
            var result = await Create(orderExists: false).GetInvoiceAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public void Invoice_RejectsInvalidVat()
        {
            Assert.Throws<ArgumentException>(() => Create(vat: -0.01m));
            Assert.Throws<ArgumentException>(() => Create(vat: 1.01m));
        }

        [Fact]
        public async Task Invoice_RoundsVatToTwoDecimals()
        {
            var service = Create(
                new List<RepairOrderPart> { new() { Quantity = 1, UnitPrice = 0.03m } },
                vat: 0.24m);
            var result = await service.GetInvoiceAsync(1);

            Assert.NotNull(result);
            Assert.Equal(0.01m, result.VatAmount);
            Assert.Equal(0.04m, result.Total);
        }

        [Fact]
        public async Task Invoice_ZeroVatReturnsSubtotal()
        {
            var service = Create(new List<RepairOrderPart>
            {
                new() { Quantity = 2, UnitPrice = 20m }
            }, vat: 0m);

            var result = await service.GetInvoiceAsync(1);
            Assert.NotNull(result);
            Assert.Equal(40m, result.Total);
        }
    }
}
