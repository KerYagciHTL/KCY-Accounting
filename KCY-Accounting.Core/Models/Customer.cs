namespace KCY_Accounting.Core.Models;

/// <summary>
/// Represents a customer (Speditionskunde) with all master data,
/// billing information and optional freight payer details.
/// </summary>
public class Customer
{
    public int Id { get; set; }

    /// <summary>Auto-generated customer number, e.g. "KD-00001".</summary>
    public string CustomerNumber { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Address
    public string Street { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Billing
    public string VatNumber { get; set; } = string.Empty;
    public int PaymentTermDays { get; set; } = 30;
    public string Currency { get; set; } = "EUR";

    // Freight payer (Frachtzahler) – can differ from invoice recipient
    public string? FreightPayerName { get; set; }
    public string? FreightPayerVatNumber { get; set; }

    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation: all orders belonging to this customer
    public ICollection<TransportOrder> TransportOrders { get; set; } = new List<TransportOrder>();
}

