namespace KCY_Accounting.Core.Models;

/// <summary>
/// Represents the freight type of a transport order.
/// </summary>
public enum FreightType
{
    EuroPalletWithExchange    = 0, // Euro-Pal mit Tausch
    EuroPalletWithoutExchange = 1, // Euro-Pal ohne Tausch

    // Full Truck Load – standard trailer (13.6 LDM, auto-weight 24 000 kg)
    FtlStandard               = 2,

    // Full Truck Load – mega trailer (13.6 LDM, higher loading height)
    FtlMegatrailer            = 3,

    // Less Than Truckload – partial load, dimensions (L × W × H) required
    Ltl                       = 4
}

