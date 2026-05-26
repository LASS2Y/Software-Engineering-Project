using FluentAssertions;
using KitBox.Models;
using KitBox.Services;
using KitBox.Services.Interfaces;
using Moq;
using Xunit;

namespace KitBox.Tests.Services;

/// <summary>
/// Unit tests for AngleIronCalculatorService.
/// The service computes total locker height and total angle-iron length.
/// Formula: TotalHeight  = batten height + 2 × crossbar height (2 cm each)
///          AngleIronLen = sum of all locker TotalHeights
/// </summary>
public class AngleIronCalculatorServiceTests
{
    private readonly Mock<ICatalogService> _catalogMock = new();
    private readonly AngleIronCalculatorService _sut;

    public AngleIronCalculatorServiceTests()
    {
        // Crossbar height is always 2 cm in the real catalog
        _catalogMock.Setup(c => c.CrossbarHeight).Returns(2.0);
        _sut = new AngleIronCalculatorService(_catalogMock.Object);
    }

    // ── CalculateLockerTotalHeight ───────────────────────────────────────────────

    [Fact]
    public void CalculateLockerTotalHeight_ShouldReturn_BattenHeightPlusFourCm()
    {
        // Arrange
        var locker = new Locker { Height = 32.0 };

        // Act
        double totalHeight = _sut.CalculateLockerTotalHeight(locker);

        // Assert  (32 + 2*2 = 36)
        totalHeight.Should().Be(36.0);
    }

    [Theory]
    [InlineData(32.0, 36.0)]
    [InlineData(42.0, 46.0)]
    [InlineData(52.0, 56.0)]
    public void CalculateLockerTotalHeight_ForAllCatalogHeights_ShouldAddFourCm(
        double battenHeight, double expectedTotal)
    {
        // Arrange
        var locker = new Locker { Height = battenHeight };

        // Act
        double totalHeight = _sut.CalculateLockerTotalHeight(locker);

        // Assert
        totalHeight.Should().Be(expectedTotal);
    }

    [Fact]
    public void CalculateLockerTotalHeight_ShouldUse_CrossbarHeightFromCatalog()
    {
        // Arrange – catalog reports a different crossbar height (edge-case override)
        _catalogMock.Setup(c => c.CrossbarHeight).Returns(3.0);
        var sut = new AngleIronCalculatorService(_catalogMock.Object);
        var locker = new Locker { Height = 42.0 };

        // Act
        double totalHeight = sut.CalculateLockerTotalHeight(locker);

        // Assert  (42 + 2*3 = 48)
        totalHeight.Should().Be(48.0);
    }

    // ── CalculateAngleIronLength ─────────────────────────────────────────────────

    [Fact]
    public void CalculateAngleIronLength_WithSingleLocker_ShouldEqualLockerTotalHeight()
    {
        // Arrange
        var lockers = new List<Locker> { new() { Height = 32.0 } };

        // Act
        double length = _sut.CalculateAngleIronLength(lockers);

        // Assert
        length.Should().Be(36.0);
    }

    [Fact]
    public void CalculateAngleIronLength_WithMultipleLockers_ShouldSumAllTotalHeights()
    {
        // Arrange – 3 lockers of heights 32, 42, 52 → totals 36 + 46 + 56 = 138
        var lockers = new List<Locker>
        {
            new() { Height = 32.0 },
            new() { Height = 42.0 },
            new() { Height = 52.0 }
        };

        // Act
        double length = _sut.CalculateAngleIronLength(lockers);

        // Assert
        length.Should().Be(138.0);
    }

    [Fact]
    public void CalculateAngleIronLength_WithSevenLockers_ShouldReturnCorrectSum()
    {
        // Arrange – 7 lockers all height=32 → each total=36, sum=252
        var lockers = Enumerable.Range(0, 7)
            .Select(_ => new Locker { Height = 32.0 })
            .ToList();

        // Act
        double length = _sut.CalculateAngleIronLength(lockers);

        // Assert
        length.Should().Be(252.0);
    }

    [Fact]
    public void CalculateAngleIronLength_WithEmptyList_ShouldReturnZero()
    {
        // Arrange
        var lockers = new List<Locker>();

        // Act
        double length = _sut.CalculateAngleIronLength(lockers);

        // Assert
        length.Should().Be(0.0);
    }

    [Fact]
    public void CalculateAngleIronLength_WithTwoSameHeightLockers_ShouldBeDoubleOfSingle()
    {
        // Arrange
        var single = new List<Locker> { new() { Height = 42.0 } };
        var doubled = new List<Locker> { new() { Height = 42.0 }, new() { Height = 42.0 } };

        // Act
        double singleLength = _sut.CalculateAngleIronLength(single);
        double doubleLength = _sut.CalculateAngleIronLength(doubled);

        // Assert
        doubleLength.Should().Be(singleLength * 2);
    }
}
