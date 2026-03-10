namespace KCY_Accounting.Core.Models;

/// <summary>
/// Represents a freight carrier / subcontractor (Frächter/Subunternehmer).
/// </summary>
public class Carrier
{
    public int Id { get; set; }

    /// <summary>Auto-generated carrier number, e.g. "FR-00001".</summary>
    public string CarrierNumber { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Address
    public string Street { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Bank details
    public string BankName { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
    public string Bic { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    // Navigation
    public ICollection<TransportOrder> TransportOrders { get; set; } = new List<TransportOrder>();
}

