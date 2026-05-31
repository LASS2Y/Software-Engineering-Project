using FluentAssertions;
using KitBox.Models;
using KitBox.Models.Enums;
using Xunit;

namespace KitBox.Tests.Models;

/// <summary>
/// Unit tests for the Order model.
/// Verifies default values, status semantics, navigation properties, and property assignment.
/// </summary>
public class OrderTests
{
    // ── Defaults ─────────────────────────────────────────────────────────────────

    [Fact]
    public void NewOrder_Lines_ShouldDefault_ToEmptyList()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Lines.Should().NotBeNull();
        order.Lines.Should().BeEmpty();
    }

    [Fact]
    public void NewOrder_Cabinets_ShouldDefault_ToEmptyList()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Cabinets.Should().NotBeNull();
        order.Cabinets.Should().BeEmpty();
    }

    [Fact]
    public void NewOrder_Deposit_ShouldDefault_ToNull()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Deposit.Should().BeNull();
    }

    [Fact]
    public void NewOrder_AvailableDate_ShouldDefault_ToNull()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.AvailableDate.Should().BeNull();
    }

    [Fact]
    public void NewOrder_BillId_ShouldDefault_ToNull()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.BillId.Should().BeNull();
    }

    [Fact]
    public void NewOrder_Customer_ShouldDefault_ToNull()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Customer.Should().BeNull();
    }

    [Fact]
    public void NewOrder_Bill_ShouldDefault_ToNull()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Bill.Should().BeNull();
    }

    // ── Status transitions ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.PartiallyAvailable)]
    [InlineData(OrderStatus.Available)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Order_CanBeSet_ToAnyValidStatus(OrderStatus status)
    {
        // Arrange
        var order = new Order();

        // Act
        order.Status = status;

        // Assert
        order.Status.Should().Be(status);
    }

    [Fact]
    public void Order_WithStatusAvailable_ShouldHave_NullDeposit()
    {
        // Arrange & Act
        var order = new Order
        {
            Status  = OrderStatus.Available,
            Deposit = null
        };

        // Assert – business rule: no deposit required when all parts are available
        order.Deposit.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Available);
    }

    [Fact]
    public void Order_WithStatusPartiallyAvailable_CanHave_Deposit()
    {
        // Arrange & Act
        var order = new Order
        {
            Status  = OrderStatus.PartiallyAvailable,
            Deposit = 50m
        };

        // Assert
        order.Deposit.Should().Be(50m);
        order.Status.Should().Be(OrderStatus.PartiallyAvailable);
    }

    [Fact]
    public void Order_WithStatusPartiallyAvailable_ShouldHave_AvailableDate()
    {
        // Arrange
        var expectedDate = DateTime.Today.AddDays(14);
        var order = new Order
        {
            Status        = OrderStatus.PartiallyAvailable,
            AvailableDate = expectedDate
        };

        // Assert
        order.AvailableDate.Should().Be(expectedDate);
    }

    // ── Navigation properties ─────────────────────────────────────────────────────

    [Fact]
    public void Order_CanHave_Customer()
    {
        // Arrange
        var customer = new Customer { Id = 1, FirstName = "Alice" };
        var order    = new Order { Customer = customer };

        // Assert
        order.Customer.Should().BeSameAs(customer);
    }

    [Fact]
    public void Order_CanHave_Cabinets()
    {
        // Arrange
        var order = new Order();
        order.Cabinets.Add(new Cabinet { Id = 1 });
        order.Cabinets.Add(new Cabinet { Id = 2 });

        // Assert
        order.Cabinets.Should().HaveCount(2);
    }

    [Fact]
    public void Order_CanHave_OrderLines()
    {
        // Arrange
        var order = new Order();
        order.Lines.Add(new OrderLine { PartId = 1, Quantity = 4, UnitPrice = 5m });
        order.Lines.Add(new OrderLine { PartId = 2, Quantity = 2, UnitPrice = 10m });

        // Assert
        order.Lines.Should().HaveCount(2);
    }

    // ── Properties ────────────────────────────────────────────────────────────────

    [Fact]
    public void Order_WithAllProperties_ShouldStore_CorrectValues()
    {
        // Arrange
        var date      = new DateTime(2026, 1, 15);
        var available = new DateTime(2026, 1, 29);

        // Act
        var order = new Order
        {
            Id          = 7,
            CustomerId  = 3,
            BillId      = 2,
            OrderDate   = date,
            Deposit     = 25m,
            AvailableDate = available,
            Status      = OrderStatus.PartiallyAvailable
        };

        // Assert
        order.Id.Should().Be(7);
        order.CustomerId.Should().Be(3);
        order.BillId.Should().Be(2);
        order.OrderDate.Should().Be(date);
        order.Deposit.Should().Be(25m);
        order.AvailableDate.Should().Be(available);
        order.Status.Should().Be(OrderStatus.PartiallyAvailable);
    }
}
