using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KitBox.Models;
using KitBox.Services.Interfaces;

namespace KitBox.ViewModels;

/// <summary>
/// Displays the full order preview: parts required, stock status, total price.
/// Allows confirming the order with an optional deposit.
/// </summary>
public partial class OrderSummaryViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly Customer      _customer;
    private readonly List<Locker>  _lockers;
    private readonly string        _angleIronColor;

    public OrderSummaryViewModel(MainViewModel main, Customer customer,
                                  List<Locker> lockers, string angleIronColor)
    {
        _main           = main;
        _customer       = customer;
        _lockers        = lockers;
        _angleIronColor = angleIronColor;

        LoadPreview();
    }

    [ObservableProperty] private double  _angleIronLength;
    [ObservableProperty] private string  _angleIronColor2 = string.Empty;
    [ObservableProperty] private decimal _totalPrice;
    [ObservableProperty] private bool    _allPartsAvailable;
    [ObservableProperty] private decimal? _depositAmount;
    [ObservableProperty] private string  _statusMessage   = string.Empty;
    [ObservableProperty] private bool    _orderPlaced;
    [ObservableProperty] private string  _loadError       = string.Empty;

    public string CustomerName => $"{_customer.FirstName} {_customer.LastName}";
    public int    LockerCount  => _lockers.Count;

    public ObservableCollection<PartAvailabilityViewModel> Parts { get; } = new();

    private void LoadPreview()
    {
        try
        {
            var preview = _main.Services.OrderService.PreviewOrder(_lockers, _angleIronColor);

            AngleIronLength   = preview.AngleIronLength;
            AngleIronColor2   = preview.AngleIronColor;
            TotalPrice        = preview.TotalPrice;
            AllPartsAvailable = preview.AllPartsAvailable;

            // Suggest 30% deposit when not all parts are in stock
            DepositAmount = AllPartsAvailable ? null : Math.Round(TotalPrice * 0.30m, 2);

            Parts.Clear();
            foreach (var p in preview.Parts)
                Parts.Add(new PartAvailabilityViewModel(p));
        }
        catch (Exception ex)
        {
            LoadError = $"Could not load order preview: {ex.Message}\n" +
                        "Make sure the database is running and the schema is initialised.";
        }
    }

    [RelayCommand]
    public void PlaceOrder()
    {
        try
        {
            var order = _main.Services.OrderService.PlaceOrder(
                _customer, _lockers, _angleIronColor, DepositAmount ?? 0m);

            string? documentPath = null;
            if (!AllPartsAvailable && (DepositAmount ?? 0m) > 0m)
            {
                var request = new InvoiceExportRequest(
                    DocumentType: "Deposit_Receipt",
                    OrderId: order.Id,
                    BillId: null,
                    CustomerName: CustomerName,
                    CustomerEmail: _customer.Email,
                    IssuedAt: DateTime.Today,
                    TotalAmount: TotalPrice,
                    DepositAmount: DepositAmount ?? 0m,
                    AmountPaid: DepositAmount ?? 0m,
                    RemainingAmount: TotalPrice - (DepositAmount ?? 0m),
                    Notes: "Deposit received for a partially available order. Remaining balance is due at pickup.",
                    EstimatedAvailableDate: order.AvailableDate
                );
                documentPath = _main.Services.InvoiceExportService.ExportTxt(request);
            }

            OrderPlaced = true;
            StatusMessage = AllPartsAvailable
                ? $"✓  Order #{order.Id} confirmed. All parts are ready for pickup."
                : $"✓  Order #{order.Id} recorded. Deposit required: €{DepositAmount:F2}." +
                  $"\nExpected availability: {order.AvailableDate?.ToString("dd/MM/yyyy") ?? "N/A"}." +
                  (documentPath == null ? string.Empty : $"\nTXT receipt downloaded: {documentPath}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error placing order: {ex.Message}";
        }
    }

    [RelayCommand]
    public void Back()
        => _main.GoToCabinetConfiguration(_customer);

    [RelayCommand]
    public void ReturnToStart()
        => _main.NavigateTo(new StartPageViewModel(_main));
}
