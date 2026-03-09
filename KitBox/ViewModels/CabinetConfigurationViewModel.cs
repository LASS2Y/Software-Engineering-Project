using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;

namespace KitBox.ViewModels;

/// <summary>
/// The main cabinet builder screen where the user adds and configures up to 7 lockers.
/// </summary>
public partial class CabinetConfigurationViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly Customer      _customer;

    public CabinetConfigurationViewModel(MainViewModel main, Customer customer)
    {
        _main     = main;
        _customer = customer;

        AngleIronColors = new List<string>(_main.Services.CatalogService.GetAvailableAngleIronColors());
        _angleIronColor = AngleIronColors[0];

        // Start with one empty locker
        AddLockerCommand.Execute(null);
    }

    public string CustomerName => $"{_customer.FirstName} {_customer.LastName}";

    public ObservableCollection<LockerConfigViewModel> Lockers { get; } = new();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanAddLocker))]
    private int _lockerCount;

    [ObservableProperty]
    private string _angleIronColor = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public bool CanAddLocker => LockerCount < Cabinet.MaxLockers;
    public List<string> AngleIronColors { get; }

    [RelayCommand(CanExecute = nameof(CanAddLocker))]
    public void AddLocker()
    {
        Lockers.Add(new LockerConfigViewModel(_main.Services.CatalogService));
        LockerCount = Lockers.Count;
    }

    [RelayCommand]
    public void RemoveLocker(LockerConfigViewModel locker)
    {
        if (Lockers.Count > 1)
        {
            Lockers.Remove(locker);
            LockerCount = Lockers.Count;
        }
    }

    [RelayCommand]
    public void Proceed()
    {
        ValidationMessage = string.Empty;

        var lockers = Lockers.Select(vm => vm.ToLocker()).ToList();
        var errors  = _main.Services.LockerValidationService.ValidateCabinet(lockers);

        if (errors.Count > 0)
        {
            ValidationMessage = string.Join("\n", errors);
            return;
        }

        double length = _main.Services.AngleIronCalculator.CalculateAngleIronLength(lockers);
        _main.GoToOrderSummary(_customer, lockers, AngleIronColor);
    }

    [RelayCommand]
    public void Back()
        => _main.GoToCustomerSelection();
}
