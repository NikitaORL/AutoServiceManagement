
using AutoService.Core.DTO;
using AutoService.Core.Interfaces;

namespace AutoService.Core.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IRepairOrderRepository _orderRepository;
        private readonly IRepairOrderPartService _partService;
        private readonly IWorkLogService _workLogService;
        private readonly decimal _vatRate;

        public InvoiceService(
            IRepairOrderRepository orderRepository,
            IRepairOrderPartService partService,
            IWorkLogService workLogService,
            decimal vatRate)
        {
            _orderRepository = orderRepository;
            _partService = partService;
            _workLogService = workLogService;

            if (vatRate < 0 || vatRate > 1)
                throw new ArgumentException("Invalid VAT rate");

            _vatRate = vatRate;
        }

        public async Task<InvoiceDto?> GetInvoiceAsync(int repairOrderId)
        {
            var order = await _orderRepository.GetByIdAsync(repairOrderId);

            if (order == null)
                return null;

            var parts = await _partService.GetByOrderIdAsync(repairOrderId);
            var workLogs = await _workLogService.GetByOrderIdAsync(repairOrderId);

            decimal partsSubtotal = Math.Round(
                parts.Sum(p => p.Quantity * p.UnitPrice),
                2,
                MidpointRounding.AwayFromZero);

            decimal laborSubtotal = Math.Round(
                workLogs.Sum(w => w.HoursWorked * w.HourlyRate),
                2,
                MidpointRounding.AwayFromZero);

            decimal subtotal = partsSubtotal + laborSubtotal;

            decimal vatAmount = Math.Round(
                subtotal * _vatRate,
                2,
                MidpointRounding.AwayFromZero);

            decimal total = subtotal + vatAmount;

            return new InvoiceDto
            {
                RepairOrderId = repairOrderId,
                PartsSubtotal = partsSubtotal,
                LaborSubtotal = laborSubtotal,
                Subtotal = subtotal,
                VatRate = _vatRate,
                VatAmount = vatAmount,
                Total = total
            };
        }
    }
}
