using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.ViewModels;

public partial class CustomerEditViewModel : ViewModelBase
{
    private readonly ICustomerRepository _repo;
    private readonly MainViewModel _shell;
    private readonly Customer _customer;

    public bool IsEditMode { get; }
    public string Title => IsEditMode ? "Kunde bearbeiten" : "Neuer Kunde";

    // ---- Basic data ----
    [ObservableProperty] private string _customerNumber = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _contactPerson = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;

    // ---- Address ----
    [ObservableProperty] private string _street = string.Empty;
    [ObservableProperty] private string _zipCode = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _country = string.Empty;

    // ---- Billing ----
    [ObservableProperty] private string _vatNumber = string.Empty;
    [ObservableProperty] private int _paymentTermDays = 30;
    [ObservableProperty] private string _currency = "EUR";

    // ---- Freight payer ----
    [ObservableProperty] private string _freightPayerName = string.Empty;
    [ObservableProperty] private string _freightPayerVatNumber = string.Empty;

    // ---- Misc ----
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public CustomerEditViewModel(ICustomerRepository repo, MainViewModel shell, Customer? existing)
    {
        _repo = repo;
        _shell = shell;
        IsEditMode = existing != null;
        _customer = existing ?? new Customer();

        if (IsEditMode) PopulateFields(_customer);
        else _ = LoadNextNumberAsync();
    }

    private async Task LoadNextNumberAsync() =>
        CustomerNumber = await _repo.GetNextCustomerNumberAsync();

    private void PopulateFields(Customer c)
    {
        CustomerNumber = c.CustomerNumber;
        CompanyName = c.CompanyName;
        ContactPerson = c.ContactPerson;
        Phone = c.Phone;
        Email = c.Email;
        Street = c.Street;
        ZipCode = c.ZipCode;
        City = c.City;
        Country = c.Country;
        VatNumber = c.VatNumber;
        PaymentTermDays = c.PaymentTermDays;
        Currency = c.Currency;
        FreightPayerName = c.FreightPayerName ?? string.Empty;
        FreightPayerVatNumber = c.FreightPayerVatNumber ?? string.Empty;
        Notes = c.Notes;
        IsActive = c.IsActive;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            ErrorMessage = "Firmenname ist ein Pflichtfeld.";
            return;
        }

        _customer.CustomerNumber = CustomerNumber;
        _customer.CompanyName = CompanyName;
        _customer.ContactPerson = ContactPerson;
        _customer.Phone = Phone;
        _customer.Email = Email;
        _customer.Street = Street;
        _customer.ZipCode = ZipCode;
        _customer.City = City;
        _customer.Country = Country;
        _customer.VatNumber = VatNumber;
        _customer.PaymentTermDays = PaymentTermDays;
        _customer.Currency = Currency;
        _customer.FreightPayerName = string.IsNullOrWhiteSpace(FreightPayerName) ? null : FreightPayerName;
        _customer.FreightPayerVatNumber = string.IsNullOrWhiteSpace(FreightPayerVatNumber) ? null : FreightPayerVatNumber;
        _customer.Notes = Notes;
        _customer.IsActive = IsActive;

        if (IsEditMode)
            await _repo.UpdateAsync(_customer);
        else
            await _repo.AddAsync(_customer);

        _shell.NavigateToCustomers();
    }

    [RelayCommand]
    private void Cancel() => _shell.NavigateToCustomers();
}

