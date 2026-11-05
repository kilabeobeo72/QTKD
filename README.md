# PHASE 2 — XÂY DỰNG HỆ THỐNG QUẢN TRỊ KINH DOANH (QTKD)

- **GitHub Repository**: [https://github.com/kilabeobeo72/QTKD](https://github.com/kilabeobeo72/QTKD)
- **Sinh viên thực hiện**: Đào Trung Kiên
- **MSSV**: 23120055
- **Nền tảng công nghệ**: ASP.NET Core Web API (.NET 10.0), Entity Framework Core, PostgreSQL

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
├── QTKD.sln
├── README.md                                       # Tài liệu hướng dẫn cài đặt, kịch bản demo & đối chiếu quy tắc
└── QTKD.Api/                                       # Project ASP.NET Core Web API (.NET 10)
    ├── Controllers/                                # Tầng API Endpoints
    │   ├── CustomersController.cs                  # CRUD Khách hàng
    │   ├── ContractsController.cs                  # CRUD Hợp đồng & Endpoint submit duyệt (US03)
    │   └── PriceListsController.cs                 # Quản lý Bảng giá & Giá dịch vụ
    ├── Services/                                   # Tầng Xử lý nghiệp vụ & Quy tắc nghiệp vụ
    │   ├── ICustomerService.cs / CustomerService.cs
    │   ├── IContractService.cs / ContractService.cs# Cài đặt CTR-01, CTR-02, CTR-03, US03
    │   └── IPriceListService.cs / PriceListService.cs
    ├── Repositories/                               # Tầng Truy cập dữ liệu (EF Core Data Access)
    │   ├── ICustomerRepository.cs / CustomerRepository.cs
    │   ├── IContractRepository.cs / ContractRepository.cs
    │   └── IPriceListRepository.cs / PriceListRepository.cs
    ├── Models/                                     # Entities ánh xạ bảng PostgreSQL
    │   ├── Customer.cs
    │   ├── Contract.cs
    │   ├── PriceList.cs
    │   ├── PriceItem.cs
    │   └── StatusConstants.cs                      # Hằng số CustomerStatus, ContractStatus
    ├── DTOs/                                       # Data Transfer Objects
    │   ├── CustomerDto.cs
    │   ├── ContractDtos.cs
    │   ├── PriceListDto.cs
    │   └── ApiResponseDtos.cs                      # ApiErrorResponse chuẩn hóa lỗi
    ├── Data/
    │   ├── QtkdDbContext.cs                        # Cấu hình EF Core DbContext & Seed Data
    │   └── DbInitializer.cs                        # Tự động nạp dữ liệu mẫu phong phú khi khởi động
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs          # Bắt lỗi toàn cục, trả về format JSON chuẩn
    ├── Migrations/                                 # Lịch sử Migration PostgreSQL
    ├── QTKD.Api.http                               # Bộ kịch bản kiểm thử API toàn diện
    └── Program.cs                                  # Cấu hình DI, Middleware, OpenAPI/Swagger
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

```bash
# 1. Chạy ứng dụng
dotnet run --project QTKD.Api

# 2. Truy cập Swagger UI kiểm thử:
http://localhost:5079/swagger
```
