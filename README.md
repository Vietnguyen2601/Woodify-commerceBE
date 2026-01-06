## 🌲 Giới thiệu

Woodify là hệ thống thương mại điện tử chuyên về sản phẩm gỗ, được xây dựng theo kiến trúc **Microservices** với .NET 8.

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Gateway (YARP)                       │
│                         Port: ...                               │
└─────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────┬───────────┼───────────┬───────────┬──────────┐
        ▼           ▼           ▼           ▼           ▼          ▼
┌──────────────┐ ┌──────────┐ ┌─────────┐ ┌─────────┐ ┌────────┐ ┌─────────┐
│   Identity   │ │  Shop    │ │Product  │ │Inventory│ │ Order  │ │Payment  │
│   Service    │ │ Service  │ │Service  │ │Service  │ │Service │ │Service  │
│  Port: ...   │ │Port: ... │ │Port:... │ │Port:... │ │Port:...│ │Port:... │
└──────────────┘ └──────────┘ └─────────┘ └─────────┘ └────────┘ └─────────┘
        │           │           │           │           │          │
        └───────────┴───────────┴───────────┴───────────┴──────────┴──────
                                │
                    ┌───────────────────────┐
                    │   PostgreSQL + RabbitMQ│
                    └───────────────────────┘
```

### Microservices

| Service | Port | Database | Domain |
|---------|------|----------|--------|
| API Gateway | ... | - | YARP Reverse Proxy |
| Identity Service | ... | identity_db | User, Account |
| Shop Service | ... | shop_db | Shop, Follow |
| Product Service | ... | product_db | Product, Certificate |
| Inventory Service | ... | inventory_db | Stock |
| Order Service | ... | order_db | Cart, Order |
| Payment Service | ... | payment_db | Payment, Wallet |

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
    "Port": "<RABBITMQ_PORT>",
    "Username": "<RABBITMQ_USER>",
    "Password": "<RABBITMQ_PASSWORD>"
  }
}
```

### Bước 3: Khởi động Docker Infrastructure

```bash
# Khởi động PostgreSQL, RabbitMQ và Adminer
docker-compose up -d
```

Đợi khoảng 10-15 giây để containers khởi động hoàn tất.

### Bước 4: Chạy Services (Tự động - Recommended)

Chúng tôi cung cấp PowerShell Script để quản lý tất cả services một cách dễ dàng:

```powershell
# Từ root folder, chạy script
.\src\ApiGateway\start-services.ps1
```

**Script sẽ:**
1. Hiển thị menu chọn services cần chạy
2. Tự động chạy `dotnet run` cho từng service trong terminal riêng
3. Hiển thị thông tin port và URLs

**Ví dụ:**
```
========================================
  WOODIFY SERVICES LAUNCHER
========================================
[Chọn services cần chạy]

1. Identity Service
2. Shop Service
3. Product Service
...
0. Start Selected Services

Chọn (0-9): 1
[Chọn] 2
[Chọn] 0

=> Khởi động Identity Service và Shop Service
```

### Bước 4 (Alternative): Chạy Service Thủ Công

Nếu bạn muốn chạy từng service một cách thủ công:

```bash
# Di chuyển vào folder service
cd src/Services/IdentityService/IdentityService.APIService

# Chạy service
dotnet run
```

### Bước 5: Kiểm tra


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

## 🐰 RabbitMQ - Message Broker

### Giới thiệu

RabbitMQ là **Message Broker** dùng để giao tiếp **bất đồng bộ** giữa các microservices. Thay vì gọi API trực tiếp (HTTP), services gửi **message/event** qua RabbitMQ.

### Kiến trúc

```
┌─────────────────┐                              ┌─────────────────┐
│   ShopService   │                              │ AccountService  │
│   (Publisher)   │                              │   (Consumer)    │
└────────┬────────┘                              └────────▲────────┘
         │                                                │
         │ 1. Publish "shop.created"                      │ 3. Subscribe & Process
         │    event sau khi tạo Shop                      │
         ▼                                                │
┌─────────────────────────────────────────────────────────┴────────┐
│                         RabbitMQ                                 │
│                    Queue: "shop.created"                         │
│                                                                  │
│    ┌─────────────────────────────────────────────────────────┐   │
│    │ Message: { ShopId, ShopName, OwnerId, CreatedAt }       │   │
│    └─────────────────────────────────────────────────────────┘   │
│                                                                  │
│                     2. Message chờ trong Queue                   │
└──────────────────────────────────────────────────────────────────┘
```

### Khi nào dùng RabbitMQ vs HTTP?

| Use Case | Phương án | Ví dụ |
|----------|-----------|-------|
| Cần data ngay + chính xác | **HTTP Call** | Lấy thông tin Owner khi xem Shop |
| Thông báo sự kiện (fire & forget) | **RabbitMQ** | Shop được tạo → gửi email |
| Side effects không cần response | **RabbitMQ** | Order đặt → trừ kho, ghi log |

### Cấu trúc code

```
src/
├── Shared/
│   ├── Events/
│   │   └── DomainEvents.cs           # Định nghĩa các Event classes
│   └── Messaging/
│       ├── RabbitMQPublisher.cs      # Gửi message
│       ├── RabbitMQConsumer.cs       # Nhận message
│       └── RabbitMQSettings.cs       # Config connection

