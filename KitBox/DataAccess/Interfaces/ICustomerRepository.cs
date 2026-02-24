using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Customer data access operations.
/// </summary>
public interface ICustomerRepository
{
    Customer? GetById(int id);
    List<Customer> GetAll();
    void Add(Customer customer);
    void Update(Customer customer);
    void Delete(int id);
}
