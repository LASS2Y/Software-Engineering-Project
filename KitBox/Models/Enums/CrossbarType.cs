namespace KitBox.Models.Enums;

/// <summary>
/// Types of crossbars (traverses) used in a locker.
/// </summary>
public enum CrossbarType
{
    /// <summary>
    /// Front crossbar with 2 grooves for door rails (2 per locker).
    /// </summary>
    Front,

    /// <summary>
    /// Back crossbar with 1 groove for the back panel (2 per locker).
    /// </summary>
    Back,

    /// <summary>
    /// Side crossbar with 1 groove for a side panel (4 per locker).
    /// </summary>
    Side
}
