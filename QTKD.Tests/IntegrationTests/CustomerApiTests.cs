using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using Xunit;

namespace QTKD.Tests.IntegrationTests;

public class CustomerApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CustomerApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region TC01: Tạo Customer hợp lệ -> Tạo được và đọc lại từ DB

    [Fact]
    public async Task TC01_CreateCustomer_WithValidData_Returns201AndPersistsInDatabase()
    {
        var requestDto = new CreateCustomerDTO
        {
            Name = "Công ty TNHH Hải Đăng Tech"
        };

        // Gọi API POST tạo khách hàng
        var postResponse = await _client.PostAsJsonAsync("/api/customers", requestDto);

        // Kiểm tra HTTP status code là 201 Created
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCustomer = await postResponse.Content.ReadFromJsonAsync<CustomerDto>();
        createdCustomer.Should().NotBeNull();
        createdCustomer!.Id.Should().BeGreaterThan(0);
        createdCustomer.Name.Should().Be("Công ty TNHH Hải Đăng Tech");
        createdCustomer.Status.Should().Be("Active");

        // Gọi API GET đọc lại từ Database để kiểm tra tính toàn vẹn
        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var readBackCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        readBackCustomer.Should().NotBeNull();
        readBackCustomer!.Id.Should().Be(createdCustomer.Id);
        readBackCustomer.Name.Should().Be("Công ty TNHH Hải Đăng Tech");
        readBackCustomer.Status.Should().Be("Active");

        // Kiểm tra trực tiếp trong DbContext của Test
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbCustomer = await db.Customers.FindAsync(createdCustomer.Id);
        dbCustomer.Should().NotBeNull();
        dbCustomer!.Name.Should().Be("Công ty TNHH Hải Đăng Tech");
    }

    #endregion

    #region TC02: Tạo Customer thiếu thông tin bắt buộc -> Bị từ chối; có thông báo lỗi

    [Fact]
    public async Task TC02_CreateCustomer_WithMissingName_Returns400BadRequest()
    {
        // Tên khách hàng NULL
        var requestDto = new CreateCustomerDTO
        {
            Name = ""
        };
        var response = await _client.PostAsJsonAsync("/api/customers", requestDto);

        // Bị từ chối với HTTP 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().NotBeNullOrWhiteSpace();
        errorContent.Should().ContainAny("bắt buộc", "không được để trống");
    }

    #endregion
}
