using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;    private readonly ICustomerRepository _customerRepository;


    public ContractService(IContractRepository contractRepository, ICustomerRepository customerRepository)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;  
    }

    public async Task<ContractResponseDto> GetContractByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id hợp đồng phải lớn hơn 0.");
        }

        var contract = await _contractRepository.GetByIdWithCustomerAsync(id, cancellationToken);
        if (contract is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng có Id = {id}.");
        }

        return MapToDto(contract);
    }

    public async Task<List<ContractResponseDto>> GetAllContractsAsync(CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepository.GetAllWithCustomerAsync(cancellationToken);
        return contracts.Select(MapToDto).ToList();
    }

    public async Task<ContractResponseDto> CreateContractAsync(CreateContractDto dto, CancellationToken cancellationToken = default)
    {
         // 1. Kiểm tra mã hợp đồng
        if (string.IsNullOrWhiteSpace(dto.ContractNo))
        {
            throw new ArgumentException("Mã số hợp đồng không được để trống.");
        }       

        var isDuplicate = await _contractRepository.ExistsByContractNoAsync(dto.ContractNo.Trim(), null, cancellationToken);
        if (isDuplicate)
        {
            throw new InvalidOperationException($"Mã hợp đồng '{dto.ContractNo}' đã tồn tại trong hệ thống.");
        }

        // 2. Kiểm tra Customer hợp lệ
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Khách hàng với Id = {dto.CustomerId} không tồn tại.");
        }

        // 3. Kiểm tra ngày hiệu lực
        if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
        {
            throw new ArgumentException("Ngày bắt đầu và ngày kết thúc không được để trống.");
        }
        if (dto.EndDate.Value < dto.StartDate.Value)
        {
            throw new ArgumentException("Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu."); 
        }

        // 4. Tạo hợp đồng ở trạng thái Draft ban đầu
        var contract = new Contract
        {
            ContractNo = dto.ContractNo.Trim(),
            CustomerId = dto.CustomerId,
            StartDate = dto.StartDate.Value,
            EndDate = dto.EndDate.Value,
            Status = ContractStatus.Draft,
            Description = dto.Description?.Trim()
        };
        await _contractRepository.AddAsync(contract, cancellationToken);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        var created = await _contractRepository.GetByIdWithCustomerAsync(contract.Id, cancellationToken);
        return MapToDto(created!);

    }
    
    public async Task<ContractResponseDto> UpdateContractAsync(int id, UpdateContractDto dto, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (contract == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng có Id = {id}.");
        }

        // --- CTR-01 ---
        // Chỉ cho phép sửa hợp đồng khi ở trạng thái Draft hoặc RevisionRequested
        if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.RevisionRequested)
        {
            throw new InvalidOperationException(
                $"Chỉ được chỉnh sửa hợp đồng ở trạng thái 'Draft' hoặc 'RevisionRequested'. Trạng thái hiện tại: '{contract.Status}'.");
        }

        // Kiểm tra khách hàng
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Khách hàng với Id = {dto.CustomerId} không tồn tại.");
        }
        // Kiểm tra ngày hiệu lực
        if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
        {
            throw new ArgumentException("Ngày bắt đầu và ngày kết thúc không được để trống.");
        }
        if (dto.EndDate.Value < dto.StartDate.Value)
        {
            throw new ArgumentException("Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu.");
        }    

        contract.CustomerId = dto.CustomerId;
        contract.StartDate = dto.StartDate.Value;
        contract.EndDate = dto.EndDate.Value;
        contract.Description = dto.Description?.Trim();
        await _contractRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(contract);        
    }

    public async Task<SubmitContractResponseDto> SubmitContractAsync(int id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (contract == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng có Id = {id}.");
        }
        var previousStatus = contract.Status;
        
        // 2. Kiểm tra trạng thái cho phép gửi
        if (contract.Status == ContractStatus.PendingApproval)
        {
            throw new InvalidOperationException("Hợp đồng đã ở trạng thái Chờ phê duyệt, không thể gửi duyệt lại.");
        }
        if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.RevisionRequested)
        {
            throw new InvalidOperationException(
                $"Hợp đồng ở trạng thái '{contract.Status}' không được phép gửi trình duyệt. Chỉ gửi khi ở 'Draft' hoặc 'RevisionRequested'.");
        }
        
        // 3. CTR-02: Kiểm tra các điều kiện bắt buộc
        var errors = new List<string>();
        // Customer phải tồn tại và ở trạng thái Active
        if (contract.Customer == null || contract.Customer.Status != CustomerStatus.Active)
        {
            errors.Add("Khách hàng của hợp đồng không hợp lệ hoặc đang không ở trạng thái 'Active'.");
        }
        // Ngày hiệu lực hợp lệ
        if (contract.EndDate < contract.StartDate)
        {
            errors.Add("Ngày kết thúc hiệu lực không được nhỏ hơn ngày bắt đầu.");
        }
        // Nội dung/tài liệu bắt buộc
        if (string.IsNullOrWhiteSpace(contract.Description))
        {
            errors.Add("Nội dung tóm tắt/điều khoản hợp đồng là bắt buộc trước khi trình duyệt.");
        }
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Không đáp ứng quy tắc CTR-02 để gửi duyệt hợp đồng:\n- {string.Join("\n- ", errors)}");
        }
        // 4. CTR-03: Đổi sang PendingApproval
        contract.Status = ContractStatus.PendingApproval;
        await _contractRepository.SaveChangesAsync(cancellationToken);
        return new SubmitContractResponseDto
        {
            ContractId = contract.Id,
            ContractNo = contract.ContractNo,
            PreviousStatus = previousStatus,
            CurrentStatus = contract.Status,
            SubmittedAt = DateTime.UtcNow,
            Message = "Hợp đồng đã được gửi trình duyệt thành công và chuyển sang trạng thái Chờ phê duyệt."
        };
    }    

    private static ContractResponseDto MapToDto(Contract contract)
    {
        return new ContractResponseDto
        {
            Id = contract.Id,
            ContractNo = contract.ContractNo,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer?.Name ?? string.Empty,
            CustomerStatus = contract.Customer?.Status ?? string.Empty,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            Description = contract.Description
        };
    }
}
