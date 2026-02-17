using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class HomePageViewModel: ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    public HomePageViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }
}