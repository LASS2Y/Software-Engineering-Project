namespace KitBox.Models.Parts;

/// <summary>
/// Represents an angle iron (cornière) that holds lockers together in a cabinet.
/// A cabinet requires exactly 4 angle irons.
/// Available in standard lengths but can be cut to fit the total height of all lockers.
/// Angle irons can be chosen in a range of colors (see catalog).
/// </summary>
public class AngleIron : Part
{
    /// <summary>
    /// The standard (uncut) length of this angle iron in centimeters.
    /// </summary>
    public double StandardLength { get; set; }
}
