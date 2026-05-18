using System;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;

namespace KitBox.ViewModels;

public partial class WelcomePageViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public WelcomePageViewModel(MainViewModel main)
    {
        _main = main;
    }
    
    [RelayCommand]
    public void ContinueAsGuest()
    {
        // Generate a short unique token so the DB UNIQUE constraint on email is satisfied
        var token = Guid.NewGuid().ToString("N")[..8].ToUpper();

        var guest = new Customer
        {
            FirstName = "Guest",
            LastName  = token,
            Email     = $"guest-{token}@guest.kitbox",
            Phone     = string.Empty
        };

        _main.GoToCabinetConfiguration(guest);
    }
    
    [RelayCommand]
    public void GoToSecretaryMenu()
        => _main.GoToSecretaryMenu();

    [RelayCommand]
    public void GoToConnexionMenu()
        => _main.GoToCustomerSelection();
}