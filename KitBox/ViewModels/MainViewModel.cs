using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentPage;
    
    private readonly HomePageViewModel _homePage;
    private readonly StartPageViewModel _startPage;
   

    public MainViewModel()
    {
        _homePage = new HomePageViewModel(this);
        _startPage = new StartPageViewModel(this);

        CurrentPage = _startPage; // page par défaut
    }
}