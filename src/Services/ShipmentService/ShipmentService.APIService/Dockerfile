FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5016

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/ShipmentService/ShipmentService.APIService/ShipmentService.APIService.csproj", "src/Services/ShipmentService/ShipmentService.APIService/"]
COPY ["src/Services/ShipmentService/ShipmentService.Application/ShipmentService.Application.csproj", "src/Services/ShipmentService/ShipmentService.Application/"]
COPY ["src/Services/ShipmentService/ShipmentService.Domain/ShipmentService.Domain.csproj", "src/Services/ShipmentService/ShipmentService.Domain/"]
COPY ["src/Services/ShipmentService/ShipmentService.Infrastructure/ShipmentService.Infrastructure.csproj", "src/Services/ShipmentService/ShipmentService.Infrastructure/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

RUN dotnet restore "src/Services/ShipmentService/ShipmentService.APIService/ShipmentService.APIService.csproj"

COPY . .
WORKDIR "/src/src/Services/ShipmentService/ShipmentService.APIService"
RUN dotnet build "ShipmentService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ShipmentService.APIService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShipmentService.APIService.dll"]
