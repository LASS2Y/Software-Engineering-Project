using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class StartPageViewModel:ViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    
    public StartPageViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    public void GoToHomePage()
    {
        _mainViewModel.CurrentPage = new HomePageViewModel(_mainViewModel);
    }
}