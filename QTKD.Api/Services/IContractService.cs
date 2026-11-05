using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<List<ContractResponseDto>> GetAllContractsAsync(CancellationToken cancellationToken = default);
    Task<ContractResponseDto> GetContractByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ContractResponseDto> CreateContractAsync(CreateContractDto dto, CancellationToken cancellationToken = default);
    Task<ContractResponseDto> UpdateContractAsync(int id, UpdateContractDto dto, CancellationToken cancellationToken = default);
    Task<SubmitContractResponseDto> SubmitContractAsync(int id, CancellationToken cancellationToken = default);
}