"# Woodify-commerceBE

## 🌲 Giới thiệu

Woodify là hệ thống thương mại điện tử chuyên về sản phẩm gỗ, được xây dựng theo kiến trúc **Microservices** với .NET 8.

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Gateway (YARP)                       │
│                         Port: 5000                              │
└─────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│ AccountService│    │  ShopService  │    │ CatalogService│
│   Port: 5010  │    │   Port: 5011  │    │   Port: 5012  │
└───────────────┘    └───────────────┘    └───────────────┘
        │                       │                       │
        └───────────────────────┼───────────────────────┘
                                ▼
                    ┌───────────────────────┐
                    │   PostgreSQL + RabbitMQ│
                    └───────────────────────┘
```

### Microservices

| Service | Port | Database | Mô tả |
|---------|------|----------|-------|
| API Gateway | 5000 | - | YARP Reverse Proxy |
| Account Service | 5010 | account_db | Quản lý tài khoản, roles |
| Shop Service | 5011 | shop_db | Quản lý cửa hàng |
| Catalog Service | 5012 | catalog_db | Quản lý danh mục |
| Product Service | 5013 | product_db | Quản lý sản phẩm |
| Certification Service | 5014 | certification_db | Chứng nhận gỗ |
| Inventory Service | 5015 | inventory_db | Quản lý kho |
| Order Service | 5016 | order_db | Quản lý đơn hàng |
| Payment Service | 5017 | payment_db | Thanh toán |
| Audit Service | 5018 | audit_db | Log tài chính |

## 📋 Yêu cầu hệ thống

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/downloads)
- IDE: Visual Studio 2022 / VS Code / Rider

## 🚀 Hướng dẫn Setup

### Bước 1: Clone Repository

```bash
git clone https://github.com/Vietnguyen2601/Woodify-commerceBE.git
cd Woodify-commerceBE
```

### Bước 2: Tạo file appsettings.json

Tạo file `appsettings.json` trong folder `src/Services/AccountService/AccountService.APIService/`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=<PORT>;Database=<DATABASE>;Username=<USERNAME>;Password=<PASSWORD>"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "<RABBITMQ_USER>",
    "Password": "<RABBITMQ_PASSWORD>"
  }
}
```

### Bước 3: Khởi động Docker Containers

```bash
# Khởi động PostgreSQL, RabbitMQ và Adminer
docker-compose up -d postgres rabbitmq adminer
```

Đợi khoảng 10-15 giây để containers khởi động hoàn tất.

### Bước 4: Chạy Service

```bash
# Di chuyển vào folder AccountService
cd src/Services/....

# Chạy service
dotnet run
```

### Bước 5: Kiểm tra

- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **Adminer (DB UI)**: http://localhost:8080
- **RabbitMQ Management**: http://localhost:15672

## 🔧 Các lệnh thường dùng

### Docker

```bash
# Khởi động infrastructure
docker-compose up -d postgres rabbitmq adminer

# Xem trạng thái containers
docker-compose ps

# Xem logs
docker-compose logs -f postgres

# Dừng containers (giữ data)
docker-compose down

# Dừng containers + xóa data (reset hoàn toàn)
docker-compose down -v
```

### Database

```bash
# Xem danh sách databases
docker exec woodify-postgres psql -U woodify -d postgres -c "\l"

# Xem tables trong db
docker exec woodify-postgres psql -U woodify -d Name-DB -c "\dt"

# Xem data
docker exec woodify-postgres psql -U woodify -d Name-DB -c "SELECT * FROM accounts;"
```

### Entity Framework Migrations

```bash
# Tạo migration mới (chạy từ folder APIService của service tương ứng)
cd src/Services/<ServiceName>/<ServiceName>.APIService
dotnet ef migrations add <TenMigration> --project ../<ServiceName>.Repositories

# Apply migrations
dotnet ef database update --project ../<ServiceName>.Repositories
```

> **Ví dụ** với AccountService:
> ```bash
> cd src/Services/AccountService/AccountService.APIService
> dotnet ef migrations add InitialCreate --project ../AccountService.Repositories
> ```

## 📁 Cấu trúc Project

```
WoodifyBE/
├── docker-compose.yml          # Cấu hình Docker
├── docker/
│   └── postgres/
│       └── init-databases.sql  # Script tạo databases
├── src/
│   ├── ApiGateway/             # YARP API Gateway
│   ├── Shared/                 # Shared library
│   └── Services/
│       └── <ServiceName>/                    # Mỗi microservice
│           ├── <ServiceName>.APIService/     # Controllers, Program.cs
│           ├── <ServiceName>.Services/       # Business logic
│           │   ├── Interfaces/               # Service interfaces
│           │   └── InternalServices/         # Service implementations
│           ├── <ServiceName>.Repositories/   # Data access
│           │   ├── Base/                     # Base repository
│           │   ├── DBContext/                # EF Core DbContext
│           │   ├── IRepositories/            # Repository interfaces
│           │   ├── Mapper/                   # Entity <-> DTO mapping
│           │   ├── Models/                   # Entities
│           │   ├── Repositories/             # Repository implementations
│           │   └── UnitOfWork/               # Unit of Work pattern
│           └── <ServiceName>.Common/         # Shared components
│               └── DTOs/                     # Data Transfer Objects
└── WoodifyBE.sln               # Solution file
```

> **Danh sách Services:** AccountService, ShopService, CatalogService, ProductService, CertificationService, InventoryService, OrderService, PaymentService, AuditService

## 🔐 Thông tin kết nối

> Liên hệ **Team Lead** để lấy credentials cho PostgreSQL, RabbitMQ và các services khác.

| Service | Host | Port |
|---------|------|------|
| PostgreSQL | localhost | 5433 |
| RabbitMQ | localhost | 5672 |
| RabbitMQ UI | localhost | 15672 |
| Adminer | localhost | 8080 |

## ❓ Troubleshooting

### Lỗi: "password authentication failed"

PostgreSQL trên máy bạn đang chiếm port 5432. Project đã config Docker dùng port **5433**.

Kiểm tra connection string có đúng port `5433` chưa:
```
Host=localhost;Port=5433;Database=<database_name>;...
```

### Lỗi: Database không có tables

Chạy migrations cho service tương ứng:
```bash
cd src/Services/<ServiceName>/<ServiceName>.APIService
dotnet ef database update --project ../<ServiceName>.Repositories
```

### Lỗi: Docker container không start

```bash
# Reset hoàn toàn
docker-compose down -v
docker-compose up -d postgres rabbitmq adminer
```

### Lỗi: Port đã được sử dụng

Kiểm tra port đang bị chiếm và tắt process:
```bash
# Windows
netstat -ano | findstr :<PORT>
taskkill /PID <PID> /F
```

## 👥 Team

- Backend Development Team

## 📄 License

Private - All rights reserved" 
