using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public class ContractResponseDto
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerStatus { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateContractDTO 
{
    [Required(ErrorMessage = "Số hợp đồng là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Số hợp đồng tối đa 50 ký tự.")]
    public string ContractNo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "CustomerId phải lớn hơn 0.")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    public DateOnly? StartDate { get; set; }
    
    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    public DateOnly? EndDate { get; set; }

    // Chỉ bắt buộc lúc Submit (CTR-02)
    [StringLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự.")]
    public string? Description { get; set; }
}

public class UpdateContractDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "CustomerId phải lớn hơn 0.")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    public DateOnly? StartDate { get; set; }
    
    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    public DateOnly? EndDate { get; set; }

    [StringLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự.")]
    public string? Description { get; set; }
}

public class CreateContractDto : CreateContractDTO { }
public class UpdateContractDto : UpdateContractDTO { }

public class SubmitContractResponseDto
{
    public int ContractId { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
