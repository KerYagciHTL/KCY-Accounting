namespace KCY_Accounting.Core.Models;

/// <summary>
/// The central entity: a transport order connecting customer, carrier, route and pricing.
/// All other modules (documents, invoices) reference this entity via OrderId.
/// </summary>
public class TransportOrder
{
    public int Id { get; set; }

    /// <summary>Auto-generated order number, e.g. "TA-20260001".</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.Today;

    /// <summary>Customer's own reference / purchase order number.</summary>
    public string? CustomerReference { get; set; }

    // ---- Loading point (Ladestelle) ----
    /// <summary>Owned entity – embedded columns prefixed "Loading_".</summary>
    public TransportStop LoadingPoint { get; set; } = new();

    // ---- Unloading point (Entladestelle) ----
    /// <summary>Owned entity – embedded columns prefixed "Unloading_".</summary>
    public TransportStop UnloadingPoint { get; set; } = new();

    // ---- Freight details ----
    public string GoodsDescription { get; set; } = string.Empty;
    public decimal? WeightKg { get; set; }
    public int? PalletCount { get; set; }
    public decimal? LoadingMeters { get; set; }
    public FreightType FreightType { get; set; } = FreightType.EuroPalletWithoutExchange;
    public bool IsHazardousGoods { get; set; }

    // ---- LTL dimensions (only used when FreightType = Ltl) ----
    /// <summary>Cargo length in metres. Required for LTL orders.</summary>
    public decimal? LengthM { get; set; }
    /// <summary>Cargo width in metres. Required for LTL orders.</summary>
    public decimal? WidthM  { get; set; }
    /// <summary>Cargo height in metres. Required for LTL orders.</summary>
    public decimal? HeightM { get; set; }

    // ---- Pricing / VAT ----
    /// <summary>VAT rate in percent applied to the sale price. Default 20 % (Austrian standard rate).</summary>
    public decimal VatRate { get; set; } = 20m;

    /// <summary>VAT amount: SalePrice * VatRate / 100, rounded to 2 decimal places.</summary>
    public decimal VatAmount => Math.Round(SalePrice * VatRate / 100m, 2);

    /// <summary>Gross sale amount including VAT.</summary>
    public decimal GrossAmount => SalePrice + VatAmount;

    // ---- Carrier assignment ----
    public int? CarrierId { get; set; }
    public Carrier? Carrier { get; set; }
    public string? LicensePlate { get; set; }
    public string? DriverName { get; set; }

    // ---- Pricing ----
    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public string Currency { get; set; } = "EUR";

    // ---- Status ----
    public OrderStatus Status { get; set; } = OrderStatus.New;

    /// <summary>Computed profit = SalePrice - PurchasePrice. Not stored in DB.</summary>
    public decimal Profit => SalePrice - PurchasePrice;

    // Navigation
    public ICollection<OrderDocument> Documents { get; set; } = new List<OrderDocument>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

