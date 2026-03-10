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
    private readonly IPdfService _pdf;
    private readonly MainViewModel _shell;

    [ObservableProperty] private ObservableCollection<Invoice> _invoiceItems = new();
    [ObservableProperty] private Invoice? _selectedInvoice;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public InvoiceListViewModel(IInvoiceRepository invoices, ITransportOrderRepository orders,
        IPdfService pdf, MainViewModel shell)
    {
        _invoices = invoices;
        _orders   = orders;
        _pdf      = pdf;
        _shell    = shell;
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

    /// <summary>
    /// Generates a PDF for the given invoice and opens it with the default viewer.
    /// </summary>
    [RelayCommand]
    private async Task PrintInvoice(Invoice? invoice)
    {
        if (invoice == null) return;

        StatusMessage = "PDF wird erstellt…";
        try
        {
            // Ensure navigation properties are loaded
            var order = await _orders.GetByIdAsync(invoice.TransportOrderId);
            if (order == null)
            {
                StatusMessage = "Auftrag nicht gefunden.";
                return;
            }

            var customer = order.Customer;
            if (customer == null)
            {
                StatusMessage = "Kunde nicht gefunden.";
                return;
            }

            var filePath = await _pdf.GenerateInvoicePdfAsync(invoice, order, customer);

            // Open PDF with the default system viewer
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = filePath,
                UseShellExecute = true
            });

            StatusMessage = $"PDF gespeichert: {filePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }
}
