using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;

namespace KitBox.ViewModels;

public partial class CabinetConfigurationViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly Customer      _customer;

    // ✅ Event déclenché chaque fois que la scène 3D doit être mise à jour
    public event Action<string>? OnRefresh3D;

    public CabinetConfigurationViewModel(MainViewModel main, Customer customer)
    {
        _main     = main;
        _customer = customer;

        AngleIronColors = new List<string>(_main.Services.CatalogService.GetAvailableAngleIronColors());
        _angleIronColor = AngleIronColors[0];

        AddLockerCommand.Execute(null);
    }

    public string CustomerName => $"{_customer.FirstName} {_customer.LastName}";

    public ObservableCollection<LockerConfigViewModel> Lockers { get; } = new();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanAddLocker))]
    private int _lockerCount;

    [ObservableProperty]
    private string _angleIronColor = string.Empty;
    
    partial void OnAngleIronColorChanged(string value) => TriggerRefresh();

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public bool CanAddLocker => LockerCount < Cabinet.MaxLockers;
    public List<string> AngleIronColors { get; }

    [RelayCommand(CanExecute = nameof(CanAddLocker))]
    public void AddLocker()
    {
        var locker = new LockerConfigViewModel(_main.Services.CatalogService);

        // ✅ Chaque changement de propriété du casier déclenche un refresh 3D
        locker.PropertyChanged += (s, e) => TriggerRefresh();

        Lockers.Add(locker);
        LockerCount = Lockers.Count;
        TriggerRefresh();
    }

    [RelayCommand]
    public void RemoveLocker(LockerConfigViewModel locker)
    {
        if (Lockers.Count > 1)
        {
            Lockers.Remove(locker);
            LockerCount = Lockers.Count;
            TriggerRefresh();
        }
    }

    // ✅ Sérialise tous les casiers en JSON et notifie la vue
    public void TriggerRefresh()
    {
        if (!Lockers.Any()) return;

        var data = new
        {
            lockers = Lockers.Select(vm => new
            {
                width       = vm.Width,
                totalHeight = vm.Height,
                depth       = vm.Depth,
                color       = vm.Color,
                hasDoors    = vm.HasDoors,
                doorColor   = vm.DoorColor ?? vm.Color
            }).ToList(),
            angleIronColor = AngleIronColor
        };

        OnRefresh3D?.Invoke(JsonSerializer.Serialize(data));
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

        _main.GoToOrderSummary(_customer, lockers, AngleIronColor);
    }

    [RelayCommand]
    public void Back() => _main.GoToWelcomePage();
}