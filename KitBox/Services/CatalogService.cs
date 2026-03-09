using System.Collections.Generic;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

/// <summary>
/// Provides the KitBox product catalog data.
/// Values are derived from the real kitboxparts.csv catalog.
/// </summary>
public class CatalogService : ICatalogService
{
    // Batten/locker heights available in the catalog (cm)
    private static readonly List<double> Heights = new() { 32, 42, 52 };

    // All available locker widths (cm) – from front/back crossbar sizes
    private static readonly List<double> Widths = new() { 32, 42, 52, 62, 80, 100, 120 };

    // Widths available ONLY for lockers WITH doors (limited by door panel availability)
    private static readonly List<double> WidthsWithDoors = new() { 32, 42, 52, 62 };

    // Available locker depths (cm) – from side crossbar sizes
    private static readonly List<double> Depths = new() { 32, 42, 52, 62 };

    // Available colors for panels (locker body)
    private static readonly List<string> Colors = new()
    {
        "White", "Marron"
    };

    // Available colors for angle irons
    private static readonly List<string> AngleIronColors = new()
    {
        "White", "Marron", "Galva", "Black"
    };

    // Available colors for doors (including Glass option)
    private static readonly List<string> DoorColors = new()
    {
        "White", "Marron", "Glass"
    };

    /// <inheritdoc/>
    public double CrossbarHeight => 2.0;

    public IReadOnlyList<double> GetAvailableHeights()          => Heights.AsReadOnly();
    public IReadOnlyList<double> GetAvailableWidths()           => Widths.AsReadOnly();
    public IReadOnlyList<double> GetAvailableWidthsWithDoors()  => WidthsWithDoors.AsReadOnly();
    public IReadOnlyList<double> GetAvailableDepths()           => Depths.AsReadOnly();
    public IReadOnlyList<string> GetAvailableColors()           => Colors.AsReadOnly();
    public IReadOnlyList<string> GetAvailableAngleIronColors()  => AngleIronColors.AsReadOnly();
    public IReadOnlyList<string> GetAvailableDoorColors()       => DoorColors.AsReadOnly();
}
