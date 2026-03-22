using System.Collections.Generic;
using KitBox.Models.Parts;

namespace KitBox.Services.Interfaces;

public record StockReplenishmentSuggestion(
    int PartId,
    string PartName,
    string PartReference,
    int CurrentStock,
    int MinimumStock,
    int QuantityToOrder,
    int? SupplierId,
    decimal? SupplierPrice,
    int? DeliveryDays
);

public record StockReplenishmentOrderResult(
    int SupplierOrderId,
    int PartId,
    int SupplierId,
    int Quantity,
    decimal UnitCost,
    int DeliveryDays
);

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

    /// <summary>
    /// Computes a minimum stock based on sales history for one part.
    /// </summary>
    int CalculateMinimumStockFromSalesHistory(
        int partId,
        int historyDays = 90,
        int coverageDays = 30,
        int fallbackMinimum = 5);

    /// <summary>
    /// Recomputes and persists minimum stock for all parts using sales history.
    /// </summary>
    void RefreshMinimumStockFromSalesHistory(
        int historyDays = 90,
        int coverageDays = 30,
        int fallbackMinimum = 5);

    /// <summary>
    /// Returns reorder suggestions for parts under dynamic minimum stock,
    /// including the best supplier by price then delivery time.
    /// </summary>
    List<StockReplenishmentSuggestion> GetReplenishmentSuggestions(
        int historyDays = 90,
        int coverageDays = 30,
        int fallbackMinimum = 5);

    /// <summary>
    /// Places a replenishment supplier order for a part.
    /// If quantity is null, minimum quantity needed to reach minimum stock is used.
    /// </summary>
    StockReplenishmentOrderResult PlaceReplenishmentOrder(int partId, int? quantity = null);
}
