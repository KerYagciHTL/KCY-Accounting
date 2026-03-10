namespace KCY_Accounting.Core.Models;

/// <summary>
/// Value object representing a loading or unloading location.
/// Stored as an owned entity in EF Core (no separate table, columns embedded in TransportOrders).
/// </summary>
public class TransportStop
{
    public string CompanyOrPersonName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    /// <summary>Earliest date/time (from). Optional – not required.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Latest date/time (to). Optional – not required.</summary>
    public DateTime? DateTo { get; set; }

    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    /// <summary>Loading/unloading reference number. Optional.</summary>
    public string? Reference { get; set; }
}

