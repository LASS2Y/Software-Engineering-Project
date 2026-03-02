using System.Collections.Generic;
using KitBox.Models.Parts;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Manages stock levels and checks part availability.
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Returns true if at least <paramref name="quantity"/> units of the part are in stock.
    /// </summary>
    bool IsAvailable(int partId, int quantity);

    /// <summary>
    /// Returns all parts whose stock_quantity is below minimum_stock.
    /// </summary>
    List<Part> GetLowStockParts();

    /// <summary>
    /// Decrements the stock for a part by <paramref name="quantity"/> units.
    /// Throws if insufficient stock.
    /// </summary>
    void DeductStock(int partId, int quantity);

    /// <summary>
    /// Increments the stock for a part (e.g. after supplier delivery).
    /// </summary>
    void AddStock(int partId, int quantity);
}
