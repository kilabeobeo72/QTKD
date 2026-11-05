using QTKD.Api.DTOs;
using QTKD.Api.Models;
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

        return priceLists.Select(MapToDto).ToList();
    }

    public async Task<PriceListDto?> GetPriceListByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var priceList = await _priceListRepository.GetByIdWithItemsAsync(id, cancellationToken);
        return priceList == null ? null : MapToDto(priceList);
    }

    public async Task<PriceListDto> CreatePriceListAsync(CreatePriceListDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Tên bảng giá không được để trống.");
        }

        var priceList = new PriceList
        {
            Name = dto.Name.Trim(),
            EffectiveDate = dto.EffectiveDate
        };

        await _priceListRepository.AddAsync(priceList, cancellationToken);
        await _priceListRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(priceList);
    }

    public async Task<PriceItemDto> AddPriceItemAsync(int priceListId, CreatePriceItemDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Bảng giá cha phải tồn tại
        var priceList = await _priceListRepository.GetByIdWithItemsAsync(priceListId, cancellationToken);
        if (priceList == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bảng giá với Id = {priceListId}.");
        }

        // 2. Đơn giá phải lớn hơn 0
        if (dto.UnitPrice <= 0)
        {
            throw new ArgumentException("Đơn giá dịch vụ phải lớn hơn 0.");
        }

        if (string.IsNullOrWhiteSpace(dto.ServiceCode))
        {
            throw new ArgumentException("Mã dịch vụ không được để trống.");
        }

        // 3. Tạo PriceItem và liên kết đúng PriceListId
        var item = new PriceItem
        {
            PriceListId = priceListId,
            ServiceCode = dto.ServiceCode.Trim().ToUpper(),
            UnitPrice = dto.UnitPrice
        };

        await _priceListRepository.AddItemAsync(item, cancellationToken);
        await _priceListRepository.SaveChangesAsync(cancellationToken);

        return new PriceItemDto
        {
            Id = item.Id,
            PriceListId = item.PriceListId,
            ServiceCode = item.ServiceCode,
            UnitPrice = item.UnitPrice
        };
    }

    private static PriceListDto MapToDto(PriceList pl)
    {
        return new PriceListDto
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
        };
    }
}