using Microsoft.EntityFrameworkCore;
using QTKD.Api.Models;

namespace QTKD.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(QtkdDbContext context)
    {
        // 1. SEED THÊM KHÁCH HÀNG
        if (!await context.Customers.AnyAsync(c => c.Name == "Tập đoàn Viễn thông An Bình"))
        {
            var newCustomers = new List<Customer>
            {
                new Customer { Name = "Tập đoàn Viễn thông An Bình", Status = CustomerStatus.Active },
                new Customer { Name = "Công ty TNHH Logistics Toàn Cầu", Status = CustomerStatus.Active },
                new Customer { Name = "Công ty Thương mại Dịch vụ Nam Hải", Status = CustomerStatus.Inactive }, // Phục vụ test CTR-02
                new Customer { Name = "Ngân hàng TMCP Phương Đông", Status = CustomerStatus.Active }
            };
            await context.Customers.AddRangeAsync(newCustomers);
            await context.SaveChangesAsync();
        }

        // Lấy danh sách ID khách hàng thực tế trong DB
        var customerAnBinh = await context.Customers.FirstOrDefaultAsync(c => c.Name == "Tập đoàn Viễn thông An Bình");
        var customerLogistics = await context.Customers.FirstOrDefaultAsync(c => c.Name == "Công ty TNHH Logistics Toàn Cầu");
        var customerNamHai = await context.Customers.FirstOrDefaultAsync(c => c.Name == "Công ty Thương mại Dịch vụ Nam Hải");

        // 2. SEED THÊM HỢP ĐỒNG Ở CÁC TRẠNG THÁI KHÁC NHAU (TEST CTR-01, CTR-02, CTR-03, AC05)
        if (!await context.Contracts.AnyAsync(c => c.ContractNo == "HD-2026-020") && customerAnBinh != null)
        {
            var newContracts = new List<Contract>
            {
                // Hợp đồng ở trạng thái PendingApproval: Test CTR-01 (không cho sửa) và AC05 (không gửi lại)
                new Contract
                {
                    ContractNo = "HD-2026-020",
                    CustomerId = customerAnBinh.Id,
                    StartDate = new DateOnly(2026, 6, 1),
                    EndDate = new DateOnly(2027, 6, 1),
                    Status = ContractStatus.PendingApproval,
                    Description = "Hợp đồng triển khai hạ tầng số doanh nghiệp (Đang chờ duyệt)"
                },
                // Hợp đồng ở trạng thái RevisionRequested: Test CTR-01 (cho phép sửa khi ở RevisionRequested)
                new Contract
                {
                    ContractNo = "HD-2026-021",
                    CustomerId = customerLogistics != null ? customerLogistics.Id : customerAnBinh.Id,
                    StartDate = new DateOnly(2026, 7, 1),
                    EndDate = new DateOnly(2027, 7, 1),
                    Status = ContractStatus.RevisionRequested,
                    Description = "Hợp đồng tích hợp hệ thống kho vận - Yêu cầu điều chỉnh thời hạn bảo hành"
                },
                // Hợp đồng Draft nhưng THIẾU Description: Test CTR-02 (gửi duyệt thất bại do thiếu mô tả)
                new Contract
                {
                    ContractNo = "HD-2026-022",
                    CustomerId = customerAnBinh.Id,
                    StartDate = new DateOnly(2026, 8, 1),
                    EndDate = new DateOnly(2027, 8, 1),
                    Status = ContractStatus.Draft,
                    Description = null // Thiếu nội dung bắt buộc
                }
            };

            // Hợp đồng Draft với khách hàng Inactive: Test CTR-02 (gửi duyệt thất bại do Customer Inactive)
            if (customerNamHai != null)
            {
                newContracts.Add(new Contract
                {
                    ContractNo = "HD-2026-023",
                    CustomerId = customerNamHai.Id,
                    StartDate = new DateOnly(2026, 9, 1),
                    EndDate = new DateOnly(2027, 9, 1),
                    Status = ContractStatus.Draft,
                    Description = "Hợp đồng thử nghiệm với khách hàng Inactive"
                });
            }

            await context.Contracts.AddRangeAsync(newContracts);
            await context.SaveChangesAsync();
        }

        // 3. SEED THÊM BẢNG GIÁ & ĐƠN GIÁ DỊCH VỤ
        if (!await context.PriceLists.AnyAsync(pl => pl.Name == "Bảng giá Dịch vụ Cloud & Bảo mật 2026"))
        {
            var cloudPriceList = new PriceList
            {
                Name = "Bảng giá Dịch vụ Cloud & Bảo mật 2026",
                EffectiveDate = new DateOnly(2026, 3, 1),
                Items = new List<PriceItem>
                {
                    new PriceItem { ServiceCode = "SVC-SEC-FIREWALL", UnitPrice = 12000000m },
                    new PriceItem { ServiceCode = "SVC-CLOUD-BACKUP", UnitPrice = 3500000m },
                    new PriceItem { ServiceCode = "SVC-SEC-AUDIT", UnitPrice = 20000000m }
                }
            };

            var trainPriceList = new PriceList
            {
                Name = "Bảng giá Đào tạo & Chuyển giao công nghệ 2026",
                EffectiveDate = new DateOnly(2026, 4, 1),
                Items = new List<PriceItem>
                {
                    new PriceItem { ServiceCode = "SVC-TRAIN-ADMIN", UnitPrice = 6000000m },
                    new PriceItem { ServiceCode = "SVC-TRAIN-USER", UnitPrice = 4000000m }
                }
            };

            await context.PriceLists.AddRangeAsync(cloudPriceList, trainPriceList);
            await context.SaveChangesAsync();
        }
    }
}
