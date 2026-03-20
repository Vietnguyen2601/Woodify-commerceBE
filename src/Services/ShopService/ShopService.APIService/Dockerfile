FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/ShopService/ShopService.APIService/ShopService.APIService.csproj", "src/Services/ShopService/ShopService.APIService/"]
COPY ["src/Services/ShopService/ShopService.Application/ShopService.Application.csproj", "src/Services/ShopService/ShopService.Application/"]
COPY ["src/Services/ShopService/ShopService.Domain/ShopService.Domain.csproj", "src/Services/ShopService/ShopService.Domain/"]
COPY ["src/Services/ShopService/ShopService.Infrastructure/ShopService.Infrastructure.csproj", "src/Services/ShopService/ShopService.Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/ShopService/ShopService.APIService/ShopService.APIService.csproj"

COPY . .
WORKDIR "/src/src/Services/ShopService/ShopService.APIService"
RUN dotnet build "ShopService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ShopService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopService.APIService.dll"]
