using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Calculates angle iron dimensions for a cabinet.
/// Angle irons hold all lockers together; there are always 4 per cabinet.
/// Length = sum of all locker TotalHeights.
/// </summary>
public interface IAngleIronCalculatorService
{
    /// <summary>
    /// Calculates the required angle iron length for the given list of lockers.
    /// </summary>
    double CalculateAngleIronLength(List<Locker> lockers);

    /// <summary>
    /// Returns the total height of a single locker:
    /// TotalHeight = BattenHeight + 2 × CrossbarHeight.
    /// </summary>
    double CalculateLockerTotalHeight(Locker locker);
}
