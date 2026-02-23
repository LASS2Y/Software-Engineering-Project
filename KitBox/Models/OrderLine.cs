namespace KitBox.Models;

/// <summary>
/// Represents a single line in an order, linking a specific part to a quantity.
/// Since parts are stored individually (not as assembled lockers),
/// each line references a specific part with its quantity and price at time of order.
/// </summary>
public class OrderLine
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int PartId { get; set; }

    /// <summary>
    /// Number of units of this part required.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit at the time the order was placed.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price for this line (Quantity × UnitPrice).
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;
}
