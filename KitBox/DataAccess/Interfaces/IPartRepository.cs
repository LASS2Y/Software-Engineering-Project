using System.Collections.Generic;
using KitBox.Models.Parts;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Part data access operations.
/// Handles all part types via Single Table Inheritance.
/// </summary>
public interface IPartRepository
{
    Part? GetById(int id);
    Part? GetByReference(string reference);
    List<Part> GetAll();
    List<Part> GetByType(string partType);
    List<Part> GetLowStock();
    void Add(Part part);
    void Update(Part part);
    void UpdateStock(int id, int quantity);
    void UpdateMinimumStock(int id, int minimumStock);
    void Delete(int id);
}
