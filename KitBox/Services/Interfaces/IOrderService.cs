using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Represents the availability of a single part required by an order.
/// </summary>
public record PartAvailability(
    string PartName,
    string Reference,
    int Required,
    int InStock,
    bool IsAvailable,
    decimal UnitPrice
);

/// <summary>
/// Full result of computing the parts needed for a cabinet.
/// </summary>
public record OrderPreview(
    List<PartAvailability> Parts,
    double AngleIronLength,
    string AngleIronColor,
    decimal TotalPrice,
    bool AllPartsAvailable
);

/// <summary>
/// Orchestrates order creation: computes required parts from locker configs,
/// checks stock, handles deposit logic, and persists the order.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Computes all parts required for the list of lockers and previews
    /// availability and pricing before confirming.
    /// </summary>
    OrderPreview PreviewOrder(List<Locker> lockers, string angleIronColor);

    /// <summary>
    /// Persists a confirmed order. Deducts stock for available parts.
    /// Sets deposit and available_date when stock is partial.
    /// Returns the created Order.
    /// </summary>
    Order PlaceOrder(Customer customer, List<Locker> lockers,
                     string angleIronColor, decimal depositAmount);
}
