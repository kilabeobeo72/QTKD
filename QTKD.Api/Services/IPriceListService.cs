using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<List<PriceListDto>> GetAllPriceListsAsync(CancellationToken cancellationToken = default);
}
