
using AutoService.Core.DTO;

namespace AutoService.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceDto?> GetInvoiceAsync(int repairOrderId);
    }
}

