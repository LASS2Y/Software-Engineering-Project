using System.Collections.Generic;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

/// <summary>
/// Provides the KitBox product catalog data.
/// Values are fixed by the physical catalog and encoded here as constants.
/// </summary>
public class CatalogService : ICatalogService
{
    // Batten heights available in the catalog (cm)
    private static readonly List<double> Heights = new() { 25, 30, 35, 40, 50 };

    // All available locker widths (cm)
    private static readonly List<double> Widths = new() { 40, 60, 80, 100 };

    // Widths available ONLY for lockers WITH doors (limited by door panel max width)
    // 2 doors × max 40 cm each = max locker width 80 cm
    private static readonly List<double> WidthsWithDoors = new() { 40, 60, 80 };

    // Available locker depths (cm)
    private static readonly List<double> Depths = new() { 30, 40, 50 };

    // Available colors for panels, doors and angle irons
    private static readonly List<string> Colors = new()
    {
        "White", "Black", "Grey", "Beige", "Oak", "Walnut"
    };

    /// <inheritdoc/>
    public double CrossbarHeight => 2.0;

    public IReadOnlyList<double> GetAvailableHeights()       => Heights.AsReadOnly();
    public IReadOnlyList<double> GetAvailableWidths()        => Widths.AsReadOnly();
    public IReadOnlyList<double> GetAvailableWidthsWithDoors() => WidthsWithDoors.AsReadOnly();
    public IReadOnlyList<double> GetAvailableDepths()        => Depths.AsReadOnly();
    public IReadOnlyList<string> GetAvailableColors()        => Colors.AsReadOnly();
}
