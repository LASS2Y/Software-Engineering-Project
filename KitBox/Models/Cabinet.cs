using System.Collections.Generic;
using System.Linq;

namespace KitBox.Models;

/// <summary>
/// Represents a cabinet (armoire) composed of up to 7 lockers held together by 4 angle irons.
/// The angle iron length equals the sum of all locker total heights.
/// </summary>
public class Cabinet
{
    /// <summary>
    /// Maximum number of lockers allowed in a single cabinet.
    /// </summary>
    public const int MaxLockers = 7;

    public int Id { get; set; }

    public int OrderId { get; set; }

    /// <summary>
    /// Color chosen for the 4 angle irons (from the catalog color range).
    /// </summary>
    public string AngleIronColor { get; set; } = string.Empty;

    /// <summary>
    /// The lockers that compose this cabinet (1 to 7).
    /// </summary>
    public List<Locker> Lockers { get; set; } = new();

    /// <summary>
    /// Required angle iron length in centimeters.
    /// Calculated as the sum of all locker total heights.
    /// </summary>
    public double AngleIronLength => Lockers.Sum(l => l.TotalHeight);
}
