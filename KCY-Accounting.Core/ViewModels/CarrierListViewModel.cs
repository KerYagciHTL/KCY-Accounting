using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using System.Collections.ObjectModel;

namespace KCY_Accounting.Core.ViewModels;

public partial class CarrierListViewModel : ViewModelBase
{
    private readonly ICarrierRepository _repo;
    private readonly MainViewModel _shell;

    [ObservableProperty] private ObservableCollection<Carrier> _carriers = new();
    [ObservableProperty] private Carrier? _selectedCarrier;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public CarrierListViewModel(ICarrierRepository repo, MainViewModel shell)
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
        Carriers = new ObservableCollection<Carrier>(items);
        IsLoading = false;
    }

    [RelayCommand]
    private async Task Search() => await LoadAsync();

    [RelayCommand]
    private void AddNew() => _shell.OpenCarrierEdit();

    [RelayCommand]
    private void Edit(Carrier? carrier)
    {
        if (carrier != null) _shell.OpenCarrierEdit(carrier);
    }

    [RelayCommand]
    private async Task Delete(Carrier? carrier)
    {
        if (carrier == null) return;
        await _repo.DeleteAsync(carrier.Id);
        await LoadAsync();
    }
}

