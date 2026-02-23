namespace KitBox.Models;

/// <summary>
/// Represents a locker (casier) configuration within a cabinet.
/// Each locker is defined by its dimensions, color, and optional doors.
/// 
/// A locker is composed of:
///   - 4 vertical battens
///   - 2 front crossbars (each with 2 grooves for door rails)
///   - 2 back crossbars (each with 1 groove for panel)
///   - 4 side crossbars (each with 1 groove for panel)
///   - 2 horizontal panels (resting on crossbars)
///   - 2 side panels (sliding into grooves)
///   - 1 back panel (sliding into grooves)
///   - 2 doors (optional) with 2 cup handles (not available for glass doors)
///
/// Total height = batten height + 2 × 2 cm (crossbar height top and bottom).
/// </summary>
public class Locker
{
    public int Id { get; set; }

    public int CabinetId { get; set; }

    /// <summary>
    /// Height of the vertical battens in centimeters (from catalog).
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Width of the locker in centimeters.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Depth of the locker in centimeters.
    /// </summary>
    public double Depth { get; set; }

    /// <summary>
    /// Color of all panels in this locker (same color for all panels).
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Whether this locker has doors.
    /// Lockers with doors have limited width options (see catalog).
    /// </summary>
    public bool HasDoors { get; set; }

    /// <summary>
    /// Color of the doors (can differ from the panel color).
    /// Null if the locker has no doors.
    /// </summary>
    public string? DoorColor { get; set; }

    /// <summary>
    /// Crossbar height constant in centimeters.
    /// </summary>
    private const double CrossbarHeight = 2.0;

    /// <summary>
    /// Total height of the locker including crossbars.
    /// Calculated as: batten height + 2 × crossbar height (top and bottom).
    /// </summary>
    public double TotalHeight => Height + 2 * CrossbarHeight;
}
