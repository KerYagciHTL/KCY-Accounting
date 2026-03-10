namespace KCY_Accounting.Core.Models;

/// <summary>
/// Type of document attached to a transport order.
/// </summary>
public enum DocumentType
{
    Cmr = 0,            // CMR Frachtbrief
    DeliveryNote = 1,   // Lieferschein
    Invoice = 2,        // Rechnung
    Photo = 3           // Foto
}

