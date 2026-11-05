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
            .Include(p => p.Items)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
        }
    public async Task<PriceList?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PriceLists
            .AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default)
    {
        await _dbContext.PriceLists.AddAsync(priceList, cancellationToken);
    }

    public async Task AddItemAsync(PriceItem priceItem, CancellationToken cancellationToken = default)
    {
        await _dbContext.PriceItems.AddAsync(priceItem, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
}
