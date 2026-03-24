using System;
using System.Collections.Generic;
using System.Linq;
using KitBox.DataAccess.Interfaces;
using KitBox.Models.Enums;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

public class SupplierOrderTrackingService : ISupplierOrderTrackingService
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IPartRepository _partRepository;
    private readonly IOrderRepository _orderRepository;

    public SupplierOrderTrackingService(
        ISupplierOrderRepository supplierOrderRepository,
        IPartRepository partRepository,
        IOrderRepository orderRepository)
    {
        _supplierOrderRepository = supplierOrderRepository;
        _partRepository = partRepository;
        _orderRepository = orderRepository;
    }

    public List<Models.SupplierOrder> GetAll()
        => _supplierOrderRepository.GetAll();

    public void TransitionStatus(int supplierOrderId, string newStatus)
    {
        string normalized = NormalizeStatus(newStatus);
        var supplierOrder = _supplierOrderRepository.GetById(supplierOrderId)
            ?? throw new InvalidOperationException($"Supplier order #{supplierOrderId} was not found.");

        if (string.Equals(supplierOrder.Status, normalized, StringComparison.OrdinalIgnoreCase))
            return;

        ValidateTransition(supplierOrder.Status, normalized);
        _supplierOrderRepository.UpdateStatus(supplierOrder.Id, normalized);

        if (string.Equals(normalized, "Received", StringComparison.OrdinalIgnoreCase))
        {
            var part = _partRepository.GetById(supplierOrder.PartId)
                ?? throw new InvalidOperationException($"Part id={supplierOrder.PartId} was not found.");
            _partRepository.UpdateStock(part.Id, part.StockQuantity + supplierOrder.Quantity);
        }

        RefreshCustomerOrderAvailability();
    }

    public void RefreshCustomerOrderAvailability()
    {
        var candidates = _orderRepository.GetAll()
            .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.PartiallyAvailable)
            .ToList();

        foreach (var order in candidates)
        {
            var linkedSupplierOrders = _supplierOrderRepository.GetByCustomerOrderId(order.Id);
            if (linkedSupplierOrders.Count == 0)
                continue;

            bool allReceived = linkedSupplierOrders.All(
                so => string.Equals(so.Status, "Received", StringComparison.OrdinalIgnoreCase));

            if (!allReceived)
                continue;

            order.Status = OrderStatus.Available;
            order.AvailableDate = DateTime.Today;
            _orderRepository.Update(order);
        }
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new InvalidOperationException("Supplier status cannot be empty.");

        return status.Trim().ToLowerInvariant() switch
        {
            "ordered" => "Ordered",
            "intransit" => "InTransit",
            "received" => "Received",
            "cancelled" => "Cancelled",
            _ => throw new InvalidOperationException($"Unsupported supplier status '{status}'.")
        };
    }

    private static void ValidateTransition(string currentStatus, string nextStatus)
    {
        string current = NormalizeStatus(currentStatus);
        if (current == nextStatus)
            return;

        bool allowed = (current, nextStatus) switch
        {
            ("Ordered", "InTransit") => true,
            ("Ordered", "Received") => true,
            ("Ordered", "Cancelled") => true,
            ("InTransit", "Received") => true,
            ("InTransit", "Cancelled") => true,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Invalid transition from '{current}' to '{nextStatus}'.");
        }
    }
}
