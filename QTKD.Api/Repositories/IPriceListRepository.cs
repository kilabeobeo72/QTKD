using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IPriceListRepository
{
    Task<List<PriceList>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
}
