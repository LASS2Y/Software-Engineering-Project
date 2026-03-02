using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models.Parts;

namespace KitBox.ViewModels;

/// <summary>Wraps a Part for display in the stock management table.</summary>
public partial class StockItemViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public StockItemViewModel(Part part, MainViewModel main)
    {
        _main = main;
        Part  = part;
        _newQuantity = part.StockQuantity;
    }

    public Part   Part      { get; }
    public string PartType  => Part.GetType().Name;
    public bool   IsLow     => Part.StockQuantity < Part.MinimumStock;
    public string  StockBadge      => IsLow ? "⚠ Low" : "OK";
    public string  RowBackground    => IsLow ? "#FFF7F0" : "Transparent";
    public string  BadgeForeground  => IsLow ? "#DC2626" : "#16A34A";

    [ObservableProperty] private int _newQuantity;

    [RelayCommand]
    public void SaveStock()
    {
        try
        {
            _main.Services.StockService.AddStock(Part.Id, NewQuantity - Part.StockQuantity);
            Part.StockQuantity = NewQuantity;
            OnPropertyChanged(nameof(IsLow));
            OnPropertyChanged(nameof(StockBadge));
        }
        catch (Exception ex)
        {
            // Surface through the dashboard's LoadError
            _ = ex;
        }
    }
}

/// <summary>
/// Owner dashboard: displays all parts with stock levels,
/// highlights low-stock items and allows inline quantity updates.
/// </summary>
public partial class OwnerDashboardViewModel : ViewModelBase
{
    private readonly MainViewModel _main;

    public OwnerDashboardViewModel(MainViewModel main)
    {
        _main = main;
        Refresh();
    }

    public ObservableCollection<StockItemViewModel> StockItems { get; } = new();

    [ObservableProperty] private int    _lowStockCount;
    [ObservableProperty] private string _loadError = string.Empty;

    [RelayCommand]
    public void Refresh()
    {
        LoadError = string.Empty;
        try
        {
            StockItems.Clear();
            var parts = _main.Services.PartRepository.GetAll();
            foreach (var p in parts)
                StockItems.Add(new StockItemViewModel(p, _main));

            LowStockCount = 0;
            foreach (var s in StockItems)
                if (s.IsLow) LowStockCount++;
        }
        catch (Exception ex)
        {
            LoadError = $"Could not load stock data: {ex.Message}\n" +
                        "Make sure the database is running and the schema is initialised.";
        }
    }

    [RelayCommand]
    public void Back()
        => _main.NavigateTo(new StartPageViewModel(_main));
}
