using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for automatic supplier purchase orders.
/// </summary>
public interface ISupplierOrderRepository
{
    SupplierOrder? GetById(int id);
    List<SupplierOrder> GetAll();
    List<SupplierOrder> GetByCustomerOrderId(int customerOrderId);
    void UpdateStatus(int id, string status);
    void Add(SupplierOrder supplierOrder);
}