namespace KCY_Accounting.Core.Models;

/// <summary>
/// Whether the invoice is issued to the customer (outgoing) or received from the carrier (incoming cost).
/// </summary>
public enum InvoiceType
{
    CustomerInvoice = 0,  // Kundenrechnung (outgoing)
    CarrierCost = 1       // Frächterkosten (incoming)
}

/// <summary>
/// Invoice record, linked to a transport order.
/// Profit = SalePrice – PurchasePrice is calculated on the fly via the ViewModel.
/// </summary>
public class Invoice
{
    public int Id { get; set; }

    /// <summary>Auto-generated invoice number, e.g. "RE-20260001".</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public InvoiceType Type { get; set; }

    public decimal Amount { get; set; }
    public decimal VatRate { get; set; } = 20m; // % – Austrian default
    public string Currency { get; set; } = "EUR";

    public DateTime IssuedAt { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; }

    /// <summary>External carrier invoice number for carrier costs.</summary>
    public string? CarrierInvoiceNumber { get; set; }

    public bool IsPaid { get; set; }
}

