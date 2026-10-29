# PHASE 2 — XÂY DỰNG HỆ THỐNG QTKD

- **GitHub Repository**: [https://github.com/kilabeobeo72/QTKD](https://github.com/kilabeobeo72/QTKD)
- **Sinh viên thực hiện**: Đào Trung Kiên

---

## 1. Sơ đồ kiến trúc các thành phần chính

- **Middleware Layer (`Middleware/`)**: Xử lý ngoại lệ tập trung (`ExceptionHandlingMiddleware`), bắt toàn bộ lỗi từ Controller/Service và trả về JSON chuẩn.
- **API Layer (`Controllers/`, `DTOs/`)**: Tiếp nhận HTTP Request, trả về dữ liệu qua DTOs (`CustomersController`, `ContractsController`, `PriceListsController`).
- **Business Logic Layer (`Services/`)**: Xử lý nghiệp vụ, ánh xạ dữ liệu và kiểm tra các quy tắc (`CustomerService`, `ContractService`, `PriceListService`).
- **Data Access Layer (`Repositories/`, `Data/`, `Models/`)**: Truy vấn cơ sở dữ liệu thông qua Entity Framework Core (`QtkdDbContext`) và PostgreSQL.

### Cây thư mục

```text
QTKD/
├── QTKD.sln                                        # Solution quản lý toàn bộ dự án
├── README.md                                       # Tài liệu hướng dẫn cài đặt và chạy dự án
└── QTKD.Api/                                       # Project ASP.NET Core Web API (.NET 10)
    ├── Controllers/                                # Tầng API: Tiếp nhận HTTP Request & trả về Response
    │   ├── CustomersController.cs                  # Endpoint: GET /api/customers
    │   ├── ContractsController.cs                  # Endpoint: GET /api/contracts, GET /api/contracts/{id}
    │   └── PriceListsController.cs                 # Endpoint: GET /api/pricelists
    ├── Services/                                   # Tầng Nghiệp vụ 
    │   ├── ICustomerService.cs
    │   ├── CustomerService.cs                      # Xử lý nghiệp vụ Khách hàng
    │   ├── IContractService.cs
    │   ├── ContractService.cs                      # Xử lý nghiệp vụ Hợp đồng & kiểm tra điều kiện
    │   ├── IPriceListService.cs
    │   └── PriceListService.cs                     # Xử lý nghiệp vụ Bảng giá
    ├── Repositories/                               # Tầng Truy cập dữ liệu 
    │   ├── ICustomerRepository.cs
    │   ├── CustomerRepository.cs                   # Truy vấn bảng Customers
    │   ├── IContractRepository.cs
    │   ├── ContractRepository.cs                   # Truy vấn bảng Contracts kèm Customer
    │   ├── IPriceListRepository.cs
    │   └── PriceListRepository.cs                  # Truy vấn bảng PriceLists kèm PriceItems
    ├── Models/                                     # Tầng Thực thể ánh xạ bảng trong PostgreSQL
    │   ├── Customer.cs                             # Bảng Khách hàng
    │   ├── Contract.cs                             # Bảng Hợp đồng
    │   ├── PriceList.cs                            # Bảng Bảng giá
    │   ├── PriceItem.cs                            # Bảng Hạng mục đơn giá dịch vụ
    │   └── StatusConstants.cs                      # Hằng số trạng thái CustomerStatus & ContractStatus 
    ├── DTOs/                                       # Dữ liệu trả về cho Client
    │   ├── CustomerDto.cs
    │   ├── ContractResponseDto.cs
    │   └── PriceListDto.cs
    ├── Data/                                       # Cấu hình Entity Framework Core DbContext
    │   └── QtkdDbContext.cs                        # Khai báo DbSet, khóa chính, khóa ngoại & Seed Data
    ├── Middleware/                                 # Tầng Middleware xử lý tập trung trên HTTP Pipeline
    │   └── ExceptionHandlingMiddleware.cs          # Bắt lỗi toàn cục và chuẩn hóa JSON lỗi trả về
    ├── Migrations/                                 # Lịch sử Migration khởi tạo & cập nhật CSDL PostgreSQL
    │   ├── 20261028173207_InitialCreate.cs
    │   ├── 20261028173207_InitialCreate.Designer.cs
    │   └── QtkdDbContextModelSnapshot.cs
    ├── Properties/
    │   └── launchSettings.json                     # Cấu hình cổng chạy local (http://localhost:5079)
    ├── appsettings.json                            # Cấu hình ứng dụng & mẫu chuỗi kết nối (không chứa mật khẩu)
    ├── appsettings.Development.json
    ├── Program.cs                                  # Khởi tạo ứng dụng, đăng ký DI, DbContext, Swagger & Middleware
    ├── QTKD.Api.csproj                             # Cấu hình các gói NuGet & UserSecretsId
    └── QTKD.Api.http                               # File chứa sẵn các HTTP Request để test nhanh API
```

## 2. Yêu cầu môi trường

- **.NET SDK**: `.NET 10.0`
- **Cơ sở dữ liệu**: `PostgreSQL` 
- **Công cụ EF Core CLI**: `dotnet-ef`
  ```bash
  dotnet tool install --global dotnet-ef
  ```


## 3. Hướng dẫn cấu hình Database 

Trong file `QTKD.Api/appsettings.json`, chuỗi kết nối mẫu chỉ chứa thông tin cơ bản (không chứa mật khẩu):
```json
"ConnectionStrings": {
  "QtkdDb": "Host=localhost;Port=5432;Database=qtkd_db;Username=postgres"
}
```

Để cấu hình chuỗi kết nối đầy đủ kèm mật khẩu trên máy cá nhân bằng **User Secrets**, mở Terminal tại thư mục gốc `d:\QTKD` và chạy lệnh:

```bash
dotnet user-secrets set "ConnectionStrings:QtkdDb" "Host=localhost;Port=5432;Database=qtkd_db;Username=postgres;Password=[PASSWORD]" --project QTKD.Api
```

Kiểm tra lại cấu hình User Secrets:
```bash
dotnet user-secrets list --project QTKD.Api
```

## 4. Chạy Migration và khởi tạo dữ liệu mẫu (Seed Data)

Migration `InitialCreate` đã bao gồm cấu trúc 4 bảng (`Customers`, `Contracts`, `PriceLists`, `PriceItems`), các khóa chính (PK), khóa ngoại (FK) và dữ liệu mẫu:
- **2 Khách hàng (`Customers`)**:
  - `Id = 1`: *Công ty Minh Phát* (`Status = Active`)
  - `Id = 2`: *Công ty Cổ phần Thành Công* (`Status = Active`)
- **1 Hợp đồng (`Contracts`)**:
  - `Id = 1`: Mã `HD-2026-018` của *Công ty Minh Phát* ở trạng thái `Draft` (Bản nháp)
- **1 Bảng giá (`PriceLists`)** kèm **2 Hạng mục giá (`PriceItems`)**:
  - `SVC-ERP-CORE` (`15,000,000 VNĐ`)
  - `SVC-CLOUD-HOST` (`5,000,000 VNĐ`)

Để tạo/cập nhật database trên PostgreSQL, chạy lệnh:

```bash
dotnet ef database update --project QTKD.Api
```

*(Nếu cần tạo mới migration từ đầu: `dotnet ef migrations add InitialCreate --project QTKD.Api`)*

---

## 5. Khởi động ứng dụng Web API

```bash
dotnet run --project QTKD.Api
```

Ứng dụng sẽ khởi chạy tại địa chỉ:
- **HTTP**: `http://localhost:5079`
- **Swagger UI**: `http://localhost:5079/swagger`

---

