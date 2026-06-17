using CommunityToolkit.Mvvm.ComponentModel;
using KitBox.DataAccess;
using KitBox.Views;

namespace KitBox.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentPage = null!;
    
    public AppServices Services { get; }

    public MainViewModel()
    {
        // Database credentials loaded from KitBox/.env
        var db = new DatabaseConnection(
            server:   EnvConfig.Get("DB_SERVER",   "localhost"),
            database: EnvConfig.Get("DB_NAME",     "kitbox"),
            user:     EnvConfig.Get("DB_USER",     "root"),
            password: EnvConfig.Get("DB_PASSWORD", ""),
            port:     int.Parse(EnvConfig.Get("DB_PORT", "3306")));

        Services = new AppServices(db);
        CurrentPage = new WelcomePageViewModel(this);
    }

    // ── Navigation helpers called by child ViewModels ────────────────────────────

    public void NavigateTo(ViewModelBase page)
        => CurrentPage = page;

    public void GoToCustomerSelection()
        => NavigateTo(new CustomerSelectionViewModel(this));

    public void GoToDashboard()
        => NavigateTo(new DashBoardViewModel(this));

    public void GoToOwnerDashboard()
        => NavigateTo(new OwnerDashboardViewModel(this));

    public void GoToSecretaryMenu()
        => NavigateTo(new NewSecretaryMenuViewModel(this));

    public void GoToSupplierCatalog()
        => NavigateTo(new SupplierCatalogViewModel(this));

    public void GoToSupplierOrderTracking()
        => NavigateTo(new SupplierOrderTrackingViewModel(this));

    public void GoToOrderHistory()
        => NavigateTo(new OrderHistoryViewModel(this));

    public void GoToHome()
        => NavigateTo(new HomePageViewModel(this));

    public void GoToCabinetConfiguration(Models.Customer customer)
        => NavigateTo(new CabinetConfigurationViewModel(this, customer));

    public void GoToOrderSummary(Models.Customer customer,
                                  System.Collections.Generic.List<Models.Locker> lockers,
                                  string angleIronColor)
        => NavigateTo(new OrderSummaryViewModel(this, customer, lockers, angleIronColor));

    public void GoToWelcomePage()
        => NavigateTo(new WelcomePageViewModel(this));
    public void GoToEmployeeInscription()
        => NavigateTo(new EmployeeInscriptionViewModel(this));
}