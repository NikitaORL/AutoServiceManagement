
namespace AutoService.Core.DTO
{
    public class InvoiceDto
    {
        public int RepairOrderId { get; set; }

        public decimal PartsSubtotal { get; set; }

        public decimal LaborSubtotal { get; set; }

        public decimal Subtotal { get; set; }

        public decimal VatRate { get; set; }

        public decimal VatAmount { get; set; }

        public decimal Total { get; set; }
    }
}
