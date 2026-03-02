using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Order data access operations.
/// </summary>
public interface IOrderRepository
{
    Order? GetById(int id);
    List<Order> GetAll();
    /// <summary>
    /// Returns all orders joined with customer info and computed line totals.
    /// </summary>
    List<Order> GetAllWithDetails();
    List<Order> GetByCustomerId(int customerId);
    void Add(Order order);
    void Update(Order order);
    void Delete(int id);
}
