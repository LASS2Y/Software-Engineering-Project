using FluentAssertions;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;
using KitBox.Models.Enums;
using KitBox.Models.Parts;
using KitBox.Services;
using KitBox.Services.Interfaces;
using Moq;
using Xunit;

namespace KitBox.Tests.Services;

/// <summary>
/// Unit tests for OrderService.
/// Covers PreviewOrder (availability checks, pricing) and PlaceOrder
/// (status assignment, deposit logic, stock deduction, supplier orders).
/// </summary>
public class OrderServiceTests
{
    // ── Mocks ────────────────────────────────────────────────────────────────────
    private readonly Mock<IOrderRepository>           _orderRepoMock     = new();
    private readonly Mock<IOrderLineRepository>       _orderLineRepoMock = new();
    private readonly Mock<ICabinetRepository>         _cabinetRepoMock   = new();
    private readonly Mock<ILockerRepository>          _lockerRepoMock    = new();
    private readonly Mock<IBillRepository>            _billRepoMock      = new();
    private readonly Mock<ICustomerRepository>        _customerRepoMock  = new();
    private readonly Mock<IPartRepository>            _partRepoMock      = new();
    private readonly Mock<IAngleIronCalculatorService> _angleCalcMock    = new();
    private readonly Mock<ISupplierSelectionService>  _supplierSvcMock   = new();
    private readonly Mock<ISupplierOrderRepository>   _supplierOrderMock = new();

    private readonly OrderService _sut;

    // ── Test data ────────────────────────────────────────────────────────────────
    private readonly Locker _simpleLocker = new()
    {
        Height = 32, Width = 42, Depth = 32,
        Color  = "White", HasDoors = false
    };

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _orderRepoMock.Object,
            _orderLineRepoMock.Object,
            _cabinetRepoMock.Object,
            _lockerRepoMock.Object,
            _billRepoMock.Object,
            _customerRepoMock.Object,
            _partRepoMock.Object,
            _angleCalcMock.Object,
            _supplierSvcMock.Object,
            _supplierOrderMock.Object);

        // Default angle-iron length for any locker list
        _angleCalcMock.Setup(c => c.CalculateAngleIronLength(It.IsAny<List<Locker>>()))
                      .Returns(36.0);
    }

    // ── PreviewOrder ─────────────────────────────────────────────────────────────

    [Fact]
    public void PreviewOrder_WhenAllPartsAvailable_ShouldReturn_AllPartsAvailableTrue()
    {
        // Arrange – set up repository to return in-stock parts for all types
        SetupAllPartsInStock(stock: 50);
        var lockers = new List<Locker> { _simpleLocker };

        // Act
        var preview = _sut.PreviewOrder(lockers, "White");

        // Assert
        preview.AllPartsAvailable.Should().BeTrue();
    }

    [Fact]
    public void PreviewOrder_WhenAPartIsOutOfStock_ShouldReturn_AllPartsAvailableFalse()
    {
        // Arrange – parts in stock but one type returns null (not found in catalog)
        SetupAllPartsInStock(stock: 50);
        // Override crossbar "Back" to not found
        _partRepoMock.Setup(r => r.GetByType("Crossbar")).Returns(new List<Part>());

        var lockers = new List<Locker> { _simpleLocker };

        // Act
        var preview = _sut.PreviewOrder(lockers, "White");

        // Assert
        preview.AllPartsAvailable.Should().BeFalse();
    }

    [Fact]
    public void PreviewOrder_ShouldReturn_CorrectAngleIronLength()
    {
        // Arrange
        _angleCalcMock.Setup(c => c.CalculateAngleIronLength(It.IsAny<List<Locker>>()))
                      .Returns(138.0);
        SetupAllPartsInStock(stock: 50);

        // Act
        var preview = _sut.PreviewOrder(new List<Locker> { _simpleLocker }, "Black");

        // Assert
        preview.AngleIronLength.Should().Be(138.0);
        preview.AngleIronColor.Should().Be("Black");
    }

    [Fact]
    public void PreviewOrder_ShouldReturn_PositiveTotalPrice_WhenPartsFound()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50, unitPrice: 5.00m);

        // Act
        var preview = _sut.PreviewOrder(new List<Locker> { _simpleLocker }, "White");

        // Assert
        preview.TotalPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PreviewOrder_WithLockerHavingGlassDoors_ShouldNotInclude_CupHandles()
    {
        // Arrange
        var lockerWithGlass = new Locker
        {
            Height = 32, Width = 42, Depth = 32,
            Color  = "White", HasDoors = true, DoorColor = "Glass"
        };
        SetupAllPartsInStock(stock: 50);

        // Act
        var preview = _sut.PreviewOrder(new List<Locker> { lockerWithGlass }, "White");

        // Assert – glass doors must not generate Handle requirements
        preview.Parts.Should().NotContain(p => p.PartName.Contains("Handle") && p.Required > 0,
            because: "glass doors have no cup handles");
    }

    [Fact]
    public void PreviewOrder_WithLockerHavingNonGlassDoors_ShouldInclude_CupHandles()
    {
        // Arrange
        var lockerWithDoors = new Locker
        {
            Height = 32, Width = 42, Depth = 32,
            Color  = "White", HasDoors = true, DoorColor = "White"
        };
        SetupAllPartsInStock(stock: 50);

        // Act
        var preview = _sut.PreviewOrder(new List<Locker> { lockerWithDoors }, "White");

        // Assert – non-glass doors must include 2 handles
        // (if handle part not found, part name contains "Handle" and qty > 0 in requirements)
        preview.Parts.Should().Contain(p => p.Required == 2,
            because: "2 handles required for non-glass doors");
    }

    // ── PlaceOrder ───────────────────────────────────────────────────────────────

    [Fact]
    public void PlaceOrder_WhenAllPartsAvailable_ShouldSet_StatusAvailable()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50);
        var customer = new Customer { Id = 1 };
        var lockers  = new List<Locker> { _simpleLocker };

        // Act
        var order = _sut.PlaceOrder(customer, lockers, "White", depositAmount: 0);

        // Assert
        order.Status.Should().Be(OrderStatus.Available);
    }

    [Fact]
    public void PlaceOrder_WhenAllPartsAvailable_ShouldNot_SetDeposit()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50);
        var customer = new Customer { Id = 1 };
        var lockers  = new List<Locker> { _simpleLocker };

        // Act
        var order = _sut.PlaceOrder(customer, lockers, "White", depositAmount: 50);

        // Assert – no deposit when everything is available
        order.Deposit.Should().BeNull();
        order.AvailableDate.Should().BeNull();
    }

    [Fact]
    public void PlaceOrder_WhenPartsPartiallyAvailable_ShouldSet_StatusPartiallyAvailable()
    {
        // Arrange – crossbars unavailable
        SetupAllPartsInStock(stock: 50);
        _partRepoMock.Setup(r => r.GetByType("Crossbar")).Returns(new List<Part>());
        _supplierSvcMock.Setup(s => s.GetBestSupplier(It.IsAny<int>()))
                        .Returns((SupplierPart?)null);

        var customer = new Customer { Id = 1 };
        var lockers  = new List<Locker> { _simpleLocker };

        // Act
        var order = _sut.PlaceOrder(customer, lockers, "White", depositAmount: 25m);

        // Assert
        order.Status.Should().Be(OrderStatus.PartiallyAvailable);
    }

    [Fact]
    public void PlaceOrder_WhenPartsPartiallyAvailable_ShouldSet_Deposit()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50);
        _partRepoMock.Setup(r => r.GetByType("Crossbar")).Returns(new List<Part>());
        _supplierSvcMock.Setup(s => s.GetBestSupplier(It.IsAny<int>()))
                        .Returns((SupplierPart?)null);

        var customer = new Customer { Id = 1 };
        var lockers  = new List<Locker> { _simpleLocker };

        // Act
        var order = _sut.PlaceOrder(customer, lockers, "White", depositAmount: 30m);

        // Assert
        order.Deposit.Should().Be(30m);
    }

    [Fact]
    public void PlaceOrder_ShouldPersist_Order_ViaRepository()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50);
        var customer = new Customer { Id = 1 };

        // Act
        _sut.PlaceOrder(customer, new List<Locker> { _simpleLocker }, "White", 0);

        // Assert
        _orderRepoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public void PlaceOrder_ShouldPersist_Cabinet_ViaRepository()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50);
        var customer = new Customer { Id = 1 };

        // Act
        _sut.PlaceOrder(customer, new List<Locker> { _simpleLocker }, "White", 0);

        // Assert
        _cabinetRepoMock.Verify(r => r.Add(It.IsAny<Cabinet>()), Times.Once);
    }

    [Fact]
    public void PlaceOrder_ShouldPersist_AllLockers_ViaRepository()
    {
        // Arrange
        SetupAllPartsInStock(stock: 50);
        var customer = new Customer { Id = 1 };
        var lockers = new List<Locker> { _simpleLocker, _simpleLocker };

        // Act
        _sut.PlaceOrder(customer, lockers, "White", 0);

        // Assert
        _lockerRepoMock.Verify(r => r.Add(It.IsAny<Locker>()), Times.Exactly(2));
    }

    [Fact]
    public void PlaceOrder_ForNewCustomer_ShouldPersist_Customer()
    {
        // Arrange – Id = 0 means new customer
        SetupAllPartsInStock(stock: 50);
        var newCustomer = new Customer { Id = 0, FirstName = "John", LastName = "Doe" };

        // Act
        _sut.PlaceOrder(newCustomer, new List<Locker> { _simpleLocker }, "White", 0);

        // Assert
        _customerRepoMock.Verify(r => r.Add(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public void PlaceOrder_ForExistingCustomer_ShouldNotPersist_CustomerAgain()
    {
        // Arrange – Id > 0 means existing customer
        SetupAllPartsInStock(stock: 50);
        var existingCustomer = new Customer { Id = 5, FirstName = "Jane" };

        // Act
        _sut.PlaceOrder(existingCustomer, new List<Locker> { _simpleLocker }, "White", 0);

        // Assert
        _customerRepoMock.Verify(r => r.Add(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public void PlaceOrder_WhenStockInsufficient_ShouldCreate_SupplierOrder()
    {
        // Arrange – stock too low, supplier available
        SetupAllPartsInStock(stock: 0);
        var supplier = new SupplierPart { SupplierId = 1, PartId = 1,
                                          Price = 5m, DeliveryDays = 7 };
        _supplierSvcMock.Setup(s => s.GetBestSupplier(It.IsAny<int>())).Returns(supplier);

        var customer = new Customer { Id = 1 };

        // Act
        _sut.PlaceOrder(customer, new List<Locker> { _simpleLocker }, "White", 10m);

        // Assert
        _supplierOrderMock.Verify(r => r.Add(It.IsAny<SupplierOrder>()), Times.AtLeastOnce);
    }

    // ── Private helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Sets up the part repository so that every GetByType call returns a single
    /// matching in-stock part for any requested type/dimensions.
    /// </summary>
    private void SetupAllPartsInStock(int stock, decimal unitPrice = 10m)
    {
        _partRepoMock.Setup(r => r.GetByType(It.IsAny<string>()))
                     .Returns((string type) =>
                     {
                         return type switch
                         {
                             "Batten"    => new List<Part> { MakeBatten(stock, unitPrice) },
                             "Crossbar"  => new List<Part>
                             {
                                 MakeCrossbar("Front", stock, unitPrice),
                                 MakeCrossbar("Back",  stock, unitPrice),
                                 MakeCrossbar("Side",  stock, unitPrice)
                             },
                             "Panel"     => new List<Part>
                             {
                                 MakePanel("Horizontal", stock, unitPrice),
                                 MakePanel("Side",       stock, unitPrice),
                                 MakePanel("Back",       stock, unitPrice)
                             },
                             "Door"      => new List<Part> { MakeDoor(isGlass: false, stock, unitPrice),
                                                             MakeDoor(isGlass: true,  stock, unitPrice) },
                             "Handle"    => new List<Part> { MakeHandle(stock, unitPrice) },
                             "AngleIron" => new List<Part> { MakeAngleIron(stock, unitPrice) },
                             _           => new List<Part>()
                         };
                     });
    }

    private static Batten MakeBatten(int stock, decimal price) =>
        new() { Id = 1, Name = "Batten", Reference = "B32",
                Height = 32, StockQuantity = stock, UnitPrice = price };

    private static Crossbar MakeCrossbar(string type, int stock, decimal price) =>
        new() { Id = 2, Name = $"Crossbar-{type}", Reference = $"CB-{type}",
                Width = 42, Depth = 32,
                Type = Enum.Parse<KitBox.Models.Enums.CrossbarType>(type),
                StockQuantity = stock, UnitPrice = price };

    private static Panel MakePanel(string type, int stock, decimal price) =>
        new() { Id = 3, Name = $"Panel-{type}", Reference = $"P-{type}",
                Width = 42, Depth = 32, Height = 32, Color = "White",
                Type = Enum.Parse<KitBox.Models.Enums.PanelType>(type),
                StockQuantity = stock, UnitPrice = price };

    private static Door MakeDoor(bool isGlass, int stock, decimal price) =>
        new() { Id = 4, Name = isGlass ? "Glass Door" : "Door", Reference = "D42",
                Height = 32, Width = 42, Color = isGlass ? "Glass" : "White",
                IsGlass = isGlass, StockQuantity = stock, UnitPrice = price };

    private static Handle MakeHandle(int stock, decimal price) =>
        new() { Id = 5, Name = "Handle", Reference = "H01",
                StockQuantity = stock, UnitPrice = price };

    private static AngleIron MakeAngleIron(int stock, decimal price) =>
        new() { Id = 6, Name = "AngleIron", Reference = "AI36",
                Height = 36, Color = "White",
                StockQuantity = stock, UnitPrice = price };
}
