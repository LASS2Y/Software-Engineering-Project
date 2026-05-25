using FluentAssertions;
using KitBox.DataAccess.Interfaces;
using KitBox.Models;
using KitBox.Services;
using Moq;
using Xunit;

namespace KitBox.Tests.Services;

/// <summary>
/// Unit tests for SupplierSelectionService.
/// Business rule: choose supplier by lowest price; on tie, shortest delivery time.
/// The service delegates selection entirely to ISupplierPartRepository.GetBestSupplierForPart.
/// </summary>
public class SupplierSelectionServiceTests
{
    private readonly Mock<ISupplierPartRepository> _repoMock = new();
    private readonly SupplierSelectionService _sut;

    public SupplierSelectionServiceTests()
    {
        _sut = new SupplierSelectionService(_repoMock.Object);
    }

    [Fact]
    public void GetBestSupplier_WhenRepositoryReturnsSupplier_ShouldReturnSameSupplier()
    {
        // Arrange
        var expected = new SupplierPart { Id = 1, SupplierId = 10, PartId = 5,
                                          Price = 9.99m, DeliveryDays = 3 };
        _repoMock.Setup(r => r.GetBestSupplierForPart(5)).Returns(expected);

        // Act
        var result = _sut.GetBestSupplier(5);

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public void GetBestSupplier_WhenNoSupplierAvailable_ShouldReturnNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetBestSupplierForPart(It.IsAny<int>()))
                 .Returns((SupplierPart?)null);

        // Act
        var result = _sut.GetBestSupplier(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetBestSupplier_ShouldDelegateToRepository_WithCorrectPartId()
    {
        // Arrange
        const int partId = 42;
        _repoMock.Setup(r => r.GetBestSupplierForPart(partId))
                 .Returns(new SupplierPart { SupplierId = 1, PartId = partId,
                                             Price = 5m, DeliveryDays = 7 });

        // Act
        _sut.GetBestSupplier(partId);

        // Assert – repository was called exactly once with the right id
        _repoMock.Verify(r => r.GetBestSupplierForPart(partId), Times.Once);
    }

    [Fact]
    public void GetBestSupplier_ShouldNeverCallRepository_WithDifferentPartId()
    {
        // Arrange
        const int requestedPartId = 10;
        _repoMock.Setup(r => r.GetBestSupplierForPart(requestedPartId))
                 .Returns(new SupplierPart { SupplierId = 2, PartId = requestedPartId,
                                             Price = 3m, DeliveryDays = 5 });

        // Act
        _sut.GetBestSupplier(requestedPartId);

        // Assert – only the exact part id was queried
        _repoMock.Verify(r => r.GetBestSupplierForPart(It.IsNotIn(requestedPartId)), Times.Never);
    }

    [Fact]
    public void GetBestSupplier_ShouldReturn_SupplierWithCorrectPrice()
    {
        // Arrange
        var supplier = new SupplierPart { SupplierId = 3, PartId = 7,
                                          Price = 12.50m, DeliveryDays = 2 };
        _repoMock.Setup(r => r.GetBestSupplierForPart(7)).Returns(supplier);

        // Act
        var result = _sut.GetBestSupplier(7);

        // Assert
        result!.Price.Should().Be(12.50m);
        result.DeliveryDays.Should().Be(2);
    }
}
