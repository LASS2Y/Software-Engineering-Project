using System.Collections.Generic;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Provides the fixed catalog data (available dimensions, colors, etc.)
/// derived from the KitBox product catalog. Acts as the single source
/// of truth for valid configuration values.
/// </summary>
public interface ICatalogService
{
    /// <summary>Available batten heights in cm (determines internal locker height).</summary>
    IReadOnlyList<double> GetAvailableHeights();

    /// <summary>Available locker widths in cm.</summary>
    IReadOnlyList<double> GetAvailableWidths();

    /// <summary>Available locker widths in cm for lockers WITH doors (limited by door panel sizes).</summary>
    IReadOnlyList<double> GetAvailableWidthsWithDoors();

    /// <summary>Available locker depths in cm.</summary>
    IReadOnlyList<double> GetAvailableDepths();

    /// <summary>Available colors for panels and angle irons.</summary>
    IReadOnlyList<string> GetAvailableColors();

    /// <summary>Crossbar height constant in cm (always 2 cm, used in TotalHeight calc).</summary>
    double CrossbarHeight { get; }
}
