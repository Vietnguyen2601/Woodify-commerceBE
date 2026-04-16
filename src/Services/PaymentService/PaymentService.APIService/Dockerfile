# ================================================================
# PAYMENT SERVICE DOCKERFILE
# ================================================================
# Build: docker build -t woodify-payment-service -f src/Services/PaymentService/PaymentService.APIService/Dockerfile .
# Run: docker run -p 5015:8080 woodify-payment-service
# ================================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy tất cả csproj files và restore
COPY ["src/Services/PaymentService/PaymentService.APIService/PaymentService.APIService.csproj", "src/Services/PaymentService/PaymentService.APIService/"]
COPY ["src/Services/PaymentService/PaymentService.Application/PaymentService.Application.csproj", "src/Services/PaymentService/PaymentService.Application/"]
COPY ["src/Services/PaymentService/PaymentService.Domain/PaymentService.Domain.csproj", "src/Services/PaymentService/PaymentService.Domain/"]
COPY ["src/Services/PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "src/Services/PaymentService/PaymentService.Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/PaymentService/PaymentService.APIService/PaymentService.APIService.csproj"

# Copy source code và build
COPY . .
WORKDIR "/src/src/Services/PaymentService/PaymentService.APIService"
RUN dotnet build "PaymentService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.APIService.dll"]
