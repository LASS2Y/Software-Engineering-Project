using System;
using System.Collections.Generic;
using KitBox.DataAccess.Interfaces;
using KitBox.Models.Parts;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

public class StockService : IStockService
{
    private readonly IPartRepository _partRepository;
    private readonly IOrderLineRepository _orderLineRepository;
    private readonly ISupplierSelectionService _supplierSelectionService;
    private readonly ISupplierOrderRepository _supplierOrderRepository;

    public StockService(
        IPartRepository partRepository,
        IOrderLineRepository orderLineRepository,
        ISupplierSelectionService supplierSelectionService,
        ISupplierOrderRepository supplierOrderRepository)
    {
        _partRepository = partRepository;
        _orderLineRepository = orderLineRepository;
        _supplierSelectionService = supplierSelectionService;
        _supplierOrderRepository = supplierOrderRepository;
    }

    public bool IsAvailable(int partId, int quantity)
    {
        var part = _partRepository.GetById(partId);
        return part != null && part.StockQuantity >= quantity;
    }

    public List<Part> GetLowStockParts()
        => _partRepository.GetLowStock();

    public void DeductStock(int partId, int quantity)
    {
        var part = _partRepository.GetById(partId)
            ?? throw new InvalidOperationException($"Part id={partId} not found.");

        if (part.StockQuantity < quantity)
            throw new InvalidOperationException(
                $"Insufficient stock for '{part.Name}': requested {quantity}, available {part.StockQuantity}.");

        _partRepository.UpdateStock(partId, part.StockQuantity - quantity);
    }

    public void AddStock(int partId, int quantity)
    {
        var part = _partRepository.GetById(partId)
            ?? throw new InvalidOperationException($"Part id={partId} not found.");

        _partRepository.UpdateStock(partId, part.StockQuantity + quantity);
    }

    public int CalculateMinimumStockFromSalesHistory(
        int partId,
        int historyDays = 90,
        int coverageDays = 30,
        int fallbackMinimum = 5)
    {
        if (historyDays <= 0) throw new ArgumentOutOfRangeException(nameof(historyDays));
        if (coverageDays <= 0) throw new ArgumentOutOfRangeException(nameof(coverageDays));
        if (fallbackMinimum < 0) throw new ArgumentOutOfRangeException(nameof(fallbackMinimum));

        DateTime startDate = DateTime.Today.AddDays(-historyDays);
        int sold = _orderLineRepository.GetSoldQuantityByPartSince(partId, startDate);

        if (sold <= 0)
            return fallbackMinimum;

        double averageDailySales = sold / (double)historyDays;
        int recommended = (int)Math.Ceiling(averageDailySales * coverageDays);
        return Math.Max(fallbackMinimum, recommended);
    }

    public void RefreshMinimumStockFromSalesHistory(
        int historyDays = 90,
        int coverageDays = 30,
        int fallbackMinimum = 5)
    {
        var parts = _partRepository.GetAll();
        foreach (var part in parts)
        {
            int minimum = CalculateMinimumStockFromSalesHistory(
                part.Id,
                historyDays,
                coverageDays,
                fallbackMinimum);

            if (part.MinimumStock != minimum)
            {
                _partRepository.UpdateMinimumStock(part.Id, minimum);
            }
        }
    }

    public List<StockReplenishmentSuggestion> GetReplenishmentSuggestions(
        int historyDays = 90,
        int coverageDays = 30,
        int fallbackMinimum = 5)
    {
        RefreshMinimumStockFromSalesHistory(historyDays, coverageDays, fallbackMinimum);

        var suggestions = new List<StockReplenishmentSuggestion>();
        var lowStockParts = _partRepository.GetLowStock();

        foreach (var part in lowStockParts)
        {
            int quantityToOrder = Math.Max(0, part.MinimumStock - part.StockQuantity);
            if (quantityToOrder == 0)
                continue;

            var supplier = _supplierSelectionService.GetBestSupplier(part.Id);

            suggestions.Add(new StockReplenishmentSuggestion(
                part.Id,
                part.Name,
                part.Reference,
                part.StockQuantity,
                part.MinimumStock,
                quantityToOrder,
                supplier?.SupplierId,
                supplier?.Price,
                supplier?.DeliveryDays));
        }

        return suggestions;
    }

    public StockReplenishmentOrderResult PlaceReplenishmentOrder(int partId, int? quantity = null)
    {
        var part = _partRepository.GetById(partId)
            ?? throw new InvalidOperationException($"Part id={partId} not found.");

        int computedQuantity = quantity ?? Math.Max(0, part.MinimumStock - part.StockQuantity);
        if (computedQuantity <= 0)
            throw new InvalidOperationException("No replenishment required for this part.");

        var supplier = _supplierSelectionService.GetBestSupplier(part.Id)
            ?? throw new InvalidOperationException("No supplier available for this part.");

        var supplierOrder = new Models.SupplierOrder
        {
            CustomerOrderId = null,
            PartId = part.Id,
            SupplierId = supplier.SupplierId,
            Quantity = computedQuantity,
            UnitCost = supplier.Price,
            DeliveryDays = supplier.DeliveryDays,
            OrderedAt = DateTime.Today,
            ExpectedDeliveryDate = DateTime.Today.AddDays(supplier.DeliveryDays),
            Status = "Ordered"
        };

        _supplierOrderRepository.Add(supplierOrder);

        return new StockReplenishmentOrderResult(
            supplierOrder.Id,
            part.Id,
            supplier.SupplierId,
            computedQuantity,
            supplier.Price,
            supplier.DeliveryDays);
    }
}
