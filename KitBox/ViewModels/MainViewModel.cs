using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KitBox.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentPage;
    
    private readonly HomePageViewModel _homePage;
   

    public MainViewModel()
    {
        _homePage = new HomePageViewModel(this);
        

        CurrentPage = _homePage; // page par défaut
    }
}