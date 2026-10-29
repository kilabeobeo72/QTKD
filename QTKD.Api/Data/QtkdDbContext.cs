using Microsoft.EntityFrameworkCore;
using QTKD.Api.Models;

namespace QTKD.Api.Data;

public class QtkdDbContext : DbContext
{
    public QtkdDbContext(DbContextOptions<QtkdDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceItem> PriceItems => Set<PriceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);

            // Dữ liệu mẫu: 2 Customer
            entity.HasData(
                new Customer
                {
                    Id = 1,
                    Name = "Công ty Minh Phát",
                    Status = CustomerStatus.Active
                },
                new Customer
                {
                    Id = 2,
                    Name = "Công ty Cổ phần Thành Công",
                    Status = CustomerStatus.Active
                }
            );
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ContractNo).IsRequired().HasMaxLength(50);
            entity.HasIndex(c => c.ContractNo).IsUnique();
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Description).HasMaxLength(1000);

            entity.HasOne(c => c.Customer)
                  .WithMany(cu => cu.Contracts)
                  .HasForeignKey(c => c.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Dữ liệu mẫu: 1 Contract ở trạng thái Draft
            entity.HasData(
                new Contract
                {
                    Id = 1,
                    ContractNo = "HD-2026-018",
                    CustomerId = 1,
                    StartDate = new DateOnly(2026, 5, 1),
                    EndDate = new DateOnly(2027, 5, 1),
                    Status = ContractStatus.Draft,
                    Description = "Hợp đồng cung cấp giải pháp phần mềm QTKD cho Công ty Minh Phát"
                }
            );
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.HasKey(pl => pl.Id);
            entity.Property(pl => pl.Name).IsRequired().HasMaxLength(200);

            // Dữ liệu mẫu: 1 PriceList
            entity.HasData(
                new PriceList
                {
                    Id = 1,
                    Name = "Bảng giá dịch vụ chuẩn năm 2026",
                    EffectiveDate = new DateOnly(2026, 1, 1)
                }
            );
        });

        modelBuilder.Entity<PriceItem>(entity =>
        {
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.ServiceCode).IsRequired().HasMaxLength(50);
            entity.Property(pi => pi.UnitPrice).HasColumnType("numeric(18,2)");

            entity.HasOne(pi => pi.PriceList)
                  .WithMany(pl => pl.Items)
                  .HasForeignKey(pi => pi.PriceListId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Dữ liệu mẫu: 2 PriceItem
            entity.HasData(
                new PriceItem
                {
                    Id = 1,
                    PriceListId = 1,
                    ServiceCode = "SVC-ERP-CORE",
                    UnitPrice = 15000000m
                },
                new PriceItem
                {
                    Id = 2,
                    PriceListId = 1,
                    ServiceCode = "SVC-CLOUD-HOST",
                    UnitPrice = 5000000m
                }
            );
        });
    }
}
