using System.Collections.Generic;
using System;
using KitBox.Models;

namespace KitBox.DataAccess.Interfaces;

/// <summary>
/// Repository interface for OrderLine data access operations.
/// </summary>
public interface IOrderLineRepository
{
    List<OrderLine> GetByOrderId(int orderId);
    int GetSoldQuantityByPartSince(int partId, DateTime startDate);
    void Add(OrderLine orderLine);
    void Delete(int id);
}
