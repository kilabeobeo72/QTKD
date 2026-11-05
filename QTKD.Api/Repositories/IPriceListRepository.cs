using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IPriceListRepository
{
    Task<List<PriceList>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default);
    Task AddItemAsync(PriceItem priceItem, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
