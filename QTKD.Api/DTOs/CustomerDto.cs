using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CustomerCreateDto
{
    [Required(ErrorMessage = "Tên khách hàng là trường bắt buộc")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên khách hàng phải từ 2 đến 200 ký tự.")]
    public string Name { get; set; } = string.Empty;
}

public class CustomerUpdateDto 
{
    [Required(ErrorMessage = "Tên khách hàng là trường bắt buộc")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên khách hàng phải từ 2 đến 200 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ nhận 'Active' hoặc 'Inactive'.")]
    public string Status { get; set; } = string.Empty;
}

public class CreateCustomerDTO : CustomerCreateDto { }
public class UpdateCustomerDTO : CustomerUpdateDto { }
