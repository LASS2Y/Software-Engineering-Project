using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for Locker data access operations.
/// </summary>
public interface ILockerRepository
{
    Locker? GetById(int id);
    List<Locker> GetByCabinetId(int cabinetId);
    void Add(Locker locker);
    void Update(Locker locker);
    void Delete(int id);
}
