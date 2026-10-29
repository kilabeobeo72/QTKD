using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
}
