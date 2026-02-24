using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Cabinet data access operations.
/// </summary>
public interface ICabinetRepository
{
    Cabinet? GetById(int id);
    List<Cabinet> GetByOrderId(int orderId);
    void Add(Cabinet cabinet);
    void Update(Cabinet cabinet);
    void Delete(int id);
}
