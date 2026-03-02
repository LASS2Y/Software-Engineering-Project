using System;

namespace KitBox.Models;

/// <summary>
/// Represents an invoice (facture) issued to a customer after delivery.
/// Customers pay upon receipt of parts, then receive this bill.
/// </summary>
public class Bill
{
    public int Id { get; set; }

    /// <summary>
    /// Date when the bill was issued.
    /// </summary>
    public DateTime EmissionDate { get; set; }

    /// <summary>
    /// Total amount of the bill.
    /// </summary>
    public decimal Amount { get; set; }
}
