using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

public partial class CustomerListViewModel : ViewModelBase
{
    private readonly ICustomerRepository _repo;
    private readonly MainViewModel _shell;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public CustomerListViewModel(ICustomerRepository repo, MainViewModel shell)
    {
        _repo = repo;
        _shell = shell;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        var items = string.IsNullOrWhiteSpace(SearchText)
            ? await _repo.GetAllAsync()
            : await _repo.SearchAsync(SearchText);
        Customers = new ObservableCollection<Customer>(items);
        IsLoading = false;
    }

    [RelayCommand]
    private async Task Search() => await LoadAsync();

    [RelayCommand]
    private void AddNew() => _shell.OpenCustomerEdit();

    [RelayCommand]
    private void Edit(Customer? customer)
    {
        if (customer != null) _shell.OpenCustomerEdit(customer);
    }

    [RelayCommand]
    private async Task Delete(Customer? customer)
    {
        if (customer == null) return;
        await _repo.DeleteAsync(customer.Id);
        await LoadAsync();
    }
}

