# ================================================================
# ACCOUNT SERVICE DOCKERFILE
# ================================================================
# Build: docker build -t woodify-account-service -f src/Services/AccountService/AccountService.APIService/Dockerfile .
# Run: docker run -p 5010:8080 woodify-account-service
# ================================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy tất cả csproj files và restore
COPY ["src/Services/AccountService/AccountService.APIService/AccountService.APIService.csproj", "src/Services/AccountService/AccountService.APIService/"]
COPY ["src/Services/AccountService/AccountService.Services/AccountService.Services.csproj", "src/Services/AccountService/AccountService.Services/"]
COPY ["src/Services/AccountService/AccountService.Common/AccountService.Common.csproj", "src/Services/AccountService/AccountService.Common/"]
COPY ["src/Services/AccountService/AccountService.Repositories/AccountService.Repositories.csproj", "src/Services/AccountService/AccountService.Repositories/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/AccountService/AccountService.APIService/AccountService.APIService.csproj"

# Copy source code và build
COPY . .
WORKDIR "/src/src/Services/AccountService/AccountService.APIService"
RUN dotnet build "AccountService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AccountService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.APIService.dll"]
