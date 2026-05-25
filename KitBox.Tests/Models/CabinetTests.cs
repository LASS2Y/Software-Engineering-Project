using FluentAssertions;
using KitBox.Models;
using Xunit;

namespace KitBox.Tests.Models;

/// <summary>
/// Unit tests for the Cabinet model.
/// Verifies AngleIronLength computation, MaxLockers constant, and defaults.
/// </summary>
public class CabinetTests
{
    // ── MaxLockers constant ──────────────────────────────────────────────────────

    [Fact]
    public void MaxLockers_ShouldBe_Seven()
    {
        // Arrange & Act & Assert
        Cabinet.MaxLockers.Should().Be(7);
    }

    // ── AngleIronLength ──────────────────────────────────────────────────────────

    [Fact]
    public void AngleIronLength_WithNoLockers_ShouldBe_Zero()
    {
        // Arrange
        var cabinet = new Cabinet();

        // Act & Assert
        cabinet.AngleIronLength.Should().Be(0.0);
    }

    [Fact]
    public void AngleIronLength_WithSingleLocker_ShouldEqual_LockerTotalHeight()
    {
        // Arrange
        var locker = new Locker { Height = 32.0 };  // TotalHeight = 36
        var cabinet = new Cabinet { Lockers = new List<Locker> { locker } };

        // Act & Assert
        cabinet.AngleIronLength.Should().Be(locker.TotalHeight);
    }

    [Fact]
    public void AngleIronLength_WithTwoLockers_ShouldBeSumOfTotalHeights()
    {
        // Arrange
        var locker1 = new Locker { Height = 32.0 }; // TotalHeight = 36
        var locker2 = new Locker { Height = 42.0 }; // TotalHeight = 46
        var cabinet = new Cabinet { Lockers = new List<Locker> { locker1, locker2 } };

        // Act & Assert
        cabinet.AngleIronLength.Should().Be(82.0); // 36 + 46
    }

    [Fact]
    public void AngleIronLength_WithSevenLockers_ShouldSumAllTotalHeights()
    {
        // Arrange – 7 lockers, alternating heights 32 and 42
        var lockers = new List<Locker>
        {
            new() { Height = 32 }, // 36
            new() { Height = 42 }, // 46
            new() { Height = 52 }, // 56
            new() { Height = 32 }, // 36
            new() { Height = 42 }, // 46
            new() { Height = 52 }, // 56
            new() { Height = 32 }, // 36
        };
        var cabinet = new Cabinet { Lockers = lockers };

        // Act & Assert  (36+46+56+36+46+56+36 = 312)
        cabinet.AngleIronLength.Should().Be(312.0);
    }

    [Fact]
    public void AngleIronLength_ShouldUpdate_WhenLockerIsAdded()
    {
        // Arrange
        var cabinet = new Cabinet();
        double before = cabinet.AngleIronLength;

        cabinet.Lockers.Add(new Locker { Height = 32.0 });

        // Act & Assert
        cabinet.AngleIronLength.Should().BeGreaterThan(before);
    }

    // ── Defaults ─────────────────────────────────────────────────────────────────

    [Fact]
    public void NewCabinet_Lockers_ShouldDefault_ToEmptyList()
    {
        // Arrange & Act
        var cabinet = new Cabinet();

        // Assert
        cabinet.Lockers.Should().NotBeNull();
        cabinet.Lockers.Should().BeEmpty();
    }

    [Fact]
    public void NewCabinet_AngleIronColor_ShouldDefault_ToEmptyString()
    {
        // Arrange & Act
        var cabinet = new Cabinet();

        // Assert
        cabinet.AngleIronColor.Should().BeEmpty();
    }

    // ── Properties ───────────────────────────────────────────────────────────────

    [Fact]
    public void Cabinet_WithAllProperties_ShouldStore_CorrectValues()
    {
        // Arrange & Act
        var cabinet = new Cabinet
        {
            Id             = 10,
            OrderId        = 5,
            AngleIronColor = "Black"
        };

        // Assert
        cabinet.Id.Should().Be(10);
        cabinet.OrderId.Should().Be(5);
        cabinet.AngleIronColor.Should().Be("Black");
    }

    [Fact]
    public void Cabinet_CanHoldUpToMaxLockers()
    {
        // Arrange
        var cabinet = new Cabinet();
        for (int i = 0; i < Cabinet.MaxLockers; i++)
            cabinet.Lockers.Add(new Locker { Height = 32 });

        // Act & Assert
        cabinet.Lockers.Should().HaveCount(Cabinet.MaxLockers);
    }
}
