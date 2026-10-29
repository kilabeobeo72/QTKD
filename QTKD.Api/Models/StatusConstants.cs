namespace QTKD.Api.Models;

public static class CustomerStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}

public static class ContractStatus
{
    /// <summary>Bản nháp - Có thể chỉnh sửa</summary>
    public const string Draft = "Draft";

    /// <summary>Chờ phê duyệt </summary>
    public const string PendingApproval = "PendingApproval";

    /// <summary>Yêu cầu chỉnh sửa - Cho phép hiệu chỉnh</summary>
    public const string RevisionRequested = "RevisionRequested";

    /// <summary>Đã phê duyệt - Chờ ngày hiệu lực</summary>
    public const string Approved = "Approved";

    /// <summary>Bị từ chối</summary>
    public const string Rejected = "Rejected";

    /// <summary>Có hiệu lực</summary>
    public const string Effective = "Effective";
}
