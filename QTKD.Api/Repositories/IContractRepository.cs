using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdWithCustomerAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Contract>> GetAllWithCustomerAsync(CancellationToken cancellationToken = default);
    Task<Contract?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByContractNoAsync(string contractNo, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
