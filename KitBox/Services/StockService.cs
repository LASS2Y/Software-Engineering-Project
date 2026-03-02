using System;
using System.Collections.Generic;
using KitBox.DataAccess.Interfaces;
using KitBox.Models.Parts;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

public class StockService : IStockService
{
    private readonly IPartRepository _partRepository;

    public StockService(IPartRepository partRepository)
    {
        _partRepository = partRepository;
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
}
