using System.Collections.Generic;
using System.Linq;
using KitBox.Models;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

public class LockerValidationService : ILockerValidationService
{
    private readonly ICatalogService _catalog;

    public LockerValidationService(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    public bool AreDimensionsValid(double height, double width, double depth, bool hasDoors)
    {
        var validHeights = _catalog.GetAvailableHeights();
        var validWidths  = hasDoors ? _catalog.GetAvailableWidthsWithDoors()
                                    : _catalog.GetAvailableWidths();
        var validDepths  = _catalog.GetAvailableDepths();

        return validHeights.Contains(height)
            && validWidths.Contains(width)
            && validDepths.Contains(depth);
    }

    public bool IsLockerCountValid(int lockerCount)
        => lockerCount >= 1 && lockerCount <= Cabinet.MaxLockers;

    public List<string> ValidateCabinet(List<Locker> lockers)
    {
        var errors = new List<string>();

        if (!IsLockerCountValid(lockers.Count))
            errors.Add($"A cabinet must have between 1 and {Cabinet.MaxLockers} lockers (currently {lockers.Count}).");

        // All lockers in the same cabinet must share the same width
        if (lockers.Count > 0)
        {
            double refWidth = lockers[0].Width;
            foreach (var l in lockers)
            {
                if (l.Width != refWidth)
                    errors.Add("All lockers in a cabinet must have the same width.");

                if (!AreDimensionsValid(l.Height, l.Width, l.Depth, l.HasDoors))
                    errors.Add($"Locker dimensions {l.Height}×{l.Width}×{l.Depth} cm" +
                               $"{(l.HasDoors ? " (with doors)" : "")} are not available in the catalog.");

                if (l.HasDoors && string.IsNullOrWhiteSpace(l.DoorColor))
                    errors.Add("Lockers with doors must have a door color selected.");
            }
        }

        return errors;
    }
}
