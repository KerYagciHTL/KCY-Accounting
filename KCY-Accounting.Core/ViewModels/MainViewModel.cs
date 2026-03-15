using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.ViewModels;

/// <summary>
/// Shell ViewModel: owns the navigation state and hosts the currently
/// displayed child view model in <see cref="CurrentView"/>.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly ICustomerRepository _customers;
    private readonly ICarrierRepository _carriers;
    private readonly ITransportOrderRepository _orders;
    private readonly IDocumentRepository _documents;
    private readonly IInvoiceRepository _invoices;
    private readonly ICarrierOrderRepository _carrierOrders;
    private readonly IPdfService _pdf;

    [ObservableProperty]
    private ViewModelBase _currentView = null!;

    [ObservableProperty]
    private string _activeNavItem = "Dashboard";

    public MainViewModel(
        ICustomerRepository customers,
        ICarrierRepository carriers,
        ITransportOrderRepository orders,
        IDocumentRepository documents,
        IInvoiceRepository invoices,
        ICarrierOrderRepository carrierOrders,
        IPdfService pdf)
    {
        _customers     = customers;
        _carriers      = carriers;
        _orders        = orders;
        _documents     = documents;
        _invoices      = invoices;
        _carrierOrders = carrierOrders;
        _pdf           = pdf;
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        ActiveNavItem = "Dashboard";
        var vm = new DashboardViewModel(_orders, this);
        CurrentView = vm;
        _ = vm.InitAsync();
    }

    [RelayCommand]
    public void NavigateToCustomers()
    {
        ActiveNavItem = "Kunden";
        var vm = new CustomerListViewModel(_customers, this);
        CurrentView = vm;
        _ = vm.LoadAsync();
    }

    [RelayCommand]
    public void NavigateToCarriers()
    {
        ActiveNavItem = "Frächter";
        var vm = new CarrierListViewModel(_carriers, this);
        CurrentView = vm;
        _ = vm.LoadAsync();
    }

    [RelayCommand]
    public void NavigateToOrders()
    {
        ActiveNavItem = "Aufträge";
        var vm = new TransportOrderListViewModel(_orders, _customers, _carriers, this);
        CurrentView = vm;
        _ = vm.LoadAsync();
    }

    [RelayCommand]
    public void NavigateToInvoices()
    {
        ActiveNavItem = "Rechnungen";
        var vm = new InvoiceListViewModel(_invoices, _orders, _pdf, this);
        CurrentView = vm;
        _ = vm.LoadAsync();
    }

    [RelayCommand]
    public void NavigateToCarrierOrders()
    {
        ActiveNavItem = "Frächteraufträge";
        var vm = new CarrierOrderListViewModel(_carrierOrders, _pdf, this);
        CurrentView = vm;
        _ = vm.LoadAsync();
    }

    public void OpenCustomerEdit(Customer? customer = null)
    {
        CurrentView = new CustomerEditViewModel(_customers, this, customer);
    }

    public void OpenCarrierEdit(Carrier? carrier = null)
    {
        CurrentView = new CarrierEditViewModel(_carriers, this, carrier);
    }

    public void OpenOrderEdit(TransportOrder? order = null)
    {
        var vm = new TransportOrderEditViewModel(_orders, _customers, _carriers, _documents, this, order);
        CurrentView = vm;
        _ = vm.InitAsync();
    }

    public void OpenInvoiceEdit(Invoice? invoice = null, TransportOrder? order = null)
    {
        var vm = new InvoiceEditViewModel(_invoices, _orders, _pdf, this, invoice, order);
        CurrentView = vm;
        _ = vm.InitAsync();
    }

    public void OpenCarrierOrderEdit(CarrierOrder? carrierOrder = null)
    {
        var vm = new CarrierOrderEditViewModel(_carrierOrders, _carriers, _orders, _pdf, this, carrierOrder);
        CurrentView = vm;
        _ = vm.InitAsync();
    }
}

