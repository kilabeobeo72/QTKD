using FluentAssertions;
using Moq;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;
using Xunit;

namespace QTKD.Tests.UnitTests;

public class PriceListServiceTests
{
    private readonly Mock<IPriceListRepository> _priceListRepoMock;
    private readonly PriceListService _priceListService;

    public PriceListServiceTests()
    {
        _priceListRepoMock = new Mock<IPriceListRepository>();
        _priceListService = new PriceListService(_priceListRepoMock.Object);
    }

    [Fact]
    public async Task AddPriceItem_WithValidUnitPrice_ShouldSucceed()
    {
        var priceList = new PriceList { Id = 1, Name = "Bảng giá 2026", EffectiveDate = new DateOnly(2026, 1, 1) };
        _priceListRepoMock.Setup(r => r.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        var dto = new CreatePriceItemDto { ServiceCode = "SVC-TEST", UnitPrice = 1000000m };

        var result = await _priceListService.AddPriceItemAsync(1, dto);

        result.Should().NotBeNull();
        result.ServiceCode.Should().Be("SVC-TEST");
        result.UnitPrice.Should().Be(1000000m);
        _priceListRepoMock.Verify(r => r.AddItemAsync(It.IsAny<PriceItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _priceListRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50000)]
    public async Task AddPriceItem_WithZeroOrNegativeUnitPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
        var priceList = new PriceList { Id = 1, Name = "Bảng giá 2026", EffectiveDate = new DateOnly(2026, 1, 1) };
        _priceListRepoMock.Setup(r => r.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        var dto = new CreatePriceItemDto { ServiceCode = "SVC-TEST", UnitPrice = invalidPrice };

        Func<Task> act = async () => await _priceListService.AddPriceItemAsync(1, dto);

        // Bắt buộc đơn giá phải > 0
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*lớn hơn 0*");
        _priceListRepoMock.Verify(r => r.AddItemAsync(It.IsAny<PriceItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddPriceItem_WhenPriceListNotFound_ShouldThrowKeyNotFoundException()
    {
        _priceListRepoMock.Setup(r => r.GetByIdWithItemsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var dto = new CreatePriceItemDto { ServiceCode = "SVC-TEST", UnitPrice = 500000m };

        Func<Task> act = async () => await _priceListService.AddPriceItemAsync(999, dto);

        // Bắt buộc liên kết PriceList cha phải hợp lệ
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }
}
