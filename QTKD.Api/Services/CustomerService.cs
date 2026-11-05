using QTKD.Api.DTOs;
using QTKD.Api.Repositories;
using QTKD.Api.Models;

namespace QTKD.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Status = c.Status
        }).ToList();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer == null ? null : new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Tên khách hàng không được để trống
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Tên khách hàng không được để trống.");
        }

        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            Status = CustomerStatus.Active
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy khách hàng với Id = {id}.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Tên khách hàng không được để trống.");
        }

        customer.Name = dto.Name;
        customer.Status = dto.Status;

        await _customerRepository.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }
}
