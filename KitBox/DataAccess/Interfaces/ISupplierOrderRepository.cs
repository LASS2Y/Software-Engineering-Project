using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for automatic supplier purchase orders.
/// </summary>
public interface ISupplierOrderRepository
{
    List<SupplierOrder> GetByCustomerOrderId(int customerOrderId);
    void Add(SupplierOrder supplierOrder);
}