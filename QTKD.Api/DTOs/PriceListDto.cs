namespace QTKD.Api.DTOs;

public class PriceListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public List<PriceItemDto> Items { get; set; } = new();
}

public class PriceItemDto
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
