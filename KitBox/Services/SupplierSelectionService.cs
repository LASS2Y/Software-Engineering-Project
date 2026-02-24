using KitBox.DataAccess.Interfaces;
using KitBox.Models;
using KitBox.Services.Interfaces;

namespace KitBox.Services;

public class SupplierSelectionService : ISupplierSelectionService
{
    private readonly ISupplierPartRepository _supplierPartRepository;

    public SupplierSelectionService(ISupplierPartRepository supplierPartRepository)
    {
        _supplierPartRepository = supplierPartRepository;
    }

    /// <summary>
    /// Returns the SupplierPart with the lowest price for the given part.
    /// If prices are equal, the one with the shortest delivery time is selected.
    /// This implements the business rule from context.md.
    /// </summary>
    public SupplierPart? GetBestSupplier(int partId)
        => _supplierPartRepository.GetBestSupplierForPart(partId);
}
