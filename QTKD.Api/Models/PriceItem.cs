namespace QTKD.Api.Models;

public class PriceItem
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    public PriceList? PriceList { get; set; }
}
