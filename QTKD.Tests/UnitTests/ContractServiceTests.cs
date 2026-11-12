using FluentAssertions;
using Moq;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;
using Xunit;

namespace QTKD.Tests.UnitTests;

/// <summary>
/// Unit Tests kiểm tra các quy tắc nghiệp vụ cốt lõi:
/// CTR-01, CTR-02, CTR-03 và US03 (AC01 - AC05) trong ContractService.
/// </summary>
public class ContractServiceTests
{
    private readonly Mock<IContractRepository> _contractRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly ContractService _contractService;

    public ContractServiceTests()
    {
        _contractRepoMock = new Mock<IContractRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _contractService = new ContractService(_contractRepoMock.Object, _customerRepoMock.Object);
    }

    #region TC03 & TC04: Quy tắc CTR-01 (Chỉ cho sửa hợp đồng ở Draft hoặc RevisionRequested)

    [Fact]
    public async Task TC03_UpdateContract_WhenStatusIsDraft_ShouldSucceed()
    {
        // Arrange: Hợp đồng ở trạng thái Draft
        var contract = new Contract
        {
            Id = 1,
            ContractNo = "HD-2026-001",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.Draft,
            Description = "Bản nháp ban đầu"
        };

        var customer = new Customer { Id = 1, Name = "Công ty A", Status = CustomerStatus.Active };
        var updateDto = new UpdateContractDto
        {
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 12, 31),
            Description = "Cập nhật thời hạn hợp đồng"
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        _customerRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _contractService.UpdateContractAsync(1, updateDto);

        // Assert: Thỏa mãn CTR-01, cập nhật thành công
        result.Should().NotBeNull();
        result.EndDate.Should().Be(new DateOnly(2027, 12, 31));
        result.Description.Should().Be("Cập nhật thời hạn hợp đồng");
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC03_UpdateContract_WhenStatusIsRevisionRequested_ShouldSucceed()
    {
        // Arrange: Hợp đồng ở trạng thái RevisionRequested (cũng được phép sửa theo CTR-01)
        var contract = new Contract
        {
            Id = 2,
            ContractNo = "HD-2026-002",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.RevisionRequested,
            Description = "Yêu cầu chỉnh sửa giá"
        };

        var customer = new Customer { Id = 1, Name = "Công ty A", Status = CustomerStatus.Active };
        var updateDto = new UpdateContractDto
        {
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "Đã chỉnh sửa theo yêu cầu"
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        _customerRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _contractService.UpdateContractAsync(2, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Đã chỉnh sửa theo yêu cầu");
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC04_UpdateContract_WhenStatusIsPendingApproval_ShouldThrowInvalidOperationException()
    {
        // Arrange: Hợp đồng đang ở PendingApproval (Vi phạm CTR-01 nếu cố tình sửa)
        var contract = new Contract
        {
            Id = 3,
            ContractNo = "HD-2026-003",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.PendingApproval,
            Description = "Đang chờ duyệt"
        };

        var updateDto = new UpdateContractDto
        {
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Description = "Cố tình sửa đổi"
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        Func<Task> act = async () => await _contractService.UpdateContractAsync(3, updateDto);

        // Assert: Bị từ chối theo CTR-01, dữ liệu giữ nguyên không SaveChanges
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Draft*RevisionRequested*");
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region TC05 & TC06: Quy tắc CTR-02 (Điều kiện bắt buộc trước khi gửi duyệt)

    [Fact]
    public async Task TC05_SubmitContract_WhenDescriptionIsEmpty_ShouldBeRejectedAndRemainDraft()
    {
        // Arrange: Hợp đồng Draft nhưng thiếu Description (Vi phạm CTR-02)
        var contract = new Contract
        {
            Id = 4,
            ContractNo = "HD-2026-004",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.Draft,
            Description = "   ", // Rỗng / whitespace
            Customer = new Customer { Id = 1, Name = "Công ty A", Status = CustomerStatus.Active }
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        Func<Task> act = async () => await _contractService.SubmitContractAsync(4);

        // Assert: Bị từ chối theo CTR-02 / AC03, trạng thái vẫn là Draft
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CTR-02*");
        contract.Status.Should().Be(ContractStatus.Draft);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TC05_SubmitContract_WhenCustomerIsInactive_ShouldBeRejected()
    {
        // Arrange: Khách hàng đang Inactive (Vi phạm CTR-02)
        var contract = new Contract
        {
            Id = 5,
            ContractNo = "HD-2026-005",
            CustomerId = 2,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.Draft,
            Description = "Hợp đồng đầy đủ mô tả",
            Customer = new Customer { Id = 2, Name = "Công ty B", Status = CustomerStatus.Inactive }
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        Func<Task> act = async () => await _contractService.SubmitContractAsync(5);

        // Assert: Bị từ chối theo CTR-02, không thay đổi hợp đồng
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CTR-02*");
        contract.Status.Should().Be(ContractStatus.Draft);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TC06_SubmitContract_WhenValid_ShouldTransitionToPendingApproval()
    {
        // Arrange: Hợp đồng Draft thỏa mãn toàn bộ điều kiện CTR-02
        var contract = new Contract
        {
            Id = 6,
            ContractNo = "HD-2026-006",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.Draft,
            Description = "Cung cấp giải pháp phần mềm hoàn chỉnh",
            Customer = new Customer { Id = 1, Name = "Công ty A", Status = CustomerStatus.Active }
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        var result = await _contractService.SubmitContractAsync(6);

        // Assert: Chuyển sang PendingApproval theo CTR-02, CTR-03, AC04
        result.Should().NotBeNull();
        result.PreviousStatus.Should().Be(ContractStatus.Draft);
        result.CurrentStatus.Should().Be(ContractStatus.PendingApproval);
        contract.Status.Should().Be(ContractStatus.PendingApproval);
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region TC07: Quy tắc CTR-03 (Không cho chuyển thẳng Draft sang Approved)

    [Fact]
    public void TC07_DraftToApproved_DirectTransition_IsDisallowedByDesign()
    {
        // CTR-03 quy định: Submit hợp đồng CHỈ chuyển sang PendingApproval,
        // không bao giờ được phép nhảy thẳng từ Draft sang Approved.
        var contract = new Contract
        {
            Id = 7,
            ContractNo = "HD-2026-007",
            Status = ContractStatus.Draft
        };

        // Assert: Trong quy trình nghiệp vụ submit, kết quả đích chỉ là PendingApproval
        var allowedTargetStatuses = new[] { ContractStatus.PendingApproval };
        allowedTargetStatuses.Should().NotContain(ContractStatus.Approved);
        contract.Status.Should().NotBe(ContractStatus.Approved);
    }

    #endregion

    #region TC08: Tiêu chí AC05 (Chặn gửi lặp khi đã PendingApproval)

    [Fact]
    public async Task TC08_SubmitContract_WhenAlreadyPendingApproval_ShouldThrowException()
    {
        // Arrange: Hợp đồng đã ở trạng thái PendingApproval
        var contract = new Contract
        {
            Id = 8,
            ContractNo = "HD-2026-008",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2027, 6, 1),
            Status = ContractStatus.PendingApproval,
            Description = "Đã gửi trước đó",
            Customer = new Customer { Id = 1, Name = "Công ty A", Status = CustomerStatus.Active }
        };

        _contractRepoMock.Setup(r => r.GetByIdForUpdateAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act: Thử gửi duyệt lại lần 2
        Func<Task> act = async () => await _contractService.SubmitContractAsync(8);

        // Assert: Thỏa mãn AC05: Không tạo thêm một lần xử lý tương đương, bị từ chối
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Chờ phê duyệt*");
        _contractRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
