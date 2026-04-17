using AmarTools.InvoiceGenerator.Models;

namespace AmarTools.InvoiceGenerator.Services
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(InvoiceViewModel model);
    }
}