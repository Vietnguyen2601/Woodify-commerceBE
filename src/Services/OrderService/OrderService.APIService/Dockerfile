# ================================================================
# ORDER SERVICE DOCKERFILE
# ================================================================
# Build: docker build -t woodify-order-service -f src/Services/OrderService/OrderService.APIService/Dockerfile .
# Run: docker run -p 5010:8080 woodify-order-service
# ================================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy tất cả csproj files và restore
COPY ["src/Services/OrderService/OrderService.APIService/OrderService.APIService.csproj", "src/Services/OrderService/OrderService.APIService/"]
COPY ["src/Services/OrderService/OrderService.Application/OrderService.Application.csproj", "src/Services/OrderService/OrderService.Application/"]
COPY ["src/Services/OrderService/OrderService.Domain/OrderService.Domain.csproj", "src/Services/OrderService/OrderService.Domain/"]
COPY ["src/Services/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "src/Services/OrderService/OrderService.Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/OrderService/OrderService.APIService/OrderService.APIService.csproj"

# Copy source code và build
COPY . .
WORKDIR "/src/src/Services/OrderService/OrderService.APIService"
RUN dotnet build "OrderService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrderService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.APIService.dll"]
