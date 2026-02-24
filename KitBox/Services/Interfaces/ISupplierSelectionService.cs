using KitBox.Models;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Implements the supplier selection business rule:
/// choose the supplier with the lowest price for a part;
/// if prices are equal, choose the shortest delivery time.
/// </summary>
public interface ISupplierSelectionService
{
    /// <summary>
    /// Returns the best SupplierPart entry for the given part,
    /// or null if no supplier provides it.
    /// </summary>
    SupplierPart? GetBestSupplier(int partId);
}
