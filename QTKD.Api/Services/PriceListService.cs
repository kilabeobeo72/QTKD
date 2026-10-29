using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class PriceListService : IPriceListService
{
    private readonly IPriceListRepository _priceListRepository;

    public PriceListService(IPriceListRepository priceListRepository)
    {
        _priceListRepository = priceListRepository;
    }

    public async Task<List<PriceListDto>> GetAllPriceListsAsync(CancellationToken cancellationToken = default)
    {
        var priceLists = await _priceListRepository.GetAllWithItemsAsync(cancellationToken);

        return priceLists.Select(pl => new PriceListDto
        {
            Id = pl.Id,
            Name = pl.Name,
            EffectiveDate = pl.EffectiveDate,
            Items = pl.Items.OrderBy(i => i.Id).Select(i => new PriceItemDto
            {
                Id = i.Id,
                PriceListId = i.PriceListId,
                ServiceCode = i.ServiceCode,
                UnitPrice = i.UnitPrice
            }).ToList()
        }).ToList();
    }
}
