using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<List<PriceListDto>> GetAllPriceListsAsync(CancellationToken cancellationToken = default);
    Task<PriceListDto?> GetPriceListByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PriceListDto> CreatePriceListAsync(CreatePriceListDto dto, CancellationToken cancellationToken = default);
    Task<PriceItemDto> AddPriceItemAsync(int priceListId, CreatePriceItemDto dto, CancellationToken cancellationToken = default);
}


