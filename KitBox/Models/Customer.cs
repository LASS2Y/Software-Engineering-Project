using System.Collections.Generic;

namespace KitBox.Models;

/// <summary>
/// Represents a customer who places orders.
/// </summary>
public class Customer
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Orders placed by this customer.
    /// </summary>
    public List<Order> Orders { get; set; } = new();
}
