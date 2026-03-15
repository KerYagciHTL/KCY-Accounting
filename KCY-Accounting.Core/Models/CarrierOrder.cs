namespace KCY_Accounting.Core.Models;
/// <summary>
/// Carrier order (Fraechterauftrag) - an outgoing order document sent to the carrier.
/// Number series: "FA-YYYY####".
/// </summary>
public class CarrierOrder
{
    public int Id { get; set; }
    public string CarrierOrderNumber { get; set; } = string.Empty;
    public int? TransportOrderId { get; set; }
    public TransportOrder? TransportOrder { get; set; }
    public int CarrierId { get; set; }
    public Carrier Carrier { get; set; } = null!;
    public decimal NetAmount { get; set; }
    public decimal VatRate { get; set; } = 0m;
    public string Currency { get; set; } = "EUR";
    public DateTime IssuedAt { get; set; } = DateTime.Today;
    public DateTime DueDate  { get; set; } = DateTime.Today.AddDays(30);
    public string GoodsDescription { get; set; } = string.Empty;
    public TransportStop LoadingPoint   { get; set; } = new();
    public TransportStop UnloadingPoint { get; set; } = new();
    public ICollection<FreightItem> FreightItems { get; set; } = new List<FreightItem>();
    public bool IsPaid { get; set; }
    public decimal VatAmount   => Math.Round(NetAmount * VatRate / 100m, 2);
    public decimal GrossAmount => NetAmount + VatAmount;
    public decimal TotalWeightKg => FreightItems.Sum(i => i.TotalWeightKg);
}
