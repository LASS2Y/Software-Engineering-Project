namespace KitBox.Models.Parts;

/// <summary>
/// Abstract base class for all parts that compose a locker or cabinet.
/// New part types (e.g., shelves, drawers) can be added by inheriting from this class
/// without modifying existing code (Open/Closed Principle).
/// </summary>
public abstract class Part
{
    public int Id { get; set; }

    /// <summary>
    /// Catalog reference code for this part.
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive name of the part.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Height of the part in centimeters.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Width of the part in centimeters.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Depth of the part in centimeters.
    /// </summary>
    public double Depth { get; set; }

    /// <summary>
    /// Color of the part.
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Current selling price per unit.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Current quantity available in stock.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum stock level determined from sales history.
    /// </summary>
    public int MinimumStock { get; set; }
}
