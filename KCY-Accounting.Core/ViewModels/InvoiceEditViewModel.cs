using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

public partial class InvoiceEditViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ITransportOrderRepository _orderRepo;
    private readonly IPdfService _pdf;
    private readonly MainViewModel _shell;
    private readonly Invoice _invoice;

    public bool IsEditMode { get; }
    public string Title => IsEditMode ? "Rechnung bearbeiten" : "Neue Rechnung";

    [ObservableProperty] private string _invoiceNumber = string.Empty;
    [ObservableProperty] private ObservableCollection<TransportOrder> _availableOrders = new();
    [ObservableProperty] private TransportOrder? _selectedOrder;
    [ObservableProperty] private InvoiceType _invoiceType = InvoiceType.CustomerInvoice;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private decimal _vatRate = 20m;
    [ObservableProperty] private string _currency = "EUR";
    [ObservableProperty] private DateTimeOffset? _issuedAt = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset? _dueDate = DateTimeOffset.Now.AddDays(30);
    [ObservableProperty] private string _carrierInvoiceNumber = string.Empty;
    [ObservableProperty] private bool _isPaid;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    // Profit display – read from the selected order
    [ObservableProperty] private decimal _orderProfit;

    public IEnumerable<InvoiceType> InvoiceTypeOptions => Enum.GetValues<InvoiceType>();

    /// <summary>Plain string list – avoids Avalonia returning ComboBoxItem.ToString() instead of the value.</summary>
    public IReadOnlyList<string> CurrencyOptions { get; } = new[] { "EUR", "USD", "CHF", "GBP" };

    public InvoiceEditViewModel(
        IInvoiceRepository invoiceRepo,
        ITransportOrderRepository orderRepo,
        IPdfService pdf,
        MainViewModel shell,
        Invoice? existing,
        TransportOrder? preselectedOrder)
    {
        _invoiceRepo = invoiceRepo;
        _orderRepo   = orderRepo;
        _pdf         = pdf;
        _shell       = shell;
        IsEditMode   = existing != null;
        _invoice     = existing ?? new Invoice();
        _preselectedOrder = preselectedOrder;
    }

    private readonly TransportOrder? _preselectedOrder;

    public async Task InitAsync()
    {
        var orders = await _orderRepo.GetAllAsync();
        AvailableOrders = new ObservableCollection<TransportOrder>(orders);

        if (IsEditMode)
        {
            PopulateFields(_invoice);
        }
        else
        {
            InvoiceNumber = await _invoiceRepo.GetNextInvoiceNumberAsync();
            if (_preselectedOrder != null)
                SelectedOrder = AvailableOrders.FirstOrDefault(o => o.Id == _preselectedOrder.Id);
        }
    }

    partial void OnSelectedOrderChanged(TransportOrder? value)
    {
        if (value == null) return;

        // Always update the profit display
        OrderProfit = value.SalePrice - value.PurchasePrice;

        // Only auto-fill fields when creating a new invoice (not editing)
        if (IsEditMode) return;

        // Auto-fill amount based on invoice type:
        // CustomerInvoice → SalePrice, CarrierCost → PurchasePrice
        Amount = InvoiceType == InvoiceType.CustomerInvoice
            ? value.SalePrice
            : value.PurchasePrice;

        // Copy VAT rate from the transport order (e.g. 20 % for Austria, 0 % for international)
        VatRate = value.VatRate;

        // Copy currency from the order
        Currency = value.Currency;

        // Copy payment terms from the linked customer if available
        if (value.Customer is { PaymentTermDays: > 0 } customer)
            DueDate = DateTimeOffset.Now.AddDays(customer.PaymentTermDays);
    }

    /// <summary>When invoice type changes, re-fill the amount from the selected order.</summary>
    partial void OnInvoiceTypeChanged(InvoiceType value)
    {
        if (IsEditMode || SelectedOrder == null) return;

        Amount = value == InvoiceType.CustomerInvoice
            ? SelectedOrder.SalePrice
            : SelectedOrder.PurchasePrice;
    }

    private void PopulateFields(Invoice i)
    {
        InvoiceNumber         = i.InvoiceNumber;
        SelectedOrder         = AvailableOrders.FirstOrDefault(o => o.Id == i.TransportOrderId);
        InvoiceType           = i.Type;
        Amount                = i.Amount;
        VatRate               = i.VatRate;
        Currency              = i.Currency;
        IssuedAt              = i.IssuedAt == default ? DateTimeOffset.Now : new DateTimeOffset(i.IssuedAt, TimeSpan.Zero);
        DueDate               = i.DueDate  == default ? DateTimeOffset.Now.AddDays(30) : new DateTimeOffset(i.DueDate, TimeSpan.Zero);
        CarrierInvoiceNumber  = i.CarrierInvoiceNumber ?? string.Empty;
        IsPaid                = i.IsPaid;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (SelectedOrder == null)
        {
            ErrorMessage = "Bitte einen Auftrag auswählen.";
            return;
        }

        _invoice.InvoiceNumber        = InvoiceNumber;
        _invoice.TransportOrderId     = SelectedOrder.Id;
        _invoice.Type                 = InvoiceType;
        _invoice.Amount               = Amount;
        _invoice.VatRate              = VatRate;
        _invoice.Currency             = Currency;
        _invoice.IssuedAt             = IssuedAt?.DateTime ?? DateTime.Today;
        _invoice.DueDate              = DueDate?.DateTime  ?? DateTime.Today.AddDays(30);
        _invoice.CarrierInvoiceNumber = string.IsNullOrWhiteSpace(CarrierInvoiceNumber) ? null : CarrierInvoiceNumber;
        _invoice.IsPaid               = IsPaid;

        if (IsEditMode)
            await _invoiceRepo.UpdateAsync(_invoice);
        else
            await _invoiceRepo.AddAsync(_invoice);

        _shell.NavigateToInvoices();
    }

    /// <summary>
    /// Saves the invoice (if new) and immediately opens the PDF preview.
    /// </summary>
    [RelayCommand]
    private async Task SaveAndPrint()
    {
        if (SelectedOrder == null)
        {
            ErrorMessage = "Bitte einen Auftrag auswählen.";
            return;
        }

        // Persist first so the invoice has a valid ID
        await Save();

        StatusMessage = "PDF wird erstellt…";
        try
        {
            // Reload order with customer navigation property
            var order = await _orderRepo.GetByIdAsync(_invoice.TransportOrderId);
            if (order?.Customer == null)
            {
                StatusMessage = "Kunde zum Auftrag nicht gefunden.";
                return;
            }

            var filePath = await _pdf.GenerateInvoicePdfAsync(_invoice, order, order.Customer);
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
    private void Cancel() => _shell.NavigateToInvoices();
}


