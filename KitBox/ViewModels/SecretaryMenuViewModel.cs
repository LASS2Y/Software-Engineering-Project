using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

/// <summary>
/// Intermediate menu for the secretary role.
/// Offers navigation to Supplier Catalog (price updates) or Stock Management.
/// </summary>
public partial class SecretaryMenuViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public SecretaryMenuViewModel(MainViewModel main)
    {
        _main = main;
    }

    [RelayCommand]
    public void GoToSupplierCatalog()
        => _main.GoToSupplierCatalog();

    [RelayCommand]
    public void GoToStockManagement()
        => _main.GoToOwnerDashboard();

    [RelayCommand]
    public void Back()
        => _main.NavigateTo(new StartPageViewModel(_main));
}
