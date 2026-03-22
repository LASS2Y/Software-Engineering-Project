using System;

namespace KitBox.Models;

/// <summary>
/// Represents an automatic purchase order sent to a supplier
/// when customer demand exceeds current stock.
/// </summary>
public class SupplierOrder
{
    public int Id { get; set; }

    public int? CustomerOrderId { get; set; }

    public int PartId { get; set; }

    public int SupplierId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public int DeliveryDays { get; set; }

    public DateTime OrderedAt { get; set; }

    public DateTime ExpectedDeliveryDate { get; set; }

    public string Status { get; set; } = "Ordered";
}