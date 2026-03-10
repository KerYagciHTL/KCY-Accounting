using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.ViewModels;

public partial class CarrierEditViewModel : ViewModelBase
{
    private readonly ICarrierRepository _repo;
    private readonly MainViewModel _shell;
    private readonly Carrier _carrier;

    public bool IsEditMode { get; }
    public string Title => IsEditMode ? "Frächter bearbeiten" : "Neuer Frächter";

    [ObservableProperty] private string _carrierNumber = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _contactPerson = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _street = string.Empty;
    [ObservableProperty] private string _zipCode = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _country = string.Empty;
    [ObservableProperty] private string _bankName = string.Empty;
    [ObservableProperty] private string _iban = string.Empty;
    [ObservableProperty] private string _bic = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public CarrierEditViewModel(ICarrierRepository repo, MainViewModel shell, Carrier? existing)
    {
        _repo = repo;
        _shell = shell;
        IsEditMode = existing != null;
        _carrier = existing ?? new Carrier();
        if (IsEditMode) PopulateFields(_carrier);
        else _ = LoadNextNumberAsync();
    }

    private async Task LoadNextNumberAsync() =>
        CarrierNumber = await _repo.GetNextCarrierNumberAsync();

    private void PopulateFields(Carrier c)
    {
        CarrierNumber = c.CarrierNumber;
        CompanyName = c.CompanyName;
        ContactPerson = c.ContactPerson;
        Phone = c.Phone;
        Email = c.Email;
        Street = c.Street;
        ZipCode = c.ZipCode;
        City = c.City;
        Country = c.Country;
        BankName = c.BankName;
        Iban = c.Iban;
        Bic = c.Bic;
        Notes = c.Notes;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            ErrorMessage = "Firmenname ist ein Pflichtfeld.";
            return;
        }

        _carrier.CarrierNumber = CarrierNumber;
        _carrier.CompanyName = CompanyName;
        _carrier.ContactPerson = ContactPerson;
        _carrier.Phone = Phone;
        _carrier.Email = Email;
        _carrier.Street = Street;
        _carrier.ZipCode = ZipCode;
        _carrier.City = City;
        _carrier.Country = Country;
        _carrier.BankName = BankName;
        _carrier.Iban = Iban;
        _carrier.Bic = Bic;
        _carrier.Notes = Notes;

        if (IsEditMode)
            await _repo.UpdateAsync(_carrier);
        else
            await _repo.AddAsync(_carrier);

        _shell.NavigateToCarriers();
    }

    [RelayCommand]
    private void Cancel() => _shell.NavigateToCarriers();
}

