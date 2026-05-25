using FluentAssertions;
using KitBox.Services;
using Xunit;

namespace KitBox.Tests.Services;

/// <summary>
/// Unit tests for CatalogService – the single source of truth for valid
/// dimensions, colors, and catalog constants.
/// </summary>
public class CatalogServiceTests
{
    private readonly CatalogService _sut = new();

    // ── Heights ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAvailableHeights_ShouldReturn_ExactlyThreeValues()
    {
        // Arrange & Act
        var heights = _sut.GetAvailableHeights();

        // Assert
        heights.Should().HaveCount(3);
    }

    [Fact]
    public void GetAvailableHeights_ShouldReturn_ExpectedValues()
    {
        // Arrange & Act
        var heights = _sut.GetAvailableHeights();

        // Assert
        heights.Should().BeEquivalentTo(new[] { 32.0, 42.0, 52.0 });
    }

    [Fact]
    public void GetAvailableHeights_ShouldReturn_ReadOnlyList()
    {
        // Arrange & Act
        var heights = _sut.GetAvailableHeights();

        // Assert
        heights.Should().NotBeNull();
        heights.Should().BeAssignableTo<IReadOnlyList<double>>();
    }

    // ── Widths ───────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAvailableWidths_ShouldReturn_SevenValues()
    {
        // Arrange & Act
        var widths = _sut.GetAvailableWidths();

        // Assert
        widths.Should().HaveCount(7);
    }

    [Fact]
    public void GetAvailableWidths_ShouldContain_AllExpectedWidths()
    {
        // Arrange & Act
        var widths = _sut.GetAvailableWidths();

        // Assert
        widths.Should().BeEquivalentTo(new[] { 32.0, 42.0, 52.0, 62.0, 80.0, 100.0, 120.0 });
    }

    [Fact]
    public void GetAvailableWidthsWithDoors_ShouldReturn_FourValues()
    {
        // Arrange & Act
        var widthsWithDoors = _sut.GetAvailableWidthsWithDoors();

        // Assert
        widthsWithDoors.Should().HaveCount(4);
    }

    [Fact]
    public void GetAvailableWidthsWithDoors_ShouldBe_SubsetOf_AllWidths()
    {
        // Arrange
        var allWidths = _sut.GetAvailableWidths();

        // Act
        var widthsWithDoors = _sut.GetAvailableWidthsWithDoors();

        // Assert
        widthsWithDoors.Should().OnlyContain(w => allWidths.Contains(w));
    }

    [Fact]
    public void GetAvailableWidthsWithDoors_ShouldNotContain_WidthsOver62()
    {
        // Arrange & Act
        var widthsWithDoors = _sut.GetAvailableWidthsWithDoors();

        // Assert – door panels are only available up to 62 cm wide
        widthsWithDoors.Should().OnlyContain(w => w <= 62.0);
    }

    // ── Depths ───────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAvailableDepths_ShouldReturn_FourValues()
    {
        // Arrange & Act
        var depths = _sut.GetAvailableDepths();

        // Assert
        depths.Should().HaveCount(4);
    }

    [Fact]
    public void GetAvailableDepths_ShouldContain_AllExpectedDepths()
    {
        // Arrange & Act
        var depths = _sut.GetAvailableDepths();

        // Assert
        depths.Should().BeEquivalentTo(new[] { 32.0, 42.0, 52.0, 62.0 });
    }

    // ── Colors ───────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAvailableColors_ShouldContain_WhiteAndMarron()
    {
        // Arrange & Act
        var colors = _sut.GetAvailableColors();

        // Assert
        colors.Should().Contain("White");
        colors.Should().Contain("Marron");
    }

    [Fact]
    public void GetAvailableColors_ShouldReturn_ExactlyTwoValues()
    {
        // Arrange & Act
        var colors = _sut.GetAvailableColors();

        // Assert
        colors.Should().HaveCount(2);
    }

    [Fact]
    public void GetAvailableAngleIronColors_ShouldContain_AllFourColors()
    {
        // Arrange & Act
        var colors = _sut.GetAvailableAngleIronColors();

        // Assert
        colors.Should().HaveCount(4);
        colors.Should().Contain(new[] { "White", "Marron", "Galva", "Black" });
    }

    [Fact]
    public void GetAvailableDoorColors_ShouldContain_Glass()
    {
        // Arrange & Act
        var colors = _sut.GetAvailableDoorColors();

        // Assert
        colors.Should().Contain("Glass");
    }

    [Fact]
    public void GetAvailableDoorColors_ShouldContain_WhiteAndMarron()
    {
        // Arrange & Act
        var colors = _sut.GetAvailableDoorColors();

        // Assert
        colors.Should().Contain("White");
        colors.Should().Contain("Marron");
    }

    // ── CrossbarHeight ──────────────────────────────────────────────────────────

    [Fact]
    public void CrossbarHeight_ShouldBe_2cm()
    {
        // Arrange & Act & Assert
        _sut.CrossbarHeight.Should().Be(2.0);
    }
}
