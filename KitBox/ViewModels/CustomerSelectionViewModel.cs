using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;

namespace KitBox.ViewModels;

/// <summary>
/// Lets the user enter or select a customer before configuring a cabinet order.
/// </summary>
public partial class CustomerSelectionViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public CustomerSelectionViewModel(MainViewModel main)
    {
        _main = main;
    }

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName  = string.Empty;
    [ObservableProperty] private string _email     = string.Empty;
    [ObservableProperty] private string _phone     = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [RelayCommand]
    public void Continue()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "First name and last name are required.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
        {
            ErrorMessage = "A valid email address is required.";
            return;
        }

        var customer = new Customer
        {
            FirstName = FirstName.Trim(),
            LastName  = LastName.Trim(),
            Email     = Email.Trim(),
            Phone     = Phone.Trim()
        };

        _main.GoToCabinetConfiguration(customer);
    }

    [RelayCommand]
    public void Back()
        => _main.NavigateTo(new StartPageViewModel(_main));
}
