using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;

    public ContractService(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
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
