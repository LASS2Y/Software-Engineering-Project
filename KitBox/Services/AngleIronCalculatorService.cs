using System.Collections.Generic;
using System.Linq;
using KitBox.Models;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

public class AngleIronCalculatorService : IAngleIronCalculatorService
{
    private readonly ICatalogService _catalog;

    public AngleIronCalculatorService(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// TotalHeight = batten height + 2 × crossbar height (one on top, one on bottom).
    /// </summary>
    public double CalculateLockerTotalHeight(Locker locker)
        => locker.Height + 2 * _catalog.CrossbarHeight;

    /// <summary>
    /// Angle iron length = sum of all locker TotalHeights.
    /// The 4 angle irons are all cut to this same length.
    /// </summary>
    public double CalculateAngleIronLength(List<Locker> lockers)
        => lockers.Sum(l => CalculateLockerTotalHeight(l));
}
