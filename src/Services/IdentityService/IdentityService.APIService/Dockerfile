# ================================================================
# IDENTITY SERVICE DOCKERFILE
# ================================================================
# Build: docker build -t woodify-identity-service -f src/Services/IdentityService/IdentityService.APIService/Dockerfile .
# Run: docker run -p 5011:8080 woodify-identity-service
# ================================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy tất cả csproj files và restore
COPY ["src/Services/IdentityService/IdentityService.APIService/IdentityService.APIService.csproj", "src/Services/IdentityService/IdentityService.APIService/"]
COPY ["src/Services/IdentityService/IdentityService.Application/IdentityService.Application.csproj", "src/Services/IdentityService/IdentityService.Application/"]
COPY ["src/Services/IdentityService/IdentityService.Domain/IdentityService.Domain.csproj", "src/Services/IdentityService/IdentityService.Domain/"]
COPY ["src/Services/IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "src/Services/IdentityService/IdentityService.Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/IdentityService/IdentityService.APIService/IdentityService.APIService.csproj"

# Copy source code và build
COPY . .
WORKDIR "/src/src/Services/IdentityService/IdentityService.APIService"
RUN dotnet build "IdentityService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IdentityService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.APIService.dll"]
