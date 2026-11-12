using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QTKD.Api.Data;

namespace QTKD.Tests.IntegrationTests;

/// <summary>
/// WebApplicationFactory tùy biến cho Integration Test:
/// Thay thế PostgreSQL thật bằng InMemory Database riêng biệt cho môi trường kiểm thử.
/// Dùng database kiểm thử riêng, không dùng database có dữ liệu quan trọng.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "QtkdTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            // Gỡ bỏ tất cả đăng ký liên quan đến DbContext và provider cũ
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<QtkdDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(QtkdDbContext) ||
                (d.ServiceType.Namespace != null && (
                    d.ServiceType.Namespace.StartsWith("Npgsql") ||
                    d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore")
                ))
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Đăng ký DbContext mới dùng InMemory Database riêng biệt cho test
            services.AddDbContext<QtkdDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }
}
