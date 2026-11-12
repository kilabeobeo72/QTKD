using FluentAssertions;
using Moq;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;
using Xunit;

namespace QTKD.Tests.UnitTests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _customerRepoMock = new Mock<ICustomerRepository>();
        _customerService = new CustomerService(_customerRepoMock.Object);
    }

    [Fact]
    public async Task CreateCustomer_WithValidName_ShouldSucceedWithActiveStatus()
    {
        var dto = new CreateCustomerDTO { Name = "Công ty TNHH Sao Mai" };

        var result = await _customerService.CreateCustomerAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Công ty TNHH Sao Mai");
        result.Status.Should().Be(CustomerStatus.Active);
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateCustomer_WithMissingName_ShouldThrowArgumentException(string? invalidName)
    {
        var dto = new CreateCustomerDTO { Name = invalidName! };

        Func<Task> act = async () => await _customerService.CreateCustomerAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Tên khách hàng không được để trống*");
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCustomer_WhenCustomerNotFound_ShouldThrowKeyNotFoundException()
    {
        _customerRepoMock.Setup(r => r.GetByIdForUpdateAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var dto = new UpdateCustomerDTO { Name = "Tên mới", Status = CustomerStatus.Active };

        Func<Task> act = async () => await _customerService.UpdateCustomerAsync(999, dto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }
}
