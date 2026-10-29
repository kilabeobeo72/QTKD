namespace QTKD.Api.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = CustomerStatus.Active;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
