# Redis Caching Implementation - ProductService

## Tổng quan

Redis caching đã được tích hợp vào ProductService để tăng hiệu suất cho các API GetAll. Khi người dùng truy vấn dữ liệu lần đầu, hệ thống sẽ lấy từ database và lưu vào Redis cache. Các lần truy vấn sau sẽ lấy trực tiếp từ cache, giảm tải cho database và tăng tốc độ phản hồi.

## Các API đã được cache

### 1. GetAllProducts
- **Endpoint**: `GET /api/ProductMasters/GetAllProducts`
- **Cache Key**: `all_products`
- **Expiration**: 30 phút
- **Description**: Cache toàn bộ danh sách products

### 2. GetAllCategories
- **Endpoint**: `GET /api/Categories/GetAllCategories`
- **Cache Key**: `all_categories`
- **Expiration**: 30 phút
- **Description**: Cache toàn bộ danh sách categories

## Cấu hình Redis

### appsettings.json
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "ProductService_"
  },
  "CacheSettings": {
    "DefaultExpirationMinutes": 30
  }
}
```

### Environment Variables (Docker)
```bash
Redis_ConnectionString=redis:6379
```

## Cache Invalidation Strategy

Cache sẽ tự động được xóa (invalidate) khi có các thao tác thay đổi dữ liệu:

### Products Cache Invalidation
Cache `all_products` sẽ bị xóa khi:
- ✅ Tạo product mới (`CreateProduct`)
- ✅ Cập nhật product (`UpdateProduct`)
- ✅ Xóa product (`DeleteProduct`)
- ✅ Archive product (`ArchiveProduct`)
- ✅ Publish product (`PublishProduct`)

### Categories Cache Invalidation
Cache `all_categories` sẽ bị xóa khi:
- ✅ Tạo category mới (`CreateCategory`)
- ✅ Cập nhật category (`UpdateCategory`)
- ✅ Xóa category (`DeleteCategory`)

## Cách thức hoạt động

### Flow đọc dữ liệu (Cache Hit)
```
1. Client gọi API GetAll
2. Service kiểm tra Redis cache
3. Nếu có data trong cache → Trả về ngay lập tức
4. Response time: ~10-50ms
```

### Flow đọc dữ liệu (Cache Miss)
```
1. Client gọi API GetAll
2. Service kiểm tra Redis cache
3. Không có data trong cache
4. Query từ PostgreSQL database
5. Lưu kết quả vào Redis cache với TTL 30 phút
6. Trả về data cho client
7. Response time: ~100-500ms (lần đầu)
```

### Flow ghi dữ liệu (Cache Invalidation)
```
1. Client gọi API Create/Update/Delete
2. Service thực hiện thao tác trên database
3. Service xóa cache key liên quan
4. Lần đọc tiếp theo sẽ cache miss và rebuild cache
```

## Chạy với Docker Compose

```bash
# Start tất cả services bao gồm Redis
docker-compose up -d

# Kiểm tra logs của ProductService
docker-compose logs -f product-service

# Kiểm tra Redis đang chạy
docker-compose ps redis

# Kết nối vào Redis CLI để kiểm tra
docker exec -it woodify-redis redis-cli

# Trong Redis CLI:
# Xem tất cả keys
> KEYS *

# Xem giá trị của một key
> GET ProductService_all_products

# Xem TTL của key
> TTL ProductService_all_products

# Xóa một key thủ công
> DEL ProductService_all_products
```

## Testing Cache

### 1. Test Cache Hit
```bash
# Lần 1 - Cache Miss (slow)
curl -X GET "http://localhost:5012/api/ProductMasters/GetAllProducts"
# Thời gian phản hồi: ~200-500ms

# Lần 2 - Cache Hit (fast)
curl -X GET "http://localhost:5012/api/ProductMasters/GetAllProducts"
# Thời gian phản hồi: ~10-50ms
```

### 2. Test Cache Invalidation
```bash
# 1. Gọi GetAll để cache data
curl -X GET "http://localhost:5012/api/ProductMasters/GetAllProducts"

# 2. Kiểm tra cache tồn tại
docker exec -it woodify-redis redis-cli KEYS "*products*"

# 3. Tạo product mới
curl -X POST "http://localhost:5012/api/ProductMasters/CreateProduct" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product", "categoryId":"..."}'

# 4. Kiểm tra cache đã bị xóa
docker exec -it woodify-redis redis-cli KEYS "*products*"
# Kết quả: empty (cache đã bị xóa)

# 5. Gọi GetAll lại sẽ rebuild cache
curl -X GET "http://localhost:5012/api/ProductMasters/GetAllProducts"
```

## Performance Improvements

### Trước khi có Redis Cache
- **Response Time**: 200-500ms
- **Database Load**: High (mỗi request đều query database)
- **Scalability**: Limited by database capacity

### Sau khi có Redis Cache
- **Response Time**: 10-50ms (khi cache hit)
- **Database Load**: Low (chỉ query khi cache miss)
- **Scalability**: Cao hơn nhiều
- **Cache Hit Ratio**: ~90-95% (typical)

## Best Practices

### ✅ DO
- Sử dụng cache cho data ít thay đổi và được đọc nhiều
- Set TTL phù hợp (không quá dài, không quá ngắn)
- Invalidate cache khi data thay đổi
- Monitor cache hit ratio

### ❌ DON'T
- Cache data thay đổi liên tục
- Cache data nhạy cảm không mã hóa
- Set TTL quá dài (stale data)
- Quên invalidate cache khi update

## Monitoring & Troubleshooting

### Kiểm tra Redis đang hoạt động
```bash
docker exec -it woodify-redis redis-cli ping
# Kết quả: PONG
```

### Xem memory usage
```bash
docker exec -it woodify-redis redis-cli INFO memory
```

### Clear toàn bộ cache (dev only)
```bash
docker exec -it woodify-redis redis-cli FLUSHALL
```

### Logs
```bash
# ProductService logs
docker-compose logs -f product-service

# Redis logs
docker-compose logs -f redis
```

## Mở rộng trong tương lai

### 1. Cache cho các APIs khác
- GetByShopId (cache per shop)
- GetRootCategories
- GetActiveCategories

### 2. Distributed Caching
- Redis Cluster cho high availability
- Redis Sentinel cho failover tự động

### 3. Cache Strategies
- Cache aside (hiện tại)
- Write through
- Write behind

### 4. Advanced Features
- Cache stampede prevention
- Cache warming
- Selective cache invalidation
- Cache compression

## Liên hệ

Nếu có vấn đề hoặc câu hỏi về Redis caching, vui lòng tạo issue trên repository.
