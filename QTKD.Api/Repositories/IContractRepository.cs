using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdWithCustomerAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Contract>> GetAllWithCustomerAsync(CancellationToken cancellationToken = default);
}
