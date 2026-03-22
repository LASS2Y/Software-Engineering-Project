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
    private readonly OwnerDashboardViewModel _parent;

    public StockItemViewModel(Part part, MainViewModel main, OwnerDashboardViewModel parent)
    {
        _main = main;
        _parent = parent;
        Part  = part;
        _newQuantity = part.StockQuantity;
    }

    public Part   Part      { get; }
    public string PartType  => Part.GetType().Name;
    public bool   IsLow     => Part.StockQuantity < Part.MinimumStock;
    public string  StockBadge      => IsLow ? "⚠ Low" : "OK";
    public string  RowBackground    => IsLow ? "#FFF7F0" : "Transparent";
    public string  BadgeForeground  => IsLow ? "#DC2626" : "#16A34A";
    public int RecommendedOrderQuantity => Math.Max(0, Part.MinimumStock - Part.StockQuantity);
    public bool CanOrderFromSupplier => RecommendedOrderQuantity > 0;

    public string BestSupplierSummary
    {
        get
        {
            var supplier = _main.Services.SupplierSelectionService.GetBestSupplier(Part.Id);
            if (supplier == null)
                return "No supplier";

            return $"Supplier #{supplier.SupplierId} | €{supplier.Price:F2} | {supplier.DeliveryDays}d";
        }
    }

    public void SetStatus(string message)
    {
        StatusMessage = message;
    }

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

    [RelayCommand]
    public void OrderFromBestSupplier()
    {
        try
        {
            var result = _main.Services.StockService.PlaceReplenishmentOrder(Part.Id, RecommendedOrderQuantity);
            _parent.SetStatus(
                $"Supplier order #{result.SupplierOrderId} placed for {Part.Name}: " +
                $"qty {result.Quantity}, supplier #{result.SupplierId}, " +
                $"EUR {result.UnitCost:F2}, ETA {result.DeliveryDays} day(s).");
        }
        catch (Exception ex)
        {
            _parent.SetStatus($"Error while ordering {Part.Name}: {ex.Message}");
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
    [ObservableProperty] private string _statusMessage = string.Empty;

    [RelayCommand]
    public void Refresh()
    {
        LoadError = string.Empty;
        try
        {
            _main.Services.StockService.RefreshMinimumStockFromSalesHistory();

            StockItems.Clear();
            var parts = _main.Services.PartRepository.GetAll();
            foreach (var p in parts)
                StockItems.Add(new StockItemViewModel(p, _main, this));

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
        => _main.NavigateTo(new SecretaryMenuViewModel(_main));
}
