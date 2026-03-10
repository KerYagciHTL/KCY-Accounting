namespace KCY_Accounting.Core.Models;

/// <summary>
/// A document file attached to a specific transport order.
/// Files are stored on disk; only the relative path is persisted in the DB.
/// </summary>
public class OrderDocument
{
    public int Id { get; set; }
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public DocumentType Type { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Path relative to the application's document storage root.</summary>
    public string RelativeFilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

