using FluentAssertions;
using KitBox.Models;
using KitBox.Services;
using KitBox.Services.Interfaces;
using Moq;
using Xunit;

namespace KitBox.Tests.Services;

/// <summary>
/// Unit tests for LockerValidationService.
/// Covers dimension validation, locker-count rules, and full cabinet validation.
/// </summary>
public class LockerValidationServiceTests
{
    private readonly Mock<ICatalogService> _catalogMock = new();
    private readonly LockerValidationService _sut;

    public LockerValidationServiceTests()
    {
        // Real catalog values from CatalogService
        _catalogMock.Setup(c => c.GetAvailableHeights())
            .Returns(new List<double> { 32, 42, 52 }.AsReadOnly());
        _catalogMock.Setup(c => c.GetAvailableWidths())
            .Returns(new List<double> { 32, 42, 52, 62, 80, 100, 120 }.AsReadOnly());
        _catalogMock.Setup(c => c.GetAvailableWidthsWithDoors())
            .Returns(new List<double> { 32, 42, 52, 62 }.AsReadOnly());
        _catalogMock.Setup(c => c.GetAvailableDepths())
            .Returns(new List<double> { 32, 42, 52, 62 }.AsReadOnly());

        _sut = new LockerValidationService(_catalogMock.Object);
    }

    // ── AreDimensionsValid ──────────────────────────────────────────────────────

    [Fact]
    public void AreDimensionsValid_WithValidDimensions_NoDoors_ShouldReturnTrue()
    {
        // Arrange & Act
        bool result = _sut.AreDimensionsValid(32, 80, 42, hasDoors: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreDimensionsValid_WithValidDimensions_WithDoors_ShouldReturnTrue()
    {
        // Arrange & Act
        bool result = _sut.AreDimensionsValid(42, 62, 32, hasDoors: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreDimensionsValid_WithInvalidHeight_ShouldReturnFalse()
    {
        // Arrange & Act
        bool result = _sut.AreDimensionsValid(99, 32, 32, hasDoors: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreDimensionsValid_WithInvalidDepth_ShouldReturnFalse()
    {
        // Arrange & Act
        bool result = _sut.AreDimensionsValid(32, 32, 99, hasDoors: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreDimensionsValid_WithInvalidWidth_NoDoors_ShouldReturnFalse()
    {
        // Arrange & Act
        bool result = _sut.AreDimensionsValid(32, 999, 32, hasDoors: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreDimensionsValid_WithWidthOver62_WithDoors_ShouldReturnFalse()
    {
        // Arrange – 80 cm is a valid width without doors, but not with doors
        bool result = _sut.AreDimensionsValid(32, 80, 32, hasDoors: true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreDimensionsValid_WithWidthOver62_WithoutDoors_ShouldReturnTrue()
    {
        // Arrange – 80 cm is valid when no doors
        bool result = _sut.AreDimensionsValid(32, 80, 32, hasDoors: false);

        // Assert
        result.Should().BeTrue();
    }

    // ── IsLockerCountValid ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public void IsLockerCountValid_WithCountBetween1And7_ShouldReturnTrue(int count)
    {
        // Arrange & Act
        bool result = _sut.IsLockerCountValid(count);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(100)]
    public void IsLockerCountValid_WithInvalidCount_ShouldReturnFalse(int count)
    {
        // Arrange & Act
        bool result = _sut.IsLockerCountValid(count);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLockerCountValid_AtMaxLockers_ShouldReturnTrue()
    {
        // Arrange & Act
        bool result = _sut.IsLockerCountValid(Cabinet.MaxLockers);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLockerCountValid_AboveMaxLockers_ShouldReturnFalse()
    {
        // Arrange & Act
        bool result = _sut.IsLockerCountValid(Cabinet.MaxLockers + 1);

        // Assert
        result.Should().BeFalse();
    }

    // ── ValidateCabinet – happy paths ───────────────────────────────────────────

    [Fact]
    public void ValidateCabinet_WithSingleValidLocker_ShouldReturnNoErrors()
    {
        // Arrange
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 62, Depth = 32, Color = "White", HasDoors = false }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCabinet_WithMultipleValidLockersSameWidth_ShouldReturnNoErrors()
    {
        // Arrange
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 42, Depth = 32, Color = "White",  HasDoors = false },
            new() { Height = 42, Width = 42, Depth = 42, Color = "Marron", HasDoors = false }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCabinet_LockerWithDoorsAndDoorColor_ShouldReturnNoErrors()
    {
        // Arrange
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 42, Depth = 32,
                    Color = "White", HasDoors = true, DoorColor = "White" }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().BeEmpty();
    }

    // ── ValidateCabinet – error cases ───────────────────────────────────────────

    [Fact]
    public void ValidateCabinet_WithEmptyLockerList_ShouldReturnError()
    {
        // Arrange
        var lockers = new List<Locker>();

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().ContainSingle();
    }

    [Fact]
    public void ValidateCabinet_WithTooManyLockers_ShouldReturnError()
    {
        // Arrange – 8 lockers exceeds Cabinet.MaxLockers (7)
        var lockers = Enumerable.Range(0, 8)
            .Select(_ => new Locker { Height = 32, Width = 32, Depth = 32, Color = "White" })
            .ToList();

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().Contain(e => e.Contains("cabinet must have between"));
    }

    [Fact]
    public void ValidateCabinet_WithMismatchedWidths_ShouldReturnError()
    {
        // Arrange – two lockers with different widths in the same cabinet
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 42, Depth = 32, Color = "White" },
            new() { Height = 32, Width = 62, Depth = 32, Color = "White" }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().Contain(e => e.Contains("same width"));
    }

    [Fact]
    public void ValidateCabinet_WithInvalidDimensions_ShouldReturnError()
    {
        // Arrange – height 99 is not in the catalog
        var lockers = new List<Locker>
        {
            new() { Height = 99, Width = 32, Depth = 32, Color = "White", HasDoors = false }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().Contain(e => e.Contains("not available in the catalog"));
    }

    [Fact]
    public void ValidateCabinet_LockerWithDoorsButNoDoorColor_ShouldReturnError()
    {
        // Arrange
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 42, Depth = 32,
                    Color = "White", HasDoors = true, DoorColor = null }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().Contain(e => e.Contains("door color"));
    }

    [Fact]
    public void ValidateCabinet_LockerWithDoorsButEmptyDoorColor_ShouldReturnError()
    {
        // Arrange
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 42, Depth = 32,
                    Color = "White", HasDoors = true, DoorColor = "   " }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert
        errors.Should().Contain(e => e.Contains("door color"));
    }

    [Fact]
    public void ValidateCabinet_ShouldReturn_MultipleErrors_WhenMultipleViolations()
    {
        // Arrange – width mismatch AND invalid door config
        var lockers = new List<Locker>
        {
            new() { Height = 32, Width = 42, Depth = 32, Color = "White",
                    HasDoors = true, DoorColor = null },
            new() { Height = 32, Width = 62, Depth = 32, Color = "White" }
        };

        // Act
        var errors = _sut.ValidateCabinet(lockers);

        // Assert – should have at least 2 errors
        errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
