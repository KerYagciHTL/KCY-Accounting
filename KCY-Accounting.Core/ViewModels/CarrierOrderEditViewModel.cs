using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

/// <summary>
/// Edit / create form for a single CarrierOrder (Fraechterauftrag).
/// Freight items (positions) are managed as an ObservableCollection of
/// FreightItemViewModel so the summary row recalculates in real time.
/// </summary>
public partial class CarrierOrderEditViewModel : ViewModelBase
{
    private readonly ICarrierOrderRepository _repo;
    private readonly ICarrierRepository      _carrierRepo;
    private readonly ITransportOrderRepository _orderRepo;
    private readonly IPdfService             _pdf;
    private readonly MainViewModel           _shell;
    private readonly CarrierOrder            _carrierOrder;

    public bool   IsEditMode { get; }
    public string Title      => IsEditMode ? "Frächterauftrag bearbeiten" : "Neuer Frächterauftrag";

    // ── Header fields ─────────────────────────────────────────────────────
    [ObservableProperty] private string _carrierOrderNumber = string.Empty;
    [ObservableProperty] private DateTimeOffset? _issuedAt = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset? _dueDate  = DateTimeOffset.Now.AddDays(30);
    [ObservableProperty] private bool   _isPaid;
    [ObservableProperty] private string _goodsDescription = string.Empty;

    // ── Carrier selection ─────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<Carrier> _availableCarriers = new();
    [ObservableProperty] private Carrier? _selectedCarrier;

    // ── Optional link to an existing transport order ──────────────────────
    [ObservableProperty] private ObservableCollection<TransportOrder> _availableOrders = new();
    [ObservableProperty] private TransportOrder? _linkedOrder;

    // ── Loading point ──────────────────────────────────────────────────────
    [ObservableProperty] private string _loadCompany = string.Empty;
    [ObservableProperty] private string _loadStreet  = string.Empty;
    [ObservableProperty] private string _loadZip     = string.Empty;
    [ObservableProperty] private string _loadCity    = string.Empty;
    [ObservableProperty] private string _loadCountry = string.Empty;
    [ObservableProperty] private DateTimeOffset? _loadDate;
    /// <summary>Reference number at the loading dock (Ref Nummer). Propagated from the linked transport order.</summary>
    [ObservableProperty] private string _loadReference = string.Empty;

    // ── Unloading point ────────────────────────────────────────────────────
    [ObservableProperty] private string _unloadCompany = string.Empty;
    [ObservableProperty] private string _unloadStreet  = string.Empty;
    [ObservableProperty] private string _unloadZip     = string.Empty;
    [ObservableProperty] private string _unloadCity    = string.Empty;
    [ObservableProperty] private string _unloadCountry = string.Empty;
    [ObservableProperty] private DateTimeOffset? _unloadDate;
    /// <summary>Registration number at the unloading dock (Reg Nummer). Propagated from the linked transport order.</summary>
    [ObservableProperty] private string _unloadReference = string.Empty;

    // ── Pricing ───────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _netAmount;
    [ObservableProperty] private decimal _vatRate;
    [ObservableProperty] private string  _currency = "EUR";

    // ── Freight item positions ─────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<FreightItemViewModel> _freightItems = new();

    // ── Error / status ────────────────────────────────────────────────────
    [ObservableProperty] private string _errorMessage  = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public IReadOnlyList<string> CurrencyOptions { get; } = new[] { "EUR", "USD", "CHF", "GBP" };

    // ── Computed totals (recalculated whenever NetAmount/VatRate change) ──
    public decimal VatAmount   => Math.Round(NetAmount * VatRate / 100m, 2);
    public decimal GrossAmount => NetAmount + VatAmount;

    /// <summary>Sum of all freight item weights.</summary>
    public decimal TotalWeightKg => FreightItems.Sum(i => i.TotalWeightKg);
#pragma warning disable IDE0060
    partial void OnNetAmountChanged(decimal value) { OnPropertyChanged(nameof(VatAmount)); OnPropertyChanged(nameof(GrossAmount)); }
    partial void OnVatRateChanged(decimal value)   { OnPropertyChanged(nameof(VatAmount)); OnPropertyChanged(nameof(GrossAmount)); }
#pragma warning restore IDE0060

    /// <summary>
    /// When a transport order is linked, copy its route data into the form fields
    /// so the user does not have to re-type them.
    /// </summary>
    partial void OnLinkedOrderChanged(TransportOrder? value)
    {
        if (value == null) return;

        // Copy loading point
        LoadCompany = value.LoadingPoint.CompanyOrPersonName;
        LoadStreet  = value.LoadingPoint.Street;
        LoadZip     = value.LoadingPoint.ZipCode;
        LoadCity    = value.LoadingPoint.City;
        LoadCountry = value.LoadingPoint.Country;
        if (value.LoadingPoint.DateFrom.HasValue)
            LoadDate = new DateTimeOffset(value.LoadingPoint.DateFrom.Value, TimeSpan.Zero);
        // Propagate loading reference (Ref Nummer) from transport order to carrier order
        LoadReference = value.LoadingPoint.Reference ?? string.Empty;

        // Copy unloading point
        UnloadCompany = value.UnloadingPoint.CompanyOrPersonName;
        UnloadStreet  = value.UnloadingPoint.Street;
        UnloadZip     = value.UnloadingPoint.ZipCode;
        UnloadCity    = value.UnloadingPoint.City;
        UnloadCountry = value.UnloadingPoint.Country;
        if (value.UnloadingPoint.DateFrom.HasValue)
            UnloadDate = new DateTimeOffset(value.UnloadingPoint.DateFrom.Value, TimeSpan.Zero);
        // Propagate unloading registration number (Reg Nummer) from transport order to carrier order
        UnloadReference = value.UnloadingPoint.Reference ?? string.Empty;

        // Copy goods description and carrier if not yet set
        if (string.IsNullOrWhiteSpace(GoodsDescription))
            GoodsDescription = value.GoodsDescription;

        if (SelectedCarrier == null && value.CarrierId.HasValue)
            SelectedCarrier = AvailableCarriers.FirstOrDefault(c => c.Id == value.CarrierId.Value);

        // Pre-fill net amount with the transport order's purchase price
        if (NetAmount == 0m && value.PurchasePrice > 0m)
            NetAmount = value.PurchasePrice;

        Currency = value.Currency;

        // Auto-populate a freight item from the transport order's cargo data if none exist yet
        if (FreightItems.Count == 0 && !string.IsNullOrWhiteSpace(value.GoodsDescription))
        {
            FreightItems.Add(new FreightItemViewModel
            {
                Description     = value.GoodsDescription,
                Quantity        = 1,
                WeightKgPerUnit = value.WeightKg ?? 0m,
                LengthM         = value.LengthM  ?? 0m,
                WidthM          = value.WidthM   ?? 0m,
                HeightM         = value.HeightM  ?? 0m,
            });
        }
    }

    public CarrierOrderEditViewModel(
        ICarrierOrderRepository     repo,
        ICarrierRepository          carrierRepo,
        ITransportOrderRepository   orderRepo,
        IPdfService                 pdf,
        MainViewModel               shell,
        CarrierOrder?               existing)
    {
        _repo         = repo;
        _carrierRepo  = carrierRepo;
        _orderRepo    = orderRepo;
        _pdf          = pdf;
        _shell        = shell;
        IsEditMode    = existing != null;
        _carrierOrder = existing ?? new CarrierOrder();
    }

    public async Task InitAsync()
    {
        var carriers = await _carrierRepo.GetAllAsync();
        AvailableCarriers = new ObservableCollection<Carrier>(carriers);

        var orders = await _orderRepo.GetAllAsync();
        AvailableOrders = new ObservableCollection<TransportOrder>(orders);

        if (IsEditMode)
        {
            var full = await _repo.GetWithDetailsAsync(_carrierOrder.Id);
            if (full != null) PopulateFields(full);
        }
        else
        {
            CarrierOrderNumber = await _repo.GetNextCarrierOrderNumberAsync();
        }
    }

    private void PopulateFields(CarrierOrder co)
    {
        CarrierOrderNumber = co.CarrierOrderNumber;
        IssuedAt           = co.IssuedAt == default ? DateTimeOffset.Now : new DateTimeOffset(co.IssuedAt, TimeSpan.Zero);
        DueDate            = co.DueDate  == default ? DateTimeOffset.Now.AddDays(30) : new DateTimeOffset(co.DueDate, TimeSpan.Zero);
        IsPaid             = co.IsPaid;
        GoodsDescription   = co.GoodsDescription;
        SelectedCarrier    = AvailableCarriers.FirstOrDefault(c => c.Id == co.CarrierId);
        LinkedOrder        = co.TransportOrderId.HasValue
            ? AvailableOrders.FirstOrDefault(o => o.Id == co.TransportOrderId.Value)
            : null;

        LoadCompany = co.LoadingPoint.CompanyOrPersonName;
        LoadStreet  = co.LoadingPoint.Street;
        LoadZip     = co.LoadingPoint.ZipCode;
        LoadCity    = co.LoadingPoint.City;
        LoadCountry = co.LoadingPoint.Country;
        LoadDate    = co.LoadingPoint.DateFrom.HasValue
            ? new DateTimeOffset(co.LoadingPoint.DateFrom.Value, TimeSpan.Zero) : null;
        LoadReference = co.LoadingPoint.Reference ?? string.Empty;

        UnloadCompany = co.UnloadingPoint.CompanyOrPersonName;
        UnloadStreet  = co.UnloadingPoint.Street;
        UnloadZip     = co.UnloadingPoint.ZipCode;
        UnloadCity    = co.UnloadingPoint.City;
        UnloadCountry = co.UnloadingPoint.Country;
        UnloadDate    = co.UnloadingPoint.DateFrom.HasValue
            ? new DateTimeOffset(co.UnloadingPoint.DateFrom.Value, TimeSpan.Zero) : null;
        UnloadReference = co.UnloadingPoint.Reference ?? string.Empty;

        NetAmount = co.NetAmount;
        VatRate   = co.VatRate;
        Currency  = co.Currency;

        FreightItems = new ObservableCollection<FreightItemViewModel>(
            co.FreightItems.Select(FreightItemViewModel.FromModel));
        FreightItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalWeightKg));
    }

    // ── Freight item management ───────────────────────────────────────────

    [RelayCommand]
    private void AddFreightItem()
    {
        var item = new FreightItemViewModel();
        item.PropertyChanged += (_, _) => OnPropertyChanged(nameof(TotalWeightKg));
        FreightItems.Add(item);
        OnPropertyChanged(nameof(TotalWeightKg));
    }

    [RelayCommand]
    private void RemoveFreightItem(FreightItemViewModel? item)
    {
        if (item == null) return;
        FreightItems.Remove(item);
        OnPropertyChanged(nameof(TotalWeightKg));
    }

    // ── Save / Cancel / PDF ───────────────────────────────────────────────

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedCarrier == null)
        {
            ErrorMessage = "Bitte einen Frächter auswählen.";
            return;
        }

        BuildCarrierOrder();

        if (IsEditMode)
            await _repo.UpdateAsync(_carrierOrder);
        else
            await _repo.AddAsync(_carrierOrder);

        _shell.NavigateToCarrierOrders();
    }

    [RelayCommand]
    private async Task SaveAndPrint()
    {
        if (SelectedCarrier == null)
        {
            ErrorMessage = "Bitte einen Frächter auswählen.";
            return;
        }

        BuildCarrierOrder();

        if (IsEditMode)
            await _repo.UpdateAsync(_carrierOrder);
        else
            await _repo.AddAsync(_carrierOrder);

        StatusMessage = "PDF wird erstellt...";
        try
        {
            // Reload with full navigation properties for the PDF renderer
            var full = await _repo.GetWithDetailsAsync(_carrierOrder.Id);
            if (full == null) { StatusMessage = "Auftrag nicht gefunden."; return; }

            var filePath = await _pdf.GenerateCarrierOrderPdfAsync(full);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = filePath,
                UseShellExecute = true
            });
            StatusMessage = $"PDF: {filePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim PDF-Export: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel() => _shell.NavigateToCarrierOrders();

    /// <summary>Map all ViewModel fields back onto the domain entity before persisting.</summary>
    private void BuildCarrierOrder()
    {
        _carrierOrder.CarrierOrderNumber = CarrierOrderNumber;
        _carrierOrder.CarrierId          = SelectedCarrier!.Id;
        _carrierOrder.TransportOrderId   = LinkedOrder?.Id;
        _carrierOrder.IssuedAt           = IssuedAt?.DateTime    ?? DateTime.Today;
        _carrierOrder.DueDate            = DueDate?.DateTime     ?? DateTime.Today.AddDays(30);
        _carrierOrder.IsPaid             = IsPaid;
        _carrierOrder.GoodsDescription   = GoodsDescription;
        _carrierOrder.NetAmount          = NetAmount;
        _carrierOrder.VatRate            = VatRate;
        _carrierOrder.Currency           = Currency;

        _carrierOrder.LoadingPoint = new TransportStop
        {
            CompanyOrPersonName = LoadCompany,
            Street  = LoadStreet, ZipCode = LoadZip,
            City    = LoadCity,   Country = LoadCountry,
            DateFrom  = LoadDate?.DateTime,
            Reference = string.IsNullOrWhiteSpace(LoadReference) ? null : LoadReference
        };
        _carrierOrder.UnloadingPoint = new TransportStop
        {
            CompanyOrPersonName = UnloadCompany,
            Street  = UnloadStreet, ZipCode = UnloadZip,
            City    = UnloadCity,   Country = UnloadCountry,
            DateFrom  = UnloadDate?.DateTime,
            Reference = string.IsNullOrWhiteSpace(UnloadReference) ? null : UnloadReference
        };

        // Replace freight items collection
        _carrierOrder.FreightItems = FreightItems
            .Select(fi => fi.ToModel(_carrierOrder.Id))
            .ToList();
    }
}

