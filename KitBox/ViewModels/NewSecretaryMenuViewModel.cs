using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class NewSecretaryMenuViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    
    
    
    [ObservableProperty]
    private bool _sideMenuExpanded = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OwnerDashboardPageIsActive))]
    [NotifyPropertyChangedFor(nameof(SupplierCatalogPageIsActive))]
    [NotifyPropertyChangedFor(nameof(OrderHistoryPageIsActive))]
    [NotifyPropertyChangedFor(nameof(SupplierOrderTrackingPageIsActive))]
    [NotifyPropertyChangedFor(nameof(DashBoardPageIsActive))]
    [NotifyPropertyChangedFor(nameof(EmployeeInscriptionPageIsActive))]
    
    private ViewModelBase? _currentMenuPage;
    
    public bool OwnerDashboardPageIsActive => CurrentMenuPage == _ownerDashboardPage;
    public bool SupplierCatalogPageIsActive => CurrentMenuPage == _supplierCatalogPage;
    public bool OrderHistoryPageIsActive => CurrentMenuPage == _orderHistoryPage;
    public bool SupplierOrderTrackingPageIsActive => CurrentMenuPage == _supplierOrderTrackingPage;
    public bool EmployeeInscriptionPageIsActive => CurrentMenuPage == _employeeInscriptionPage;

    public bool DashBoardPageIsActive => CurrentMenuPage == _dashBoardPage;
    
    
    private readonly OwnerDashboardViewModel _ownerDashboardPage ;
    private readonly SupplierCatalogViewModel  _supplierCatalogPage;
    private readonly OrderHistoryViewModel _orderHistoryPage;
    private readonly SupplierOrderTrackingViewModel _supplierOrderTrackingPage;
    private readonly DashBoardViewModel _dashBoardPage;
    private readonly EmployeeInscriptionViewModel _employeeInscriptionPage;

    public NewSecretaryMenuViewModel(MainViewModel main)
    {
        _main = main;
        _ownerDashboardPage = new OwnerDashboardViewModel(_main);
        _supplierCatalogPage = new SupplierCatalogViewModel(_main);
        _orderHistoryPage = new OrderHistoryViewModel(_main);
        _supplierOrderTrackingPage = new SupplierOrderTrackingViewModel(_main);
        _dashBoardPage = new DashBoardViewModel(_main);
        _employeeInscriptionPage = new EmployeeInscriptionViewModel(_main);
        CurrentMenuPage = _orderHistoryPage;
    }
    
    [RelayCommand]
    private void SideMenuResize()
    {
        SideMenuExpanded = !SideMenuExpanded;
    }

    [RelayCommand]
    private void GoToOwnerDashboard()
    {
        CurrentMenuPage = _ownerDashboardPage;
    }

    [RelayCommand]
    private void GoToSupplierCatalog()
    {
        CurrentMenuPage = _supplierCatalogPage;
    }

    [RelayCommand]
    private void GoToOrderHistory()
    {
        CurrentMenuPage = _orderHistoryPage;
    }

    [RelayCommand]
    private void GoToSupplierOrderTracking()
    {
        CurrentMenuPage = _supplierOrderTrackingPage;
    }

    [RelayCommand]
    private void GoToDashBoard()
    {
        CurrentMenuPage = _dashBoardPage;
    }
    
    [RelayCommand]
    private void GoToEmployeeInscription()
    {
        CurrentMenuPage = _employeeInscriptionPage;
    }

    [RelayCommand]
    public void Back()
        => _main.NavigateTo(new WelcomePageViewModel(_main));
}