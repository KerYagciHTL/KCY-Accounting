using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

/// <summary>
/// List view for all carrier orders (Frächteraufträge).
/// Supports search by carrier name or order number.
/// Provides inline PDF printing without navigating to the edit form.
/// </summary>
public partial class CarrierOrderListViewModel : ViewModelBase
{
    private readonly ICarrierOrderRepository _repo;
    private readonly IPdfService             _pdf;
    private readonly MainViewModel           _shell;

    [ObservableProperty] private ObservableCollection<CarrierOrder> _carrierOrders = new();
    [ObservableProperty] private string _searchText  = string.Empty;
    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;

    private List<CarrierOrder> _allOrders = new();

    public CarrierOrderListViewModel(ICarrierOrderRepository repo, IPdfService pdf, MainViewModel shell)
    {
        _repo  = repo;
        _pdf   = pdf;
        _shell = shell;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        _allOrders = (await _repo.GetAllAsync()).ToList();
        ApplyFilter();
        IsLoading = false;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = SearchText.Trim().ToLowerInvariant();
        var filtered = string.IsNullOrEmpty(q)
            ? _allOrders
            : _allOrders.Where(co =>
                co.CarrierOrderNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (co.Carrier?.CompanyName.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (co.TransportOrder?.OrderNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));

        CarrierOrders = new ObservableCollection<CarrierOrder>(filtered);
    }

    [RelayCommand]
    private void NewCarrierOrder() => _shell.OpenCarrierOrderEdit();

    [RelayCommand]
    private void EditCarrierOrder(CarrierOrder? co)
    {
        if (co != null) _shell.OpenCarrierOrderEdit(co);
    }

    /// <summary>
    /// Loads the full carrier order (with FreightItems + Carrier navigation)
    /// and opens the generated PDF directly – no need to open the edit form first.
    /// </summary>
    [RelayCommand]
    private async Task PrintCarrierOrder(CarrierOrder? co)
    {
        if (co == null) return;

        StatusMessage = "PDF wird erstellt…";
        try
        {
            var full = await _repo.GetWithDetailsAsync(co.Id);
            if (full == null) { StatusMessage = "Auftrag nicht gefunden."; return; }

            var filePath = await _pdf.GenerateCarrierOrderPdfAsync(full);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = filePath,
                UseShellExecute = true
            });
            StatusMessage = $"PDF geöffnet: {filePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteCarrierOrder(CarrierOrder? co)
    {
        if (co == null) return;
        await _repo.DeleteAsync(co.Id);
        _allOrders.Remove(co);
        CarrierOrders.Remove(co);
    }
}

