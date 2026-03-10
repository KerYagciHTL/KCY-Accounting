using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

/// <summary>
/// Dashboard ViewModel: shows key metrics, recent orders and status breakdown.
/// Navigation commands delegate back to the MainViewModel shell.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly ITransportOrderRepository _orders;
    private readonly MainViewModel _shell;

    [ObservableProperty] private int _openOrdersCount;
    [ObservableProperty] private int _todayOrdersCount;
    [ObservableProperty] private int _totalOrdersCount;
    [ObservableProperty] private decimal _totalProfit;
    [ObservableProperty] private bool _isLoading;

    // Status breakdown counts
    [ObservableProperty] private int _statusNewCount;
    [ObservableProperty] private int _statusAssignedCount;
    [ObservableProperty] private int _statusInTransitCount;
    [ObservableProperty] private int _statusDeliveredCount;
    [ObservableProperty] private int _statusBilledCount;

    // Status breakdown percentages (0–100) for progress bars
    [ObservableProperty] private double _statusNewPct;
    [ObservableProperty] private double _statusAssignedPct;
    [ObservableProperty] private double _statusInTransitPct;
    [ObservableProperty] private double _statusDeliveredPct;
    [ObservableProperty] private double _statusBilledPct;

    /// <summary>The 10 most recent orders shown in the dashboard table.</summary>
    [ObservableProperty] private ObservableCollection<TransportOrder> _recentOrders = new();

    public DashboardViewModel(ITransportOrderRepository orders, MainViewModel shell)
    {
        _orders = orders;
        _shell = shell;
    }

    /// <summary>
    /// Loads all dashboard metrics asynchronously. Called by the shell after
    /// the view is attached so the UI is ready to receive property-change notifications.
    /// </summary>
    public async Task InitAsync()
    {
        IsLoading = true;
        var all = (await _orders.GetAllAsync()).ToList();

        TotalOrdersCount = all.Count;
        OpenOrdersCount = all.Count(o => o.Status is OrderStatus.New or OrderStatus.Assigned or OrderStatus.InTransit);
        TodayOrdersCount = all.Count(o => o.OrderDate.Date == DateTime.Today);
        TotalProfit = all.Sum(o => o.SalePrice - o.PurchasePrice);

        // Status breakdown
        StatusNewCount = all.Count(o => o.Status == OrderStatus.New);
        StatusAssignedCount = all.Count(o => o.Status == OrderStatus.Assigned);
        StatusInTransitCount = all.Count(o => o.Status == OrderStatus.InTransit);
        StatusDeliveredCount = all.Count(o => o.Status == OrderStatus.Delivered);
        StatusBilledCount = all.Count(o => o.Status == OrderStatus.Invoiced);

        // Calculate percentages for progress bars
        double total = TotalOrdersCount > 0 ? TotalOrdersCount : 1;
        StatusNewPct = StatusNewCount / total * 100;
        StatusAssignedPct = StatusAssignedCount / total * 100;
        StatusInTransitPct = StatusInTransitCount / total * 100;
        StatusDeliveredPct = StatusDeliveredCount / total * 100;
        StatusBilledPct = StatusBilledCount / total * 100;

        // Show the 10 most recent orders (by date descending).
        // We deliberately mutate the existing collection instead of replacing the reference,
        // because Avalonia's DataGrid can lose its binding when the ItemsSource reference
        // changes during the first layout pass.
        var recent = all.OrderByDescending(o => o.OrderDate).Take(10).ToList();
        RecentOrders.Clear();
        foreach (var o in recent)
            RecentOrders.Add(o);

        IsLoading = false;
    }

    // Quick-action navigation commands
    [RelayCommand] public void NewOrder() => _shell.OpenOrderEdit();
    [RelayCommand] public void NewCustomer() => _shell.OpenCustomerEdit();
    [RelayCommand] public void NewCarrier() => _shell.OpenCarrierEdit();
    [RelayCommand] public void NewInvoice() => _shell.OpenInvoiceEdit();
    [RelayCommand] public void GoToOrders() => _shell.NavigateToOrders();
}
