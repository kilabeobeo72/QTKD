using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using Xunit;

namespace QTKD.Tests.IntegrationTests;

/// <summary>
/// Kiểm thử tích hợp toàn bộ luồng nghiệp vụ xuyên suốt:
/// Tạo Customer -> Tạo Contract Draft -> Gửi thiếu dữ liệu (Thất bại) -> Bổ sung dữ liệu -> Gửi lại (Thành công) -> Kiểm tra Database.
/// </summary>
public class FullFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public FullFlowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullDemoFlow_FromCustomerToContractSubmit_ShouldSucceedAllSteps()
    {
        // --------------------------------------------------------------------
        // BƯỚC 1: Tạo Khách hàng mới (POST /api/customers)
        // --------------------------------------------------------------------
        var customerName = "Công ty Cổ phần Alpha Global " + Guid.NewGuid().ToString("N")[..6];
        var createCustomerResponse = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerDTO
        {
            Name = customerName
        });
        createCustomerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await createCustomerResponse.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Id.Should().BeGreaterThan(0);
        customer.Status.Should().Be("Active");

        // --------------------------------------------------------------------
        // BƯỚC 2: Lập hợp đồng nháp nhưng THIẾU mô tả (POST /api/contracts)
        // --------------------------------------------------------------------
        var contractNo = "HD-FLOW-" + Guid.NewGuid().ToString("N")[..6];
        var createContractResponse = await _client.PostAsJsonAsync("/api/contracts", new CreateContractDto
        {
            ContractNo = contractNo,
            CustomerId = customer.Id,
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2027, 7, 1),
            Description = "" // Rỗng
        });
        createContractResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var contract = await createContractResponse.Content.ReadFromJsonAsync<ContractResponseDto>();
        contract.Should().NotBeNull();
        contract!.Status.Should().Be(ContractStatus.Draft);

        // --------------------------------------------------------------------
        // BƯỚC 3: Gửi duyệt khi thiếu dữ liệu -> THẤT BẠI (CTR-02 / AC03)
        // --------------------------------------------------------------------
        var failedSubmitResponse = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        failedSubmitResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Đọc lại từ DB: xác nhận hợp đồng vẫn là Draft
        var getContractCheck = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var contractStillDraft = await getContractCheck.Content.ReadFromJsonAsync<ContractResponseDto>();
        contractStillDraft!.Status.Should().Be(ContractStatus.Draft);

        // --------------------------------------------------------------------
        // BƯỚC 4: Bổ sung dữ liệu hợp đồng (PUT /api/contracts/{id})
        // --------------------------------------------------------------------
        var updateResponse = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", new UpdateContractDto
        {
            CustomerId = customer.Id,
            StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2027, 7, 1),
            Description = "Cung cấp hệ thống giải pháp ERP toàn diện và bảo hành 12 tháng."
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // --------------------------------------------------------------------
        // BƯỚC 5: Gửi duyệt lại -> THÀNH CÔNG (CTR-02, CTR-03, AC04)
        // --------------------------------------------------------------------
        var successSubmitResponse = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        successSubmitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var submitResult = await successSubmitResponse.Content.ReadFromJsonAsync<SubmitContractResponseDto>();
        submitResult.Should().NotBeNull();
        submitResult!.PreviousStatus.Should().Be(ContractStatus.Draft);
        submitResult.CurrentStatus.Should().Be(ContractStatus.PendingApproval);

        // --------------------------------------------------------------------
        // BƯỚC 6: Kiểm tra trực tiếp Database (Yêu cầu 17 & 18)
        // --------------------------------------------------------------------
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.FindAsync(contract.Id);

        dbContract.Should().NotBeNull();
        dbContract!.Status.Should().Be(ContractStatus.PendingApproval);
        dbContract.Description.Should().Be("Cung cấp hệ thống giải pháp ERP toàn diện và bảo hành 12 tháng.");
    }
}
