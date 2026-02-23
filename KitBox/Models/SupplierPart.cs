namespace KitBox.Models;

/// <summary>
/// Represents the relationship between a supplier and a part they can provide.
/// Contains the supplier's price and delivery time for a specific part.
/// A secretary regularly updates prices from supplier catalogs.
/// </summary>
public class SupplierPart
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int PartId { get; set; }

    /// <summary>
    /// Price offered by this supplier for the part.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Delivery time in business days.
    /// </summary>
    public int DeliveryDays { get; set; }

    /// <summary>
    /// Navigation property to the supplier.
    /// </summary>
    public Supplier? Supplier { get; set; }
}
