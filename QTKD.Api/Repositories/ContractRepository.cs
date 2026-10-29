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
}
