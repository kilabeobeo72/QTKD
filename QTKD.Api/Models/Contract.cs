namespace QTKD.Api.Models;

public class Contract
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = ContractStatus.Draft;
    public string? Description { get; set; }

    public Customer? Customer { get; set; }
}
