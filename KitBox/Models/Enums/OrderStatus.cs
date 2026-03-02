namespace KitBox.Models.Enums;

/// <summary>
/// Possible statuses for a customer order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been placed but not yet processed.
    /// </summary>
    Pending,

    /// <summary>
    /// Some parts are available, others are on order. Deposit required.
    /// </summary>
    PartiallyAvailable,

    /// <summary>
    /// All parts are available and ready for pickup.
    /// </summary>
    Available,

    /// <summary>
    /// Order has been delivered to the customer.
    /// </summary>
    Delivered,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled
}
