namespace KitBox.Models.Enums;

/// <summary>
/// Types of panels used in a locker.
/// </summary>
public enum PanelType
{
    /// <summary>
    /// Horizontal panels resting on crossbars (2 per locker).
    /// </summary>
    Horizontal,

    /// <summary>
    /// Side panels sliding into grooves of battens and crossbars (2 per locker).
    /// </summary>
    Side,

    /// <summary>
    /// Back panel sliding into grooves of battens and a crossbar (1 per locker).
    /// </summary>
    Back
}
