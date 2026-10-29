using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<ContractResponseDto> GetContractByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ContractResponseDto>> GetAllContractsAsync(CancellationToken cancellationToken = default);
}
