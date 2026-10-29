using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public class PriceListRepository : IPriceListRepository
{
    private readonly QtkdDbContext _dbContext;

    public PriceListRepository(QtkdDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PriceList>> GetAllWithItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PriceLists
            .AsNoTracking()
            .Include(pl => pl.Items)
            .OrderBy(pl => pl.Id)
            .ToListAsync(cancellationToken);
    }
}
