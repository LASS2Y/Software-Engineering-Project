using System.Collections.Generic;
using KitBox.Models;

namespace KitBox.Services.Interfaces;

/// <summary>
/// Handles supplier-order lifecycle transitions and downstream stock/customer-order effects.
/// </summary>
public interface ISupplierOrderTrackingService
{
    List<SupplierOrder> GetAll();
    void TransitionStatus(int supplierOrderId, string newStatus);
    void RefreshCustomerOrderAvailability();
}
