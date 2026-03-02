using KitBox.Models.Enums;

namespace KitBox.Models.Parts;

/// <summary>
/// Represents a panel used in a locker (horizontal, side, or back).
/// A locker contains: 2 horizontal panels, 2 side panels, and 1 back panel.
/// All panels in the same locker share the same color.
/// </summary>
public class Panel : Part
{
    /// <summary>
    /// The type of panel (Horizontal, Side, or Back).
    /// </summary>
    public PanelType Type { get; set; }
}
