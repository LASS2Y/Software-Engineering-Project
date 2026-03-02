using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using KitBox.Models;
using KitBox.Services.Interfaces;

namespace KitBox.ViewModels;

/// <summary>
/// Represents a single locker being configured by the user inside the cabinet builder.
/// Bound directly to the locker configuration form rows.
/// </summary>
public partial class LockerConfigViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog;

    public LockerConfigViewModel(ICatalogService catalog)
    {
        _catalog = catalog;

        // Default to first catalog values
        var heights = _catalog.GetAvailableHeights();
        var widths  = _catalog.GetAvailableWidths();
        var depths  = _catalog.GetAvailableDepths();
        var colors  = _catalog.GetAvailableColors();

        _height   = heights.Count > 0 ? heights[0] : 30;
        _width    = widths.Count  > 0 ? widths[0]  : 60;
        _depth    = depths.Count  > 0 ? depths[0]  : 40;
        _color    = colors.Count  > 0 ? colors[0]  : "White";
        _doorColor = _color;
    }

    [ObservableProperty] private double  _height;
    [ObservableProperty] private double  _width;
    [ObservableProperty] private double  _depth;
    [ObservableProperty] private string  _color     = string.Empty;
    [ObservableProperty] private bool    _hasDoors;
    [ObservableProperty] private string  _doorColor = string.Empty;

    // Catalog choices exposed to the ComboBoxes
    public IReadOnlyList<double> AvailableHeights => _catalog.GetAvailableHeights();
    public IReadOnlyList<double> AvailableWidths
        => HasDoors ? _catalog.GetAvailableWidthsWithDoors() : _catalog.GetAvailableWidths();
    public IReadOnlyList<double> AvailableDepths  => _catalog.GetAvailableDepths();
    public IReadOnlyList<string> AvailableColors  => _catalog.GetAvailableColors();

    // Re-evaluate available widths when HasDoors changes
    partial void OnHasDoorsChanged(bool value)
    {
        OnPropertyChanged(nameof(AvailableWidths));
        // Reset width if current selection no longer valid
        if (!AvailableWidths.Contains(Width))
            Width = AvailableWidths[0];
    }

    /// <summary>Converts this ViewModel to the domain Locker model.</summary>
    public Locker ToLocker() => new Locker
    {
        Height    = Height,
        Width     = Width,
        Depth     = Depth,
        Color     = Color,
        HasDoors  = HasDoors,
        DoorColor = HasDoors ? DoorColor : null
    };
}
