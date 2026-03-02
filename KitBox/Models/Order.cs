using System;
using System.Collections.Generic;
using KitBox.Models.Enums;

namespace KitBox.Models;

/// <summary>
/// Represents a customer order containing one or more cabinets.
/// If all parts are not in stock, the customer pays a deposit and picks up later.
/// Payment is made upon receipt of parts, then an invoice (Bill) is issued.
/// </summary>
public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int? BillId { get; set; }

    /// <summary>
    /// Date when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Deposit amount paid if parts are not immediately available.
    /// Null if no deposit is required.
    /// </summary>
    public decimal? Deposit { get; set; }

    /// <summary>
    /// Date when all parts become available for pickup.
    /// Null if parts are already available or not yet determined.
    /// </summary>
    public DateTime? AvailableDate { get; set; }

    /// <summary>
    /// Current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Navigation property to the customer who placed the order.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property to the invoice issued after delivery.
    /// </summary>
    public Bill? Bill { get; set; }

    /// <summary>
    /// Individual part lines making up this order.
    /// </summary>
    public List<OrderLine> Lines { get; set; } = new();

    /// <summary>
    /// Cabinets ordered in this order.
    /// </summary>
    public List<Cabinet> Cabinets { get; set; } = new();
}
