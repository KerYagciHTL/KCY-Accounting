namespace KCY_Accounting.Core.Models;

/// <summary>
/// A single freight line item on a carrier order.
/// Stores dimensions (L×W×H), quantity and weight.
/// The total weight is the sum of all items on the carrier order.
/// </summary>
public class FreightItem
{
    public int Id { get; set; }

    public int CarrierOrderId { get; set; }
    public CarrierOrder CarrierOrder { get; set; } = null!;

    /// <summary>Number of pieces / units for this line item.</summary>
    public int Quantity { get; set; } = 1;

    // ── Dimensions in metres ──────────────────────────────────────────────
    public decimal? LengthM { get; set; }
    public decimal? WidthM  { get; set; }
    public decimal? HeightM { get; set; }

    /// <summary>Weight in kg for this single line item (one piece).</summary>
    public decimal? WeightKgPerUnit { get; set; }

    /// <summary>Optional description (e.g. "Europalette", "Maschinenteil").</summary>
    public string Description { get; set; } = string.Empty;

    // ── Computed helpers (not stored in DB) ──────────────────────────────
    /// <summary>Total weight = Quantity × WeightKgPerUnit.</summary>
    public decimal TotalWeightKg => Quantity * (WeightKgPerUnit ?? 0m);

    /// <summary>Volume in m³ = Quantity × (L × W × H) where all values are in metres.</summary>
    public decimal? VolumeCbm =>
        LengthM.HasValue && WidthM.HasValue && HeightM.HasValue
            ? Math.Round(Quantity * LengthM.Value * WidthM.Value * HeightM.Value, 4)
            : null;
}

