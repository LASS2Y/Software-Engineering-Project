using System.Collections.Generic;

namespace KitBox.Models;

/// <summary>
/// Represents a supplier (fournisseur) that provides parts.
/// Each part can be supplied by multiple suppliers.
/// The supplier is chosen based on best price, or best delivery time if prices are equal.
/// </summary>
public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Catalog of parts this supplier can provide, with prices and delivery times.
    /// </summary>
    public List<SupplierPart> SupplierParts { get; set; } = new();

    public override string ToString() => Name;
}
