using FluentAssertions;
using KitBox.Models;
using Xunit;

namespace KitBox.Tests.Models;

/// <summary>
/// Unit tests for the Locker model.
/// Verifies TotalHeight computation and default property values.
/// </summary>
public class LockerTests
{
    // ── TotalHeight ──────────────────────────────────────────────────────────────

    [Fact]
    public void TotalHeight_WithHeight32_ShouldBe_36()
    {
        // Arrange
        var locker = new Locker { Height = 32.0 };

        // Act & Assert
        // TotalHeight = 32 + 2*2 = 36
        locker.TotalHeight.Should().Be(36.0);
    }

    [Theory]
    [InlineData(32.0, 36.0)]
    [InlineData(42.0, 46.0)]
    [InlineData(52.0, 56.0)]
    public void TotalHeight_ForAllCatalogHeights_ShouldAddFourCm(double height, double expected)
    {
        // Arrange
        var locker = new Locker { Height = height };

        // Act & Assert
        locker.TotalHeight.Should().Be(expected,
            because: $"TotalHeight = battens({height}) + 2×crossbar(2 cm)");
    }

    [Fact]
    public void TotalHeight_ShouldAlways_BeGreaterThan_Height()
    {
        // Arrange
        var locker = new Locker { Height = 42.0 };

        // Act & Assert
        locker.TotalHeight.Should().BeGreaterThan(locker.Height);
    }

    [Fact]
    public void TotalHeight_ShouldBe_HeightPlusFour()
    {
        // Arrange
        double height = 52.0;
        var locker = new Locker { Height = height };

        // Act & Assert – 2 crossbars × 2 cm = 4 cm extra
        locker.TotalHeight.Should().Be(height + 4.0);
    }

    // ── Defaults ─────────────────────────────────────────────────────────────────

    [Fact]
    public void NewLocker_HasDoors_ShouldDefault_ToFalse()
    {
        // Arrange & Act
        var locker = new Locker();

        // Assert
        locker.HasDoors.Should().BeFalse();
    }

    [Fact]
    public void NewLocker_DoorColor_ShouldDefault_ToNull()
    {
        // Arrange & Act
        var locker = new Locker();

        // Assert
        locker.DoorColor.Should().BeNull();
    }

    [Fact]
    public void NewLocker_Color_ShouldDefault_ToEmptyString()
    {
        // Arrange & Act
        var locker = new Locker();

        // Assert
        locker.Color.Should().BeEmpty();
    }

    // ── Properties ───────────────────────────────────────────────────────────────

    [Fact]
    public void Locker_WithAllProperties_ShouldStore_CorrectValues()
    {
        // Arrange & Act
        var locker = new Locker
        {
            Id       = 1,
            CabinetId = 2,
            Height   = 32.0,
            Width    = 62.0,
            Depth    = 42.0,
            Color    = "White",
            HasDoors = true,
            DoorColor = "Marron"
        };

        // Assert
        locker.Id.Should().Be(1);
        locker.CabinetId.Should().Be(2);
        locker.Height.Should().Be(32.0);
        locker.Width.Should().Be(62.0);
        locker.Depth.Should().Be(42.0);
        locker.Color.Should().Be("White");
        locker.HasDoors.Should().BeTrue();
        locker.DoorColor.Should().Be("Marron");
    }

    [Fact]
    public void Locker_WithGlassDoors_ShouldStore_GlassDoorColor()
    {
        // Arrange & Act
        var locker = new Locker
        {
            HasDoors  = true,
            DoorColor = "Glass"
        };

        // Assert
        locker.DoorColor.Should().Be("Glass");
        locker.HasDoors.Should().BeTrue();
    }
}
