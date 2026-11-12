using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using Xunit;

namespace QTKD.Tests.IntegrationTests;

public class ContractApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ContractApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region TC03 & TC04: Sửa hợp đồng (CTR-01) qua API PUT /api/contracts/{id}

    [Fact]
    public async Task TC03_UpdateContract_WhenDraft_Returns200Ok()
    {
        // Tạo hợp đồng Draft
        var contractNo = "HD-TEST-TC03-" + Guid.NewGuid().ToString("N")[..6];
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", new CreateContractDto
        {
            ContractNo = contractNo,
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "Hợp đồng nháp cần cập nhật"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContractResponseDto>();

        // Cập nhật hợp đồng ở trạng thái Draft
        var updateResponse = await _client.PutAsJsonAsync($"/api/contracts/{created!.Id}", new UpdateContractDto
        {
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 12, 31),
            Description = "Đã cập nhật gia hạn thời hạn"
        });

        // HTTP 200 OK và dữ liệu được cập nhật thành công
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ContractResponseDto>();
        updated.Should().NotBeNull();
        updated!.EndDate.Should().Be(new DateOnly(2027, 12, 31));
        updated.Description.Should().Be("Đã cập nhật gia hạn thời hạn");
    }

    [Fact]
    public async Task TC04_UpdateContract_WhenPendingApproval_Returns400BadRequest()
    {
        // Tìm hoặc tạo hợp đồng và chuyển sang PendingApproval
        var contractNo = "HD-TEST-TC04-" + Guid.NewGuid().ToString("N")[..6];
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", new CreateContractDto
        {
            ContractNo = contractNo,
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "Hợp đồng đầy đủ để submit"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ContractResponseDto>();

        // Submit để chuyển sang PendingApproval
        var submitResponse = await _client.PostAsync($"/api/contracts/{created!.Id}/submit", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Thử sửa hợp đồng khi đang ở PendingApproval
        var updateResponse = await _client.PutAsJsonAsync($"/api/contracts/{created.Id}", new UpdateContractDto
        {
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2028, 1, 1),
            Description = "Cố tình sửa đổi hợp đồng đang chờ duyệt"
        });

        // Bị từ chối với HTTP 400 Bad Request
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorJson = await updateResponse.Content.ReadAsStringAsync();
        errorJson.Should().Contain("Draft");

        // Đọc lại từ API để kiểm tra dữ liệu trong DB KHÔNG bị thay đổi
        var getResponse = await _client.GetAsync($"/api/contracts/{created.Id}");
        var currentContract = await getResponse.Content.ReadFromJsonAsync<ContractResponseDto>();
        currentContract!.Status.Should().Be(ContractStatus.PendingApproval);
        currentContract.Description.Should().Be("Hợp đồng đầy đủ để submit");
    }

    #endregion

    #region TC05 & TC06: Submit hợp đồng (CTR-02, CTR-03) qua API POST /api/contracts/{id}/submit

    [Fact]
    public async Task TC05_SubmitContract_WhenMissingDescription_Returns400BadRequest()
    {
        // Tạo hợp đồng nháp nhưng thiếu Description
        var contractNo = "HD-TEST-TC05-" + Guid.NewGuid().ToString("N")[..6];
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", new CreateContractDto
        {
            ContractNo = contractNo,
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "" // Rỗng
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ContractResponseDto>();

        // Thử gửi trình duyệt khi thiếu nội dung bắt buộc
        var submitResponse = await _client.PostAsync($"/api/contracts/{created!.Id}/submit", null);

        // Bị từ chối với HTTP 400 Bad Request
        submitResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await submitResponse.Content.ReadAsStringAsync();
        error.Should().ContainAny("CTR-02", "bắt buộc");

        // Kiểm tra trong DB: Hợp đồng vẫn giữ nguyên trạng thái Draft
        var getResponse = await _client.GetAsync($"/api/contracts/{created.Id}");
        var contractInDb = await getResponse.Content.ReadFromJsonAsync<ContractResponseDto>();
        contractInDb!.Status.Should().Be(ContractStatus.Draft);
    }

    [Fact]
    public async Task TC06_SubmitContract_WhenValid_Returns200OkAndStatusIsPendingApproval()
    {
        // Hợp đồng hợp lệ đáp ứng đầy đủ CTR-02
        var contractNo = "HD-TEST-TC06-" + Guid.NewGuid().ToString("N")[..6];
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", new CreateContractDto
        {
            ContractNo = contractNo,
            CustomerId = 1, // Khách hàng Active
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "Điều khoản hợp đồng hoàn chỉnh hợp lệ"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ContractResponseDto>();

        // Gửi trình duyệt
        var submitResponse = await _client.PostAsync($"/api/contracts/{created!.Id}/submit", null);

        // Trả về HTTP 200 OK
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var submitResult = await submitResponse.Content.ReadFromJsonAsync<SubmitContractResponseDto>();
        submitResult.Should().NotBeNull();
        submitResult!.PreviousStatus.Should().Be(ContractStatus.Draft);
        submitResult.CurrentStatus.Should().Be(ContractStatus.PendingApproval);

        // Chuyển sang PendingApproval, không chuyển thẳng sang Approved
        var getResponse = await _client.GetAsync($"/api/contracts/{created.Id}");
        var contractInDb = await getResponse.Content.ReadFromJsonAsync<ContractResponseDto>();
        contractInDb!.Status.Should().Be(ContractStatus.PendingApproval);
        contractInDb.Status.Should().NotBe(ContractStatus.Approved);
    }

    #endregion

    #region TC08: Submit lặp một Contract (AC05)

    [Fact]
    public async Task TC08_SubmitContract_WhenAlreadyPendingApproval_Returns400BadRequest()
    {
        // Tạo hợp đồng và submit lần 1 thành công
        var contractNo = "HD-TEST-TC08-" + Guid.NewGuid().ToString("N")[..6];
        var createResponse = await _client.PostAsJsonAsync("/api/contracts", new CreateContractDto
        {
            ContractNo = contractNo,
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "Hợp đồng thử nghiệm submit lặp"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ContractResponseDto>();

        var firstSubmit = await _client.PostAsync($"/api/contracts/{created!.Id}/submit", null);
        firstSubmit.StatusCode.Should().Be(HttpStatusCode.OK);

        // Thử submit lần 2 đối với cùng một hợp đồng (AC05)
        var secondSubmit = await _client.PostAsync($"/api/contracts/{created.Id}/submit", null);

        // Bị từ chối với HTTP 400 Bad Request, không tạo phiên xử lý trùng
        secondSubmit.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await secondSubmit.Content.ReadAsStringAsync();
        errorContent.Should().ContainAny("Chờ phê duyệt", "PendingApproval", "BUSINESS_RULE_VIOLATION");
    }

    #endregion
}
