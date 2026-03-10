namespace KCY_Accounting.Core.Models;

/// <summary>
/// Order status lifecycle for a transport order.
/// </summary>
public enum OrderStatus
{
    New = 0,        // Neu
    Assigned = 1,   // Beauftragt
    InTransit = 2,  // Unterwegs
    Delivered = 3,  // Zugestellt
    Invoiced = 4    // Abgerechnet
}

