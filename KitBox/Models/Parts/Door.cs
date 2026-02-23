namespace KitBox.Models.Parts;

/// <summary>
/// Represents a door that can be optionally added to a locker.
/// Each locker can have 0 or 2 doors. Door color can differ from the locker panel color.
/// Door widths are limited (see catalog for available widths).
/// Cup handles are not available for glass doors.
/// </summary>
public class Door : Part
{
    /// <summary>
    /// Whether this door is made of glass.
    /// Glass doors cannot have cup-type handles.
    /// </summary>
    public bool IsGlass { get; set; }
}
