using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public HomePageViewModel(MainViewModel main)
    {
        _main = main;
    }

    [RelayCommand]
    public void NewOrder()
        => _main.GoToCustomerSelection();
}