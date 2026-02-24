using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Bill data access operations.
/// </summary>
public interface IBillRepository
{
    Bill? GetById(int id);
    void Add(Bill bill);
}
