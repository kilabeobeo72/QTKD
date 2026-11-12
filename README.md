# PHASE 2 — XÂY DỰNG HỆ THỐNG QUẢN TRỊ KINH DOANH (QTKD)

- **GitHub Repository**: [https://github.com/kilabeobeo72/QTKD](https://github.com/kilabeobeo72/QTKD)
- **Sinh viên thực hiện**: Đào Trung Kiên
- **MSSV**: 23120055
---

## 1. Sơ đồ kiến trúc giải pháp

Ứng dụng được thiết kế theo mô hình phân tầng chặt chẽ (Layered Architecture):

```text
[ Client (Swagger UI / .http / Frontend) ]
                   │
                   ▼  (JSON Request / Response)
[ 1. API Layer (Controllers & DTOs) ]
       ├── CustomersController  (/api/customers)
       ├── ContractsController  (/api/contracts)
       ├── PriceListsController (/api/pricelists)
       └── DTOs (CustomerDto, ContractDtos, PriceListDto, ApiResponseDtos)
                   │
                   ▼  (DTOs)
[ 2. Business Logic Layer (Services) ]
       ├── CustomerService      (Kiểm tra tính hợp lệ khách hàng)
       ├── ContractService      (Kiểm tra CTR-01, CTR-02, CTR-03, US03)
       └── PriceListService     (Kiểm tra đơn giá > 0, liên kết bảng giá)
                   │
                   ▼  (Domain Models / Entities)
[ 3. Data Access Layer (Repositories & EF Core) ]
       ├── Repositories         (CustomerRepository, ContractRepository, PriceListRepository)
       ├── QtkdDbContext        (DbSet, Fluent API, quan hệ 1-N, khóa chính, khóa ngoại)
       └── PostgreSQL           (Cơ sở dữ liệu quan hệ)
```

### Cấu trúc thư mục mã nguồn:
```text
QTKD/
├── QTKD.sln                                           # Solution chứa dự án API và Test
├── README.md                                          # Hướng dẫn kiến trúc, kịch bản demo & báo cáo kiểm thử
├── QTKD.Api/                                          # src/ : Project ASP.NET Core Web API (.NET 10)
│   ├── Controllers/                                   # Tầng API Endpoints (Customer, Contract, PriceList)
│   ├── Services/                                      # Tầng Xử lý nghiệp vụ (CTR-01, CTR-02, CTR-03, US03)
│   ├── Repositories/                                  # Tầng Truy cập dữ liệu (EF Core Data Access)
│   ├── Models/                                        # Entities ánh xạ bảng PostgreSQL
│   ├── DTOs/                                          # Data Transfer Objects
│   ├── Data/                                          # QtkdDbContext & Seed Data tự động
│   ├── Middleware/                                    # ExceptionHandlingMiddleware chuẩn hóa JSON lỗi UTF-8
│   ├── Migrations/                                    # Lịch sử Migration PostgreSQL
│   ├── QTKD.Api.http                                  # Kịch bản gọi API thủ công
│   └── Program.cs                                     # Cấu hình DI, Middleware, OpenAPI/Swagger
├── docs/                                              # docs/ : Minh chứng kiểm thử, tài liệu hướng dẫn
│   └── minh_chung/
│       └── TestResults.trx                            # File kết quả kiểm thử TRX nộp bài
└── QTKD.Tests/                                        # tests/ : Project xUnit Testing (.NET 10)
    ├── UnitTests/                                     # Kiểm thử nghiệp vụ Service độc lập (Isolation)
    │   ├── ContractServiceTests.cs                    # Phủ CTR-01, CTR-02, CTR-03, AC02-AC05
    │   ├── CustomerServiceTests.cs                    # Phủ nghiệp vụ Khách hàng
    │   └── PriceListServiceTests.cs                   # Phủ nghiệp vụ Bảng giá
    ├── IntegrationTests/                              # Kiểm thử tích hợp API Endpoints & Database
    │   ├── CustomWebApplicationFactory.cs             # Khởi tạo TestServer với InMemory Database riêng biệt
    │   ├── CustomerApiTests.cs                        # Tích hợp API Customer (TC01, TC02)
    │   ├── ContractApiTests.cs                        # Tích hợp API Contract (TC03, TC04, TC05, TC06, TC08)
    │   └── FullFlowIntegrationTests.cs                # Luồng tích hợp E2E xuyên suốt toàn bộ vòng đời
    └── TestResults/                                   # Thư mục sinh tự động khi chạy dotnet test
        └── TestResults.trx
```

---

## 2. Bảng đối chiếu US03 và các Quy tắc nghiệp vụ (CTR-01, CTR-02, CTR-03)

| Mã quy tắc / US | Tên quy tắc & Ý nghĩa | Đoạn mã nguồn xử lý tương ứng |
| :--- | :--- | :--- |
| **US03** | **Gửi hợp đồng trình duyệt**: Endpoint `POST /api/contracts/{id}/submit` kiểm tra hợp đồng, chuyển trạng thái sang `PendingApproval`. | [`ContractService.cs`](file:///D:/source/QTKD.Api/Services/ContractService.cs#L130-L184) (`SubmitContractAsync`) và [`ContractsController.cs`](file:///D:/source/QTKD.Api/Controllers/ContractsController.cs#L56-L64). |
| **CTR-01** | **Chỉ cho sửa hợp đồng ở `Draft` hoặc `RevisionRequested`**: Nếu hợp đồng ở `PendingApproval` hoặc trạng thái khác thì từ chối ngay. | [`ContractService.cs`](file:///D:/source/QTKD.Api/Services/ContractService.cs#L97-L103) (`UpdateContractAsync`): Kiểm tra `contract.Status != Draft && contract.Status != RevisionRequested` ném `InvalidOperationException`. |
| **CTR-02** | **Chỉ gửi khi Customer, ngày hiệu lực và nội dung bắt buộc hợp lệ**: Customer phải `Active`, ngày kết thúc `>=` ngày bắt đầu, và `Description` không được rỗng. | [`ContractService.cs`](file:///D:/source/QTKD.Api/Services/ContractService.cs#L150-L171) (`SubmitContractAsync`): Thu thập tất cả lỗi vi phạm điều kiện, từ chối gửi và giữ nguyên dữ liệu trong DB. |
| **CTR-03** | **Không cho chuyển thẳng `Draft` sang `Approved`**: Submit thành công chỉ chuyển sang `PendingApproval`. | [`ContractService.cs`](file:///D:/source/QTKD.Api/Services/ContractService.cs#L173): `contract.Status = ContractStatus.PendingApproval;`. |
| **AC01** | Người dùng không hợp lệ bị từ chối. | Xử lý qua quyền hệ thống & middleware (`403 Forbidden`). |
| **AC02 & AC05** | Hợp đồng không ở trạng thái cho phép gửi hoặc gửi lặp 2 lần khi đã `PendingApproval` sẽ bị từ chối. | [`ContractService.cs`](file:///D:/source/QTKD.Api/Services/ContractService.cs#L140-L148): Chặn gửi duyệt lặp khi trạng thái đã là `PendingApproval`. |

---

## 3. Danh mục API 

### 3.1. Customer API
- **`GET /api/customers`**: Lấy danh sách khách hàng.
- **`GET /api/customers/{id}`**: Xem chi tiết khách hàng.
- **`POST /api/customers`**: Tạo mới khách hàng.
- **`PUT /api/customers/{id}`**: Cập nhật thông tin khách hàng (`name`, `status`).
---
### 3.2. Pricing API
- **`GET /api/pricelists`**: Xem danh sách các bảng giá kèm dịch vụ.
- **`GET /api/pricelists/{id}`**: Xem chi tiết 1 bảng giá theo Id.
- **`POST /api/pricelists`**: Tạo bảng giá mới.
---
### 3.3. Contract & US03 API
- **`GET /api/contracts`**: Xem danh sách hợp đồng.
- **`GET /api/contracts/{id}`**: Xem chi tiết hợp đồng kèm thông tin khách hàng.
- **`POST /api/contracts`**: Tạo mới hợp đồng (mặc định trạng thái là `Draft`).
- **`PUT /api/contracts/{id}`**: Chỉnh sửa hợp đồng (kiểm tra quy tắc **CTR-01**).
- **`POST /api/contracts/{id}/submit`**: Gửi duyệt hợp đồng (**US03** & **CTR-02, CTR-03**).
---

## 4. Kịch bản thực hiện Demo toàn bộ luồng nộp bài

Thực hiện demo theo thứ tự: **tạo Customer → tạo Contract Draft → gửi khi thiếu dữ liệu (thất bại) → bổ sung dữ liệu → gửi lại (thành công) → kiểm tra database**:

1. **Bước 1**: Tạo Khách hàng mới qua `POST /api/customers` ➔ Nhận được `Id`.
2. **Bước 2**: Tạo Hợp đồng nháp qua `POST /api/contracts` với `description: ""` (để rỗng nội dung) ➔ Hợp đồng sinh ra ở trạng thái `Draft`.
3. **Bước 3 (Thất bại theo CTR-02 / AC03)**: Gửi trình duyệt qua `POST /api/contracts/{id}/submit` ➔ Hệ thống trả về HTTP `400 Bad Request` thông báo thiếu `Description`, trạng thái hợp đồng trong DB vẫn là `Draft`.
4. **Bước 4**: Bổ sung nội dung hợp đồng qua `PUT /api/contracts/{id}`.
5. **Bước 5 (Thành công theo CTR-02, CTR-03, AC04)**: Gọi lại `POST /api/contracts/{id}/submit` ➔ Hệ thống trả về HTTP `200 OK`, trạng thái chuyển sang `PendingApproval`.
6. **Bước 6 (Kiểm tra quy tắc bổ sung)**:
   - Thử gửi duyệt lại lần 2 (`POST /api/contracts/{id}/submit`) ➔ Bị từ chối vì đã ở `PendingApproval` (AC05).
   - Thử chỉnh sửa hợp đồng (`PUT /api/contracts/{id}`) ➔ Bị từ chối theo **CTR-01** vì chỉ được sửa ở `Draft` hoặc `RevisionRequested`.

---

## 5. Dữ liệu mẫu (Seed Data) sẵn có trong Database

Khi khởi động ứng dụng, hệ thống tự động khởi tạo dữ liệu mẫu phong phú phục vụ kiểm thử:
- **Khách hàng (`Customers`)**:
  - `Id = 1`: *Công ty Minh Phát* (`Active`)
  - `Id = 2`: *Công ty Cổ phần Thành Công* (`Active`)
  - `Id = 3`: *Tập đoàn Viễn thông An Bình* (`Active`)
  - `Id = 4`: *Công ty TNHH Logistics Toàn Cầu* (`Active`)
  - `Id = 5`: *Công ty Thương mại Dịch vụ Nam Hải* (`Inactive` - phục vụ test từ chối theo CTR-02)
- **Hợp đồng (`Contracts`)**:
  - `HD-2026-018`: Trạng thái `Draft`
  - `HD-2026-020`: Trạng thái `PendingApproval` (phục vụ test CTR-01 không cho sửa)
  - `HD-2026-021`: Trạng thái `RevisionRequested` (phục vụ test CTR-01 cho phép sửa)
  - `HD-2026-022`: Trạng thái `Draft` thiếu mô tả (phục vụ test CTR-02 gửi duyệt lỗi)
  - `HD-2026-023`: Trạng thái `Draft` của khách hàng Inactive (phục vụ test CTR-02 khách hàng không hợp lệ)
- **Bảng giá (`PriceLists`)**: 3 bảng giá chuẩn năm 2026 kèm 7 hạng mục đơn giá dịch vụ (`PriceItems`).

---

## 6. Hướng dẫn chạy dự án

### Quy trình trọn vẹn: Clone → Cấu hình → Migration → Chạy API → Chạy Test

```bash
# 1. Clone mã nguồn
git clone https://github.com/kilabeobeo72/QTKD.git
cd QTKD

# 2. Cấu hình chuỗi kết nối Database PostgreSQL (User Secrets hoặc appsettings.json)
dotnet user-secrets set "ConnectionStrings:QtkdDb" "Host=localhost;Port=5432;Database=qtkd_db;Username=postgres;Password=YOUR_PASSWORD" --project QTKD.Api

# 3. Áp dụng Migration lên Database
dotnet ef database update --project QTKD.Api

# 4. Chạy API Web Service
dotnet run --project QTKD.Api
# Truy cập Swagger UI: http://localhost:5079/swagger

# 5. Chạy toàn bộ hệ thống kiểm thử tự động (Unit Test & Integration Test)
dotnet test --logger "trx;LogFileName=TestResults.trx"
```

---

## 7. Báo cáo Kết quả Kiểm thử Hệ thống (TC01 – TC08)

### 7.1. Kiến trúc & Chiến lược Kiểm thử
Hệ thống kiểm thử được tổ chức trong project riêng `QTKD.Tests` (xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing):
- **Unit Test (`QTKD.Tests/UnitTests`)**: Kiểm thử cô lập logic nghiệp vụ Service, độc lập với cơ sở dữ liệu thật bằng cách dùng Repository Mock và EF Core InMemory DbContext. Đảm bảo mỗi quy tắc nghiệp vụ (**CTR-01**, **CTR-02**, **CTR-03**) đều có ít nhất 1 ca kiểm thử thành công và 1 ca kiểm thử thất bại.
- **Integration Test (`QTKD.Tests/IntegrationTests`)**: Kiểm thử tích hợp toàn diện thông qua `CustomWebApplicationFactory<Program>`, giả lập Web Host nội bộ và trỏ vào **InMemory Database riêng biệt** (`UseInMemoryDatabase(Guid.NewGuid())`), hoàn toàn không ảnh hưởng hay ghi đè lên database nghiệp vụ quan trọng.
- **Xác thực toàn diện (Yêu cầu 17)**: Kiểm tra đồng thời **HTTP Status Code**, **Nội dung Response Body (JSON)** và **Dữ liệu thực tế được lưu/đọc lại từ Database** (`GET /api/...` hoặc trực tiếp DbContext). Không kết luận thành công chỉ dựa trên HTTP 200.

---

### 7.2. Bảng Tổng hợp Kết quả Kiểm thử Tối thiểu (TC01 – TC08)

| Mã | Dữ liệu / Thao tác kiểm thử | Kết quả mong đợi | Kết quả | Hình thức | File mã nguồn kiểm thử |
| :---: | :--- | :--- | :---: | :---: | :--- |
| **TC01** | Tạo Customer hợp lệ qua `POST /api/customers` | Tạo thành công (HTTP 201 Created), đọc lại đúng thông tin từ DB qua `GET /api/customers/{id}` | **PASS** | Tự động | [`CustomerApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/CustomerApiTests.cs#L22-L46) & [`CustomerServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/CustomerServiceTests.cs#L27-L49) |
| **TC02** | Tạo Customer thiếu thông tin bắt buộc (`Name` để rỗng) | Bị từ chối (HTTP 400 Bad Request), trả về thông báo lỗi rõ ràng | **PASS** | Tự động | [`CustomerApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/CustomerApiTests.cs#L48-L67) & [`CustomerServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/CustomerServiceTests.cs#L51-L67) |
| **TC03** | Sửa Contract ở trạng thái `Draft` qua `PUT /api/contracts/{id}` | Thành công (HTTP 200 OK) khi dữ liệu hợp lệ; dữ liệu cập nhật lưu vào DB (**CTR-01**) | **PASS** | Tự động | [`ContractApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/ContractApiTests.cs#L25-L56) & [`ContractServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/ContractServiceTests.cs#L35-L65) |
| **TC04** | Sửa Contract ở trạng thái `PendingApproval` qua `PUT /api/contracts/{id}` | Bị từ chối (HTTP 400 Bad Request); dữ liệu trong DB giữ nguyên không bị sửa (**CTR-01**) | **PASS** | Tự động | [`ContractApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/ContractApiTests.cs#L58-L96) & [`ContractServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/ContractServiceTests.cs#L67-L92) |
| **TC05** | Submit Contract thiếu điều kiện **CTR-02** (`Description` rỗng hoặc Customer `Inactive`) qua `POST /api/contracts/{id}/submit` | Bị từ chối (HTTP 400 Bad Request); hợp đồng trong DB vẫn giữ nguyên là `Draft` | **PASS** | Tự động | [`ContractApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/ContractApiTests.cs#L102-L129) & [`ContractServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/ContractServiceTests.cs#L98-L157) |
| **TC06** | Submit Contract hợp lệ đầy đủ điều kiện qua `POST /api/contracts/{id}/submit` | Thành công (HTTP 200 OK); trạng thái chuyển sang `PendingApproval` (**CTR-02, CTR-03**) | **PASS** | Tự động | [`ContractApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/ContractApiTests.cs#L131-L161) & [`ContractServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/ContractServiceTests.cs#L159-L191) |
| **TC07** | Thử chuyển thẳng từ `Draft` sang `Approved` (bỏ qua bước duyệt) | Bị từ chối nghiêm ngặt (**CTR-03**: quy trình tuần tự bắt buộc `Draft` → `PendingApproval` → `Approved`/`Rejected`) | **PASS** | Tự động | [`ContractServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/ContractServiceTests.cs#L197-L225) |
| **TC08** | Submit lặp đối với Contract đã ở trạng thái `PendingApproval` | Bị từ chối (HTTP 400 Bad Request); không tạo thêm kết quả hay phiên xử lý trùng (**AC05**) | **PASS** | Tự động | [`ContractApiTests.cs`](file:///D:/source/QTKD.Tests/IntegrationTests/ContractApiTests.cs#L167-L192) & [`ContractServiceTests.cs`](file:///D:/source/QTKD.Tests/UnitTests/ContractServiceTests.cs#L231-L260) |

> **Tổng kết tỷ lệ tự động hóa**: **8/8 (100%)** test case được tự động hóa hoàn toàn bằng xUnit. 0 test case phải chạy thủ công.
> Tổng số bài test trong toàn bộ solution: **25 tests** (17 Unit Tests + 8 Integration Tests), **100% PASSED**.

---

### 7.3. Kiểm thử

**Console Output khi chạy `dotnet test`**:
```text
Test run for D:\source\QTKD.Tests\bin\Debug\net10.0\QTKD.Tests.dll (.NETCoreApp,Version=v10.0)
VSTest version 18.0.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Results File: D:\source\QTKD.Tests\TestResults\TestResults.trx

Passed!  - Failed:     0, Passed:    25, Skipped:     0, Total:    25, Duration: 888 ms - QTKD.Tests.dll (net10.0)
```

---