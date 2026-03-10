using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

public partial class TransportOrderEditViewModel : ViewModelBase
{
    private readonly ITransportOrderRepository _orderRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly ICarrierRepository _carrierRepo;
    private readonly IDocumentRepository _docRepo;
    private readonly MainViewModel _shell;
    private readonly TransportOrder _order;

    public bool IsEditMode { get; }
    public string Title => IsEditMode ? "Auftrag bearbeiten" : "Neuer Transportauftrag";

    // ---- General ----
    [ObservableProperty] private string _orderNumber = string.Empty;
    [ObservableProperty] private DateTimeOffset? _orderDate = DateTimeOffset.Now;
    [ObservableProperty] private string _customerReference = string.Empty;
    [ObservableProperty] private ObservableCollection<Customer> _availableCustomers = new();
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private OrderStatus _status = OrderStatus.New;

    // ---- Loading point (Ladestelle) ----
    [ObservableProperty] private string _loadCompany = string.Empty;
    [ObservableProperty] private string _loadStreet = string.Empty;
    [ObservableProperty] private string _loadZip = string.Empty;
    [ObservableProperty] private string _loadCity = string.Empty;
    [ObservableProperty] private string _loadCountry = string.Empty;
    [ObservableProperty] private DateTimeOffset? _loadDateFrom;
    [ObservableProperty] private DateTimeOffset? _loadDateTo;
    [ObservableProperty] private string _loadContact = string.Empty;
    [ObservableProperty] private string _loadPhone = string.Empty;
    [ObservableProperty] private string _loadReference = string.Empty;

    // ---- Unloading point (Entladestelle) ----
    [ObservableProperty] private string _unloadCompany = string.Empty;
    [ObservableProperty] private string _unloadStreet = string.Empty;
    [ObservableProperty] private string _unloadZip = string.Empty;
    [ObservableProperty] private string _unloadCity = string.Empty;
    [ObservableProperty] private string _unloadCountry = string.Empty;
    [ObservableProperty] private DateTimeOffset? _unloadDateFrom;
    [ObservableProperty] private DateTimeOffset? _unloadDateTo;
    [ObservableProperty] private string _unloadContact = string.Empty;
    [ObservableProperty] private string _unloadPhone = string.Empty;
    [ObservableProperty] private string _unloadReference = string.Empty;

    // ---- Freight ----
    [ObservableProperty] private string _goodsDescription = string.Empty;
    [ObservableProperty] private decimal? _weightKg;
    [ObservableProperty] private int? _palletCount;
    [ObservableProperty] private decimal? _loadingMeters;
    [ObservableProperty] private FreightType _freightType = FreightType.EuroPalletWithoutExchange;
    [ObservableProperty] private bool _isHazardousGoods;

    // ---- Carrier ----
    [ObservableProperty] private ObservableCollection<Carrier> _availableCarriers = new();
    [ObservableProperty] private Carrier? _selectedCarrier;
    [ObservableProperty] private string _licensePlate = string.Empty;
    [ObservableProperty] private string _driverName = string.Empty;

    // ---- Pricing ----
    [ObservableProperty] private decimal _salePrice;
    [ObservableProperty] private decimal _purchasePrice;
    [ObservableProperty] private string _currency = "EUR";

    // ---- Documents ----
    [ObservableProperty] private ObservableCollection<OrderDocument> _documents = new();

    [ObservableProperty] private string _errorMessage = string.Empty;

    public IEnumerable<OrderStatus> StatusOptions => Enum.GetValues<OrderStatus>();
    public IEnumerable<FreightType> FreightTypeOptions => Enum.GetValues<FreightType>();

    /// <summary>Plain string list – avoids Avalonia returning ComboBoxItem.ToString() instead of the value.</summary>
    public IReadOnlyList<string> CurrencyOptions { get; } = new[] { "EUR", "USD", "CHF", "GBP" };

    /// <summary>Profit computed in real time from sale and purchase price.</summary>
    public decimal Profit => SalePrice - PurchasePrice;

    partial void OnSalePriceChanged(decimal value) => OnPropertyChanged(nameof(Profit));
    partial void OnPurchasePriceChanged(decimal value) => OnPropertyChanged(nameof(Profit));

    public TransportOrderEditViewModel(
        ITransportOrderRepository orderRepo,
        ICustomerRepository customerRepo,
        ICarrierRepository carrierRepo,
        IDocumentRepository docRepo,
        MainViewModel shell,
        TransportOrder? existing)
    {
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
        _carrierRepo = carrierRepo;
        _docRepo = docRepo;
        _shell = shell;
        IsEditMode = existing != null;
        _order = existing ?? new TransportOrder();
    }

    public async Task InitAsync()
    {
        var customers = await _customerRepo.GetActiveAsync();
        AvailableCustomers = new ObservableCollection<Customer>(customers);
        var carriers = await _carrierRepo.GetAllAsync();
        AvailableCarriers = new ObservableCollection<Carrier>(carriers);

        if (IsEditMode)
        {
            var full = await _orderRepo.GetWithDetailsAsync(_order.Id);
            if (full != null)
            {
                PopulateFields(full);
                Documents = new ObservableCollection<OrderDocument>(full.Documents);
            }
        }
        else
        {
            OrderNumber = await _orderRepo.GetNextOrderNumberAsync();
        }
    }

    private void PopulateFields(TransportOrder o)
    {
        OrderNumber = o.OrderNumber;
        OrderDate = o.OrderDate == default ? DateTimeOffset.Now : new DateTimeOffset(o.OrderDate, TimeSpan.Zero);
        CustomerReference = o.CustomerReference ?? string.Empty;
        SelectedCustomer = AvailableCustomers.FirstOrDefault(c => c.Id == o.CustomerId);
        Status = o.Status;

        var lp = o.LoadingPoint;
        LoadCompany = lp.CompanyOrPersonName;
        LoadStreet = lp.Street;
        LoadZip = lp.ZipCode;
        LoadCity = lp.City;
        LoadCountry = lp.Country;
        LoadDateFrom = lp.DateFrom.HasValue ? new DateTimeOffset(lp.DateFrom.Value, TimeSpan.Zero) : null;
        LoadDateTo = lp.DateTo.HasValue ? new DateTimeOffset(lp.DateTo.Value, TimeSpan.Zero) : null;
        LoadContact = lp.ContactPerson;
        LoadPhone = lp.Phone;
        LoadReference = lp.Reference ?? string.Empty;

        var up = o.UnloadingPoint;
        UnloadCompany = up.CompanyOrPersonName;
        UnloadStreet = up.Street;
        UnloadZip = up.ZipCode;
        UnloadCity = up.City;
        UnloadCountry = up.Country;
        UnloadDateFrom = up.DateFrom.HasValue ? new DateTimeOffset(up.DateFrom.Value, TimeSpan.Zero) : null;
        UnloadDateTo = up.DateTo.HasValue ? new DateTimeOffset(up.DateTo.Value, TimeSpan.Zero) : null;
        UnloadContact = up.ContactPerson;
        UnloadPhone = up.Phone;
        UnloadReference = up.Reference ?? string.Empty;

        GoodsDescription = o.GoodsDescription;
        WeightKg = o.WeightKg;
        PalletCount = o.PalletCount;
        LoadingMeters = o.LoadingMeters;
        FreightType = o.FreightType;
        IsHazardousGoods = o.IsHazardousGoods;

        SelectedCarrier = AvailableCarriers.FirstOrDefault(c => c.Id == o.CarrierId);
        LicensePlate = o.LicensePlate ?? string.Empty;
        DriverName = o.DriverName ?? string.Empty;

        SalePrice = o.SalePrice;
        PurchasePrice = o.PurchasePrice;
        Currency = o.Currency;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedCustomer == null)
        {
            ErrorMessage = "Bitte einen Kunden auswählen.";
            return;
        }

        _order.OrderNumber = OrderNumber;
        _order.OrderDate = OrderDate?.DateTime ?? DateTime.Today;
        _order.CustomerId = SelectedCustomer.Id;
        _order.CustomerReference = string.IsNullOrWhiteSpace(CustomerReference) ? null : CustomerReference;
        _order.Status = Status;

        _order.LoadingPoint = new TransportStop
        {
            CompanyOrPersonName = LoadCompany,
            Street = LoadStreet, ZipCode = LoadZip, City = LoadCity, Country = LoadCountry,
            DateFrom = LoadDateFrom?.DateTime, DateTo = LoadDateTo?.DateTime,
            ContactPerson = LoadContact, Phone = LoadPhone,
            Reference = string.IsNullOrWhiteSpace(LoadReference) ? null : LoadReference
        };

        _order.UnloadingPoint = new TransportStop
        {
            CompanyOrPersonName = UnloadCompany,
            Street = UnloadStreet, ZipCode = UnloadZip, City = UnloadCity, Country = UnloadCountry,
            DateFrom = UnloadDateFrom?.DateTime, DateTo = UnloadDateTo?.DateTime,
            ContactPerson = UnloadContact, Phone = UnloadPhone,
            Reference = string.IsNullOrWhiteSpace(UnloadReference) ? null : UnloadReference
        };

        _order.GoodsDescription = GoodsDescription;
        _order.WeightKg = WeightKg;
        _order.PalletCount = PalletCount;
        _order.LoadingMeters = LoadingMeters;
        _order.FreightType = FreightType;
        _order.IsHazardousGoods = IsHazardousGoods;

        _order.CarrierId = SelectedCarrier?.Id;
        _order.LicensePlate = string.IsNullOrWhiteSpace(LicensePlate) ? null : LicensePlate;
        _order.DriverName = string.IsNullOrWhiteSpace(DriverName) ? null : DriverName;

        _order.SalePrice = SalePrice;
        _order.PurchasePrice = PurchasePrice;
        _order.Currency = Currency;

        if (IsEditMode)
            await _orderRepo.UpdateAsync(_order);
        else
            await _orderRepo.AddAsync(_order);

        _shell.NavigateToOrders();
    }

    [RelayCommand]
    private void Cancel() => _shell.NavigateToOrders();

    /// <summary>
    /// Delegate set by the View to open a native file picker.
    /// Keeps platform UI code out of the ViewModel.
    /// </summary>
    public Func<Task>? RequestFileUpload { get; set; }

    [RelayCommand]
    private async Task UploadDocument()
    {
        // Delegate actual file picking to the View via the injected callback.
        if (RequestFileUpload != null)
            await RequestFileUpload();
    }

    /// <summary>Called by the View after the user picks a file.</summary>
    public async Task UploadDocumentAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;
        var doc = await _docRepo.AddFileAsync(_order.Id, filePath, DocumentType.Cmr);
        Documents.Add(doc);
    }

    [RelayCommand]
    private async Task DeleteDocument(OrderDocument? doc)
    {
        if (doc == null) return;
        await _docRepo.DeleteAsync(doc.Id);
        Documents.Remove(doc);
    }
}

