# ================================================================
# PRODUCT SERVICE DOCKERFILE
# ================================================================
# Build: docker build -t woodify-product-service -f src/Services/ProductService/ProductService.APIService/Dockerfile .
# Run: docker run -p 5012:8080 woodify-product-service
# ================================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/ProductService/ProductService.APIService/ProductService.APIService.csproj", "src/Services/ProductService/ProductService.APIService/"]
COPY ["src/Services/ProductService/ProductService.Application/ProductService.Application.csproj", "src/Services/ProductService/ProductService.Application/"]
COPY ["src/Services/ProductService/ProductService.Domain/ProductService.Domain.csproj", "src/Services/ProductService/ProductService.Domain/"]
COPY ["src/Services/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "src/Services/ProductService/ProductService.Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/ProductService/ProductService.APIService/ProductService.APIService.csproj"

COPY . .
WORKDIR "/src/src/Services/ProductService/ProductService.APIService"
RUN dotnet build "ProductService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ProductService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.APIService.dll"]
