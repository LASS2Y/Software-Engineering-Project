using KitBox.Models.Enums;

namespace KitBox.Models.Parts;

/// <summary>
/// Represents a crossbar (traverse) used in a locker.
/// A locker contains: 2 front crossbars, 2 back crossbars, and 4 side crossbars.
/// Each crossbar type has a specific number of grooves for panels or doors.
/// </summary>
public class Crossbar : Part
{
    /// <summary>
    /// The type of crossbar (Front, Back, or Side).
    /// </summary>
    public CrossbarType Type { get; set; }

    /// <summary>
    /// Number of grooves on this crossbar.
    /// Front: 2 grooves (for door rails), Back: 1 groove (for panel), Side: 1 groove (for panel).
    /// </summary>
    public int GrooveCount { get; set; }
}
