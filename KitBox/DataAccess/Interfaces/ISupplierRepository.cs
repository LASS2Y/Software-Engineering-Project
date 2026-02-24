using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Supplier data access operations.
/// </summary>
public interface ISupplierRepository
{
    Supplier? GetById(int id);
    List<Supplier> GetAll();
    void Add(Supplier supplier);
    void Update(Supplier supplier);
    void Delete(int id);
}
