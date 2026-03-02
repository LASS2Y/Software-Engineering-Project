using CommunityToolkit.Mvvm.ComponentModel;
using KitBox.Services.Interfaces;

namespace KitBox.ViewModels;

/// <summary>Wraps a PartAvailability record for display in the order summary list.</summary>
public partial class PartAvailabilityViewModel : ViewModelBase
{
    public PartAvailabilityViewModel(PartAvailability data)
    {
        PartName   = data.PartName;
        Reference  = data.Reference;
        Required   = data.Required;
        InStock    = data.InStock;
        IsAvailable = data.IsAvailable;
        UnitPrice  = data.UnitPrice;
    }

    public string  PartName    { get; }
    public string  Reference   { get; }
    public int     Required    { get; }
    public int     InStock     { get; }
    public bool    IsAvailable { get; }
    public decimal UnitPrice   { get; }
    public decimal TotalPrice  => Required * UnitPrice;

    /// <summary>Display label for stock availability.</summary>
    public string StockStatus => IsAvailable
        ? $"✓  In stock ({InStock})"
        : $"✗  Insufficient ({InStock}/{Required})";

    public string StatusBackground => IsAvailable ? "#DCFCE7" : "#FEE2E2";
    public string StatusForeground  => IsAvailable ? "#16A34A" : "#DC2626";
}
