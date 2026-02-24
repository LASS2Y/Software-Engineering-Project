using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class StartPageViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public StartPageViewModel(MainViewModel main)
    {
        _main = main;
    }

    [RelayCommand]
    public void GoToHomePage()
        => _main.GoToCustomerSelection();

    [RelayCommand]
    public void GoToOwnerDashboard()
        => _main.GoToOwnerDashboard();
}