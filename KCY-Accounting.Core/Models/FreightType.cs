namespace KCY_Accounting.Core.Models;

/// <summary>
/// Represents the freight type of a transport order.
/// </summary>
public enum FreightType
{
    EuroPalletWithExchange = 0,    // Euro-Pal mit Tausch
    EuroPalletWithoutExchange = 1, // Euro-Pal ohne Tausch
    Ftl = 2                        // Full Truck Load
}

