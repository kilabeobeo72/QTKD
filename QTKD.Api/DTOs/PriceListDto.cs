using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

// --- PRICE LIST DTOs ---
public class PriceListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public List<PriceItemDto> Items { get; set; } = new();
}

public class CreatePriceListDto
{
    [Required(ErrorMessage = "Tên bảng giá là bắt buộc.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên bảng giá từ 2 đến 200 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày hiệu lực là bắt buộc.")]
    public DateOnly EffectiveDate { get; set; }
}

// --- PRICE ITEM DTOs ---
public class PriceItemDto
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

public class CreatePriceItemDto
{
    [Required(ErrorMessage = "Mã dịch vụ là bắt buộc.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Mã dịch vụ từ 2 đến 50 ký tự.")]
    public string ServiceCode { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
    public decimal UnitPrice { get; set; }
}

public class CreatePriceListDTO : CreatePriceListDto { }
public class PriceListItemDto : PriceItemDto { }
public class CreatePriceListItemDTO : CreatePriceItemDto { }
