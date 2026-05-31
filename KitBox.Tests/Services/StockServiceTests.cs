using FluentAssertions;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;
using KitBox.Models.Parts;
using KitBox.Services;
using KitBox.Services.Interfaces;
using Moq;
using Xunit;

namespace KitBox.Tests.Services;

/// <summary>
/// Unit tests for StockService.
/// Covers availability checks, stock deduction/addition,
/// minimum-stock calculation, replenishment suggestions and orders.
/// </summary>
public class StockServiceTests
{
    private readonly Mock<IPartRepository>            _partRepoMock      = new();
    private readonly Mock<IOrderLineRepository>       _orderLineRepoMock = new();
    private readonly Mock<ISupplierSelectionService>  _supplierSvcMock   = new();
    private readonly Mock<ISupplierOrderRepository>   _supplierOrderMock = new();
    private readonly StockService _sut;

    public StockServiceTests()
    {
        _sut = new StockService(
            _partRepoMock.Object,
            _orderLineRepoMock.Object,
            _supplierSvcMock.Object,
            _supplierOrderMock.Object);
    }

    // ── IsAvailable ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsAvailable_WhenPartHasSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 10);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        // Act
        bool result = _sut.IsAvailable(1, quantity: 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WhenPartHasExactStock_ShouldReturnTrue()
    {
        // Arrange
        var part = MakePart(id: 2, stock: 5);
        _partRepoMock.Setup(r => r.GetById(2)).Returns(part);

        // Act
        bool result = _sut.IsAvailable(2, quantity: 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WhenPartHasInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var part = MakePart(id: 3, stock: 2);
        _partRepoMock.Setup(r => r.GetById(3)).Returns(part);

        // Act
        bool result = _sut.IsAvailable(3, quantity: 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_WhenPartNotFound_ShouldReturnFalse()
    {
        // Arrange
        _partRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Part?)null);

        // Act
        bool result = _sut.IsAvailable(999, quantity: 1);

        // Assert
        result.Should().BeFalse();
    }

    // ── GetLowStockParts ─────────────────────────────────────────────────────────

    [Fact]
    public void GetLowStockParts_ShouldReturn_PartsFromRepository()
    {
        // Arrange
        var lowStockParts = new List<Part> { MakePart(id: 1, stock: 0), MakePart(id: 2, stock: 1) };
        _partRepoMock.Setup(r => r.GetLowStock()).Returns(lowStockParts);

        // Act
        var result = _sut.GetLowStockParts();

        // Assert
        result.Should().BeEquivalentTo(lowStockParts);
    }

    [Fact]
    public void GetLowStockParts_WhenNoParts_ShouldReturnEmptyList()
    {
        // Arrange
        _partRepoMock.Setup(r => r.GetLowStock()).Returns(new List<Part>());

        // Act
        var result = _sut.GetLowStockParts();

        // Assert
        result.Should().BeEmpty();
    }

    // ── DeductStock ──────────────────────────────────────────────────────────────

    [Fact]
    public void DeductStock_WhenSufficientStock_ShouldCallUpdateStock_WithReducedValue()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 10);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        // Act
        _sut.DeductStock(1, quantity: 4);

        // Assert
        _partRepoMock.Verify(r => r.UpdateStock(1, 6), Times.Once);
    }

    [Fact]
    public void DeductStock_WhenInsufficientStock_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 2);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        // Act
        Action act = () => _sut.DeductStock(1, quantity: 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public void DeductStock_WhenPartNotFound_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        _partRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Part?)null);

        // Act
        Action act = () => _sut.DeductStock(999, quantity: 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*not found*");
    }

    // ── AddStock ─────────────────────────────────────────────────────────────────

    [Fact]
    public void AddStock_ShouldCallUpdateStock_WithIncreasedValue()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 5);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        // Act
        _sut.AddStock(1, quantity: 10);

        // Assert
        _partRepoMock.Verify(r => r.UpdateStock(1, 15), Times.Once);
    }

    [Fact]
    public void AddStock_WhenPartNotFound_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        _partRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Part?)null);

        // Act
        Action act = () => _sut.AddStock(999, quantity: 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*not found*");
    }

    // ── CalculateMinimumStockFromSalesHistory ────────────────────────────────────

    [Fact]
    public void CalculateMinimumStock_WhenNoSalesHistory_ShouldReturn_FallbackMinimum()
    {
        // Arrange
        _orderLineRepoMock.Setup(r => r.GetSoldQuantityByPartSince(It.IsAny<int>(), It.IsAny<DateTime>()))
                          .Returns(0);

        // Act
        int result = _sut.CalculateMinimumStockFromSalesHistory(1,
            historyDays: 90, coverageDays: 30, fallbackMinimum: 5);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void CalculateMinimumStock_WithSalesHistory_ShouldReturn_CalculatedMinimum()
    {
        // Arrange – 90 units sold in 90 days = 1/day average; coverage 30 days → need 30
        _orderLineRepoMock.Setup(r => r.GetSoldQuantityByPartSince(1, It.IsAny<DateTime>()))
                          .Returns(90);

        // Act
        int result = _sut.CalculateMinimumStockFromSalesHistory(1,
            historyDays: 90, coverageDays: 30, fallbackMinimum: 5);

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public void CalculateMinimumStock_WhenCalculatedIsLessThanFallback_ShouldReturn_Fallback()
    {
        // Arrange – only 9 units in 90 days → average 0.1/day → coverage 30 days = 3, below fallback 5
        _orderLineRepoMock.Setup(r => r.GetSoldQuantityByPartSince(1, It.IsAny<DateTime>()))
                          .Returns(9);

        // Act
        int result = _sut.CalculateMinimumStockFromSalesHistory(1,
            historyDays: 90, coverageDays: 30, fallbackMinimum: 5);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void CalculateMinimumStock_WithInvalidHistoryDays_ShouldThrow_ArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => _sut.CalculateMinimumStockFromSalesHistory(1, historyDays: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CalculateMinimumStock_WithInvalidCoverageDays_ShouldThrow_ArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => _sut.CalculateMinimumStockFromSalesHistory(1,
            historyDays: 90, coverageDays: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CalculateMinimumStock_WithNegativeFallback_ShouldThrow_ArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => _sut.CalculateMinimumStockFromSalesHistory(1,
            historyDays: 90, coverageDays: 30, fallbackMinimum: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── PlaceReplenishmentOrder ───────────────────────────────────────────────────

    [Fact]
    public void PlaceReplenishmentOrder_WhenPartNotFound_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        _partRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Part?)null);

        // Act
        Action act = () => _sut.PlaceReplenishmentOrder(999);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public void PlaceReplenishmentOrder_WhenNoSupplierAvailable_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 2, minimumStock: 10);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);
        _supplierSvcMock.Setup(s => s.GetBestSupplier(1)).Returns((SupplierPart?)null);

        // Act
        Action act = () => _sut.PlaceReplenishmentOrder(1);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*No supplier*");
    }

    [Fact]
    public void PlaceReplenishmentOrder_WhenStockAlreadySufficient_ShouldThrow_InvalidOperationException()
    {
        // Arrange – stock >= minimum, no replenishment needed
        var part = MakePart(id: 1, stock: 10, minimumStock: 5);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        // Act
        Action act = () => _sut.PlaceReplenishmentOrder(1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*No replenishment required*");
    }

    [Fact]
    public void PlaceReplenishmentOrder_WithValidPartAndSupplier_ShouldReturn_CorrectResult()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 2, minimumStock: 10);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        var supplier = new SupplierPart { SupplierId = 5, PartId = 1,
                                          Price = 4.50m, DeliveryDays = 7 };
        _supplierSvcMock.Setup(s => s.GetBestSupplier(1)).Returns(supplier);
        _supplierOrderMock.Setup(r => r.Add(It.IsAny<SupplierOrder>()));

        // Act
        var result = _sut.PlaceReplenishmentOrder(1);

        // Assert
        result.PartId.Should().Be(1);
        result.SupplierId.Should().Be(5);
        result.Quantity.Should().Be(8);   // minimumStock(10) - stock(2)
        result.UnitCost.Should().Be(4.50m);
        result.DeliveryDays.Should().Be(7);
    }

    [Fact]
    public void PlaceReplenishmentOrder_WithExplicitQuantity_ShouldUse_ThatQuantity()
    {
        // Arrange
        var part = MakePart(id: 1, stock: 2, minimumStock: 10);
        _partRepoMock.Setup(r => r.GetById(1)).Returns(part);

        var supplier = new SupplierPart { SupplierId = 5, PartId = 1,
                                          Price = 4.50m, DeliveryDays = 7 };
        _supplierSvcMock.Setup(s => s.GetBestSupplier(1)).Returns(supplier);
        _supplierOrderMock.Setup(r => r.Add(It.IsAny<SupplierOrder>()));

        // Act
        var result = _sut.PlaceReplenishmentOrder(1, quantity: 20);

        // Assert
        result.Quantity.Should().Be(20);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static Batten MakePart(int id, int stock, int minimumStock = 5) =>
        new() { Id = id, Name = $"Part-{id}", Reference = $"REF{id}",
                StockQuantity = stock, MinimumStock = minimumStock };
}
