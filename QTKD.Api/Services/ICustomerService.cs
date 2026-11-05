using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateCustomerAsync(int id, CustomerUpdateDto dto, CancellationToken cancellationToken = default);
}
