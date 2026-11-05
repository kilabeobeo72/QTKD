using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly QtkdDbContext _dbContext;

    public ContractRepository(QtkdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Contract?> GetByIdWithCustomerAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Contract>> GetAllWithCustomerAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .AsNoTracking()
            .Include(c => c.Customer)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contract?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByContractNoAsync(string contractNo, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Contracts.AsNoTracking().Where(c => c.ContractNo == contractNo);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await _dbContext.Contracts.AddAsync(contract, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
}
