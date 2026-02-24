using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for SupplierPart (supplier catalog) data access.
/// Supports the business rule: choose supplier by best price, then best delivery time.
/// </summary>
public interface ISupplierPartRepository
{
    List<SupplierPart> GetByPartId(int partId);
    List<SupplierPart> GetBySupplierId(int supplierId);
    SupplierPart? GetBestSupplierForPart(int partId);
    void Add(SupplierPart supplierPart);
    void Update(SupplierPart supplierPart);
    void Delete(int id);
}
