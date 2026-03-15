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

    // ── Dimensions in centimetres ─────────────────────────────────────────
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm  { get; set; }
    public decimal? HeightCm { get; set; }

    /// <summary>Weight in kg for this single line item (one piece).</summary>
    public decimal? WeightKgPerUnit { get; set; }

    /// <summary>Optional description (e.g. "Europalette", "Maschinenteil").</summary>
    public string Description { get; set; } = string.Empty;

    // ── Computed helpers (not stored in DB) ──────────────────────────────
    /// <summary>Total weight = Quantity × WeightKgPerUnit.</summary>
    public decimal TotalWeightKg => Quantity * (WeightKgPerUnit ?? 0m);

    /// <summary>Volume in m³ = Quantity × (L×W×H) / 1_000_000.</summary>
    public decimal? VolumeCbm =>
        LengthCm.HasValue && WidthCm.HasValue && HeightCm.HasValue
            ? Math.Round(Quantity * LengthCm.Value * WidthCm.Value * HeightCm.Value / 1_000_000m, 4)
            : null;
}

