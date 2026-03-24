using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;

namespace KitBox.ViewModels;

public partial class SupplierOrderItemViewModel : ViewModelBase
{
    private readonly SupplierOrderTrackingViewModel _parent;

    public SupplierOrderItemViewModel(
        SupplierOrder supplierOrder,
        SupplierOrderTrackingViewModel parent,
        string partLabel,
        string supplierLabel)
    {
        _parent = parent;
        SupplierOrder = supplierOrder;
        PartLabel = partLabel;
        SupplierLabel = supplierLabel;
    }

    public SupplierOrder SupplierOrder { get; }

    public string PartLabel { get; }
    public string SupplierLabel { get; }

    public int SupplierOrderId => SupplierOrder.Id;
    public string CustomerOrderInfo => SupplierOrder.CustomerOrderId.HasValue
        ? $"Customer order #{SupplierOrder.CustomerOrderId.Value}"
        : "Stock replenishment";

    public int Quantity => SupplierOrder.Quantity;
    public string UnitCost => $"EUR {SupplierOrder.UnitCost:F2}";
    public string OrderedAt => SupplierOrder.OrderedAt.ToString("dd/MM/yyyy");
    public string EtaDate => SupplierOrder.ExpectedDeliveryDate.ToString("dd/MM/yyyy");
    public string StatusText => SupplierOrder.Status;

    private string StatusNormalized => SupplierOrder.Status.Trim().ToLowerInvariant();

    public string StatusBackground => StatusNormalized switch
    {
        "ordered" => "#EFF6FF",
        "intransit" => "#FFF7ED",
        "received" => "#F0FDF4",
        "cancelled" => "#FEF2F2",
        _ => "#F3F4F6"
    };

    public string StatusColor => StatusNormalized switch
    {
        "ordered" => "#1D4ED8",
        "intransit" => "#C2410C",
        "received" => "#15803D",
        "cancelled" => "#B91C1C",
        _ => "#4B5563"
    };

    public bool CanMarkInTransit => StatusNormalized == "ordered";
    public bool CanMarkReceived => StatusNormalized == "ordered" || StatusNormalized == "intransit";
    public bool CanCancel => StatusNormalized == "ordered" || StatusNormalized == "intransit";

    [RelayCommand]
    public void MarkInTransit() => TransitionTo("InTransit");

    [RelayCommand]
    public void MarkReceived() => TransitionTo("Received");

    [RelayCommand]
    public void CancelOrder() => TransitionTo("Cancelled");

    private void TransitionTo(string status)
    {
        try
        {
            _parent.Main.Services.SupplierOrderTrackingService.TransitionStatus(SupplierOrder.Id, status);
            SupplierOrder.Status = status;
            RefreshProps();
            _parent.SetStatus($"Supplier order #{SupplierOrder.Id} moved to '{status}'.");
            _parent.Refresh();
        }
        catch (Exception ex)
        {
            _parent.SetStatus($"Error for supplier order #{SupplierOrder.Id}: {ex.Message}");
        }
    }

    private void RefreshProps()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusBackground));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(CanMarkInTransit));
        OnPropertyChanged(nameof(CanMarkReceived));
        OnPropertyChanged(nameof(CanCancel));
    }
}

public partial class SupplierOrderTrackingViewModel : ViewModelBase
{
    public MainViewModel Main { get; }

    public SupplierOrderTrackingViewModel(MainViewModel main)
    {
        Main = main;
        Refresh();
    }

    public ObservableCollection<SupplierOrderItemViewModel> Orders { get; } = new();

    [ObservableProperty] private int _totalOrders;
    [ObservableProperty] private int _openOrders;
    [ObservableProperty] private int _receivedOrders;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _loadError = string.Empty;

    [ObservableProperty] private string _selectedFilter = "All";
    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    public string[] FilterOptions { get; } = { "All", "Ordered", "InTransit", "Received", "Cancelled" };

    private List<SupplierOrderItemViewModel>? _allItems;

    [RelayCommand]
    public void Refresh()
    {
        LoadError = string.Empty;

        try
        {
            var partLabels = Main.Services.PartRepository.GetAll()
                .ToDictionary(p => p.Id, p => $"{p.Name} ({p.Reference})");
            var supplierLabels = Main.Services.SupplierRepository.GetAll()
                .ToDictionary(s => s.Id, s => s.Name);

            var orders = Main.Services.SupplierOrderTrackingService.GetAll();
            _allItems = orders.Select(so => new SupplierOrderItemViewModel(
                so,
                this,
                partLabels.TryGetValue(so.PartId, out var partLabel)
                    ? partLabel
                    : $"Part #{so.PartId}",
                supplierLabels.TryGetValue(so.SupplierId, out var supplierLabel)
                    ? supplierLabel
                    : $"Supplier #{so.SupplierId}"))
                .ToList();

            ApplyFilter();

            TotalOrders = _allItems.Count;
            ReceivedOrders = _allItems.Count(i => string.Equals(i.SupplierOrder.Status, "Received", StringComparison.OrdinalIgnoreCase));
            OpenOrders = _allItems.Count(i =>
                string.Equals(i.SupplierOrder.Status, "Ordered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(i.SupplierOrder.Status, "InTransit", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            LoadError = $"Could not load supplier orders: {ex.Message}\nMake sure the database is running.";
        }
    }

    [RelayCommand]
    public void Back()
        => Main.NavigateTo(new SecretaryMenuViewModel(Main));

    public void SetStatus(string msg) => StatusMessage = msg;

    private void ApplyFilter()
    {
        if (_allItems == null)
            return;

        Orders.Clear();
        IEnumerable<SupplierOrderItemViewModel> filtered = _allItems;

        if (SelectedFilter != "All")
        {
            filtered = filtered.Where(item =>
                string.Equals(item.SupplierOrder.Status, SelectedFilter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var item in filtered)
            Orders.Add(item);
    }
}
