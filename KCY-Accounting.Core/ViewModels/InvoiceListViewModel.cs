using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

public partial class InvoiceListViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoices;
    private readonly ITransportOrderRepository _orders;
    private readonly MainViewModel _shell;

    [ObservableProperty] private ObservableCollection<Invoice> _invoiceItems = new();
    [ObservableProperty] private Invoice? _selectedInvoice;
    [ObservableProperty] private bool _isLoading;

    public InvoiceListViewModel(IInvoiceRepository invoices, ITransportOrderRepository orders, MainViewModel shell)
    {
        _invoices = invoices;
        _orders = orders;
        _shell = shell;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var items = await _invoices.GetAllAsync();
        InvoiceItems = new ObservableCollection<Invoice>(items);
        IsLoading = false;
    }

    [RelayCommand]
    private void AddNew() => _shell.OpenInvoiceEdit();

    [RelayCommand]
    private void Edit(Invoice? invoice)
    {
        if (invoice != null) _shell.OpenInvoiceEdit(invoice);
    }

    [RelayCommand]
    private async Task Delete(Invoice? invoice)
    {
        if (invoice == null) return;
        await _invoices.DeleteAsync(invoice.Id);
        await LoadAsync();
    }
}

