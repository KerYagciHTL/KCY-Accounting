using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

public partial class TransportOrderListViewModel : ViewModelBase
{
    private readonly ITransportOrderRepository _orders;
    private readonly ICustomerRepository _customers;
    private readonly ICarrierRepository _carriers;
    private readonly MainViewModel _shell;

    [ObservableProperty] private ObservableCollection<TransportOrder> _orders2 = new();
    [ObservableProperty] private TransportOrder? _selectedOrder;
    [ObservableProperty] private bool _isLoading;

    // Filter properties
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<Customer> _availableCustomers = new();
    [ObservableProperty] private ObservableCollection<Carrier> _availableCarriers = new();
    [ObservableProperty] private Customer? _filterCustomer;
    [ObservableProperty] private Carrier? _filterCarrier;
    [ObservableProperty] private OrderStatus? _filterStatus;
    [ObservableProperty] private DateTime? _filterDateFrom;
    [ObservableProperty] private DateTime? _filterDateTo;

    /// <summary>All possible status values for the filter dropdown.</summary>
    public IEnumerable<OrderStatus?> StatusOptions =>
        new OrderStatus?[] { null, OrderStatus.New, OrderStatus.Assigned, OrderStatus.InTransit, OrderStatus.Delivered, OrderStatus.Invoiced };

    public TransportOrderListViewModel(
        ITransportOrderRepository orders,
        ICustomerRepository customers,
        ICarrierRepository carriers,
        MainViewModel shell)
    {
        _orders = orders;
        _customers = customers;
        _carriers = carriers;
        _shell = shell;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var customers = await _customers.GetActiveAsync();
        var carriers = await _carriers.GetAllAsync();
        AvailableCustomers = new ObservableCollection<Customer>(customers);
        AvailableCarriers = new ObservableCollection<Carrier>(carriers);
        await Search();
        IsLoading = false;
    }

    [RelayCommand]
    private async Task Search()
    {
        var filter = new OrderFilter
        {
            SearchText = SearchText,
            CustomerId = FilterCustomer?.Id,
            CarrierId = FilterCarrier?.Id,
            Status = FilterStatus,
            DateFrom = FilterDateFrom,
            DateTo = FilterDateTo
        };
        var results = await _orders.SearchAsync(filter);
        Orders2 = new ObservableCollection<TransportOrder>(results);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterCustomer = null;
        FilterCarrier = null;
        FilterStatus = null;
        FilterDateFrom = null;
        FilterDateTo = null;
        _ = Search();
    }

    [RelayCommand]
    private void AddNew() => _shell.OpenOrderEdit();

    [RelayCommand]
    private void Edit(TransportOrder? order)
    {
        if (order != null) _shell.OpenOrderEdit(order);
    }

    [RelayCommand]
    private async Task Delete(TransportOrder? order)
    {
        if (order == null) return;
        await _orders.DeleteAsync(order.Id);
        _ = Search();
    }
}

