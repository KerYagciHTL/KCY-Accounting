using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.Interfaces;

/// <summary>
/// Abstraction for PDF generation. Implementations live in Infrastructure
/// so the Core layer stays free of any rendering dependencies.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a customer invoice PDF and returns the full file path of the saved document.
    /// </summary>
    Task<string> GenerateInvoicePdfAsync(Invoice invoice, TransportOrder order, Customer customer);
}

