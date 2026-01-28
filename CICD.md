# 🚀 CI/CD Pipeline Documentation

## Tổng quan

Woodify sử dụng **GitHub Actions** để tự động hóa quá trình build, code quality check, và Docker image verification. Pipeline được thiết kế để chạy trên mỗi PR và push đến `main` hoặc `develop` branch.

## 📋 Workflows

### 1. Build & Code Quality (`build.yml`)

**Trigger:** 
- Push đến `main` hoặc `develop`
- Pull Request đến `main` hoặc `develop`

**Jobs:**

| Job | Mô tả |
|-----|------|
| **build** | Restore dependencies và build toàn bộ solution với cấu hình Release |
| **lint** | Kiểm tra code formatting sử dụng `dotnet-format` |
| **docker-build** | Verify Docker images có thể build được |
| **summary** | Tóm tắt kết quả của tất cả jobs |

**Kết quả:**
- ✅ Build thất bại → PR không thể merge
- ✅ Build thành công → Có thể proceed đến code review

### 2. CodeQL Security Analysis (`codeql.yml`)

**Trigger:**
- Push đến `main` hoặc `develop`
- Pull Request đến `main` hoặc `develop`
- Tự động chạy hàng tuần (Chủ nhật lúc 00:00 UTC)

**Mục đích:**
- Scan code để tìm security vulnerabilities
- Detect code quality issues
- Phân tích C# code patterns

**Kết quả:**
- Reports xuất hiện trong tab "Security" của repository
- Có thể set enforcement rules để block merges

### 3. Docker Compose Validation (`docker-compose.yml`)

**Trigger:**
- Push hoặc PR thay đổi `docker-compose.yml`

**Mục đích:**
- Validate Docker Compose YAML syntax
- Kiểm tra required services (PostgreSQL, RabbitMQ, API Gateway)

## 🔄 Pipeline Flow

```
Developer Push → GitHub
    ↓
┌─────────────────────────────────┐
│   1. Build & Code Quality       │
│   ├─ .NET Build                 │
│   ├─ Code Linting               │
│   ├─ Docker Build Verification  │
│   └─ Summary                     │
└─────────────────────────────────┘
    ↓
┌─────────────────────────────────┐
│   2. CodeQL Security Analysis   │
│   ├─ Initialize CodeQL          │
│   ├─ Build Code                 │
│   └─ Analyze Security Issues    │
└─────────────────────────────────┘
    ↓
┌─────────────────────────────────┐
│   3. Docker Compose Validation  │
│   ├─ Validate YAML Syntax       │
│   └─ Check Required Services    │
└─────────────────────────────────┘
    ↓
✅ All Checks Passed → Ready for Merge
```

## 📊 Monitoring & Status

### GitHub Actions Dashboard
Xem tất cả workflow runs:
```
Repository → Actions tab → Select Workflow
```

### Branch Protection Rules (Khuyến nghị)
Để enforce CI/CD, hãy setup branch protection:

1. Go to **Settings → Branches**
2. Add rule cho `main` branch
3. Require status checks:
   - `build / Build Solution`
   - `build / Docker Build Verification`
   - `analyze / Analyze Code` (from CodeQL)

## ⚙️ Configuration

### Environment Variables
```yaml
DOTNET_VERSION: '8.0.x'
DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
DOTNET_CLI_TELEMETRY_OPTOUT: true
```

### Customize Workflows

**Thêm thêm services vào Docker build check:**
```yaml
- name: Build [ServiceName] Docker image
  uses: docker/build-push-action@v5
  with:
    context: .
    file: ./src/Services/[ServiceName]/[ServiceName].APIService/Dockerfile
    push: false
```

**Adjust branch triggers:**
```yaml
on:
  push:
    branches: [ main, develop, staging ]
```

## 🐛 Troubleshooting

### Build Failed
1. Check workflow logs: **Actions → Failed Workflow → View logs**
2. Common issues:
   - Missing .NET SDK version
   - Project file references lỗi
   - NuGet package restore failures

### CodeQL Analysis Slow
- CodeQL analysis có thể mất 5-10 phút
- Cache layer giúp speed up subsequent runs

### Docker Build Failures
- Kiểm tra Dockerfile paths
- Verify Docker Hub credentials (khi push thực sự)

## 📈 Next Steps (Khi sẵn sàng deploy)

Khi project chuẩn bị production deployment:

1. **Add Docker Registry Push**
   - Push images đến Docker Hub / GitHub Container Registry
   - Store credentials an toàn trong GitHub Secrets

2. **Add Deployment Pipeline**
   - Deploy đến staging environment
   - Run smoke tests
   - Deploy đến production (manual approval)

3. **Add Monitoring**
   - Setup Application Insights / ELK logging
   - Health check endpoints
   - Alert notifications

4. **Database Migrations**
   - Auto-run EF Core migrations
   - Backup before deploy

## 📞 Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [CodeQL Documentation](https://codeql.github.com/docs/)
- [.NET Build Best Practices](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

---

**Last Updated:** January 28, 2026
**Status:** ✅ Active & Monitoring
