using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.DataAccess;
using KitBox.Models;
using KitBox.Models.Enums;
using KitBox.Services.Interfaces;

namespace KitBox.ViewModels;

/// <summary>Wraps a single Order row for display and status management.</summary>
public partial class OrderItemViewModel : ViewModelBase
{
    private readonly OrderHistoryViewModel _parent;

    public OrderItemViewModel(Order order, OrderHistoryViewModel parent)
    {
        _parent = parent;
        Order   = order;
    }

    public Order Order { get; }

    // ── Display properties ───────────────────────────────────────────────────
    public int      OrderId       => Order.Id;
    public string   CustomerName  => Order.Customer != null
        ? $"{Order.Customer.FirstName} {Order.Customer.LastName}" : $"Customer #{Order.CustomerId}";
    public string   CustomerEmail => Order.Customer?.Email ?? "";
    public string   OrderDate     => Order.OrderDate.ToString("dd/MM/yyyy");
    public string   StatusText    => Order.Status.ToString();
    public decimal? Deposit       => Order.Deposit;
    public string   AvailableDate => Order.AvailableDate?.ToString("dd/MM/yyyy") ?? "—";
    public bool     HasBill       => Order.BillId.HasValue;
    public string   BillInfo      => Order.BillId.HasValue ? $"Bill #{Order.BillId}" : "No bill";

    // Computed from the transient OrderLine we stuffed in GetAllWithDetails
    public int     TotalParts  => Order.Lines.FirstOrDefault()?.Quantity ?? 0;
    public decimal TotalAmount => Order.Lines.FirstOrDefault()?.UnitPrice ?? 0m;

    // ── Status styling ───────────────────────────────────────────────────────
    public string StatusColor => Order.Status switch
    {
        OrderStatus.Available          => "#16A34A",
        OrderStatus.PartiallyAvailable => "#D97706",
        OrderStatus.Delivered          => "#2563EB",
        OrderStatus.Cancelled          => "#DC2626",
        _                              => "#6B7280"
    };

    public string StatusBackground => Order.Status switch
    {
        OrderStatus.Available          => "#F0FDF4",
        OrderStatus.PartiallyAvailable => "#FFFBEB",
        OrderStatus.Delivered          => "#EFF6FF",
        OrderStatus.Cancelled          => "#FEF2F2",
        _                              => "#F3F4F6"
    };

    // ── Can-execute flags ────────────────────────────────────────────────────
    public bool CanMarkAvailable => Order.Status == OrderStatus.PartiallyAvailable;
    public bool CanMarkDelivered => Order.Status == OrderStatus.Available;
    public bool CanGenerateBill  => Order.Status == OrderStatus.Delivered && !HasBill;
    public bool CanCancel        => Order.Status != OrderStatus.Delivered && Order.Status != OrderStatus.Cancelled;

    // ── Actions ──────────────────────────────────────────────────────────────

    [RelayCommand]
    public void MarkAvailable()
    {
        try
        {
            Order.Status = OrderStatus.Available;
            Order.AvailableDate = DateTime.Today;
            _parent.Main.Services.OrderRepository.Update(Order);
            RefreshProps();
            _parent.SetStatus($"Order #{OrderId} marked as Available.");
        }
        catch (Exception ex) { _parent.SetStatus($"Error: {ex.Message}"); }
    }

    [RelayCommand]
    public void MarkDelivered()
    {
        try
        {
            Order.Status = OrderStatus.Delivered;
            _parent.Main.Services.OrderRepository.Update(Order);
            RefreshProps();
            _parent.SetStatus($"Order #{OrderId} marked as Delivered.");
        }
        catch (Exception ex) { _parent.SetStatus($"Error: {ex.Message}"); }
    }

    [RelayCommand]
    public void GenerateBill()
    {
        try
        {
            var bill = new Bill
            {
                EmissionDate = DateTime.Today,
                Amount       = TotalAmount - (Deposit ?? 0m)
            };
            _parent.Main.Services.BillRepository.Add(bill);

            Order.BillId = bill.Id;
            _parent.Main.Services.OrderRepository.Update(Order);

            var request = new InvoiceExportRequest(
                DocumentType: "Final_Payment_Invoice",
                OrderId: OrderId,
                BillId: bill.Id,
                CustomerName: CustomerName,
                CustomerEmail: CustomerEmail,
                IssuedAt: bill.EmissionDate,
                TotalAmount: TotalAmount,
                DepositAmount: Deposit ?? 0m,
                AmountPaid: bill.Amount,
                RemainingAmount: 0m,
                Notes: "Final payment received. Balance is now fully settled."
            );
            string documentPath = _parent.Main.Services.InvoiceExportService.ExportPdf(request);

            RefreshProps();
            _parent.SetStatus($"Bill #{bill.Id} generated for Order #{OrderId} — Amount: €{bill.Amount:F2} | PDF downloaded: {documentPath}");
        }
        catch (Exception ex) { _parent.SetStatus($"Error: {ex.Message}"); }
    }

    [RelayCommand]
    public void CancelOrder()
    {
        try
        {
            // Restore only the quantity that was actually consumed from stock at order creation.
            RestoreStockForCancellation();

            Order.Status = OrderStatus.Cancelled;
            _parent.Main.Services.OrderRepository.Update(Order);
            RefreshProps();
            _parent.SetStatus($"Order #{OrderId} cancelled.");
        }
        catch (Exception ex) { _parent.SetStatus($"Error: {ex.Message}"); }
    }

    private void RestoreStockForCancellation()
    {
        var lines = _parent.Main.Services.OrderLineRepository.GetByOrderId(Order.Id);
        if (lines.Count == 0)
            return;

        var requiredByPart = lines
            .GroupBy(l => l.PartId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var missingByPart = _parent.Main.Services.SupplierOrderRepository
            .GetByCustomerOrderId(Order.Id)
            .GroupBy(so => so.PartId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var (partId, requiredQty) in requiredByPart)
        {
            int missingQty = missingByPart.TryGetValue(partId, out int value) ? value : 0;
            int consumedQty = Math.Max(0, requiredQty - missingQty);
            if (consumedQty <= 0)
                continue;

            var part = _parent.Main.Services.PartRepository.GetById(partId);
            if (part == null)
                continue;

            _parent.Main.Services.PartRepository.UpdateStock(partId, part.StockQuantity + consumedQty);
        }
    }

    private void RefreshProps()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(StatusBackground));
        OnPropertyChanged(nameof(HasBill));
        OnPropertyChanged(nameof(BillInfo));
        OnPropertyChanged(nameof(CanMarkAvailable));
        OnPropertyChanged(nameof(CanMarkDelivered));
        OnPropertyChanged(nameof(CanGenerateBill));
        OnPropertyChanged(nameof(CanCancel));
    }
}

/// <summary>
/// Displays the full order history with filtering by status
/// and inline actions: mark available, mark delivered, generate bill, cancel.
/// </summary>
public partial class OrderHistoryViewModel : ViewModelBase
{
    public MainViewModel Main { get; }
    

    public OrderHistoryViewModel(MainViewModel main)
    {
        Main = main;
        Refresh();
    }

    public ObservableCollection<OrderItemViewModel> Orders { get; } = new();

    [ObservableProperty] private int    _totalOrders;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private string _loadError     = string.Empty;

    // Filter
    [ObservableProperty] private string _selectedFilter = "All";
    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    public string[] FilterOptions { get; } =
        { "All", "Pending", "PartiallyAvailable", "Available", "Delivered", "Cancelled" };

    private System.Collections.Generic.List<OrderItemViewModel>? _allItems;

    [RelayCommand]
    public void Refresh()
    {
        LoadError = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var orders = Main.Services.OrderRepository.GetAllWithDetails();
            _allItems = orders.Select(o => new OrderItemViewModel(o, this)).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            LoadError = $"Could not load orders: {ex.Message}\nMake sure the database is running.";
        }
    }

    [RelayCommand]
    public void Back()
        => Main.NavigateTo(new SecretaryMenuViewModel(Main));

    public void SetStatus(string msg) => StatusMessage = msg;

    private void ApplyFilter()
    {
        if (_allItems == null) return;
        Orders.Clear();

        var filtered = _allItems.AsEnumerable();
        if (SelectedFilter != "All")
        {
            if (Enum.TryParse<OrderStatus>(SelectedFilter, out var status))
                filtered = filtered.Where(o => o.Order.Status == status);
        }

        foreach (var item in filtered)
            Orders.Add(item);
        TotalOrders = Orders.Count;
    }
}
