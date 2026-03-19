using FluentValidation;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Services;
using ShipmentService.Application.Validators;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.ExternalProviders;
using ShipmentService.Infrastructure.Persistence;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using ShipmentService.Infrastructure.Repositories.Repository;
using ShipmentService.Infrastructure.Services;

namespace ShipmentService.APIService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShipmentServices(this IServiceCollection services)
    {
        // Order Info Cache (from RabbitMQ events)
        services.AddSingleton<IOrderInfoCacheRepository, OrderInfoCacheRepository>();

        // Shop Info Cache (from RabbitMQ events)
        services.AddSingleton<IShopInfoCacheRepository, ShopInfoCacheRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IShipmentRepository>(sp =>
            new ShipmentRepository(sp.GetRequiredService<ShipmentDbContext>()));
        services.AddScoped<IShippingProviderRepository>(sp =>
            new ShippingProviderRepository(sp.GetRequiredService<ShipmentDbContext>()));
        services.AddScoped<IProviderServiceRepository>(sp =>
            new ProviderServiceRepository(sp.GetRequiredService<ShipmentDbContext>()));

        // Application Services
        services.AddScoped<IShipmentService, ShipmentAppService>();
        services.AddScoped<IShippingProviderService, ShippingProviderAppService>();
        services.AddScoped<IProviderServiceService, ProviderServiceAppService>();
        services.AddScoped<IShippingFeePreviewService, ShippingFeePreviewAppService>();

        return services;
    }

    public static IServiceCollection AddShippingFeeCalculator(
        this IServiceCollection services)
    {
        // Register mock shipping fee calculator as Singleton (stateless, safe for singleton consumers like OrderEventConsumer)
        // MockShippingFeeCalculator is thread-safe and doesn't maintain mutable state
        services.AddSingleton<IShippingFeeCalculator, MockShippingFeeCalculator>();

        return services;
    }

    public static IServiceCollection AddExternalServiceClients(
        this IServiceCollection services, IConfiguration config)
    {
        // DOTNET_RUNNING_IN_CONTAINER=true is set automatically by official .NET Docker images
        bool isDocker = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true", StringComparison.OrdinalIgnoreCase);

        var orderServiceUrl = Environment.GetEnvironmentVariable("OrderService_Url")
                              ?? config["ExternalServices:OrderServiceUrl"]
                              ?? (isDocker ? "http://woodify-order-service:5014" : "http://localhost:5014");

        var productServiceUrl = Environment.GetEnvironmentVariable("ProductService_Url")
                                ?? config["ExternalServices:ProductServiceUrl"]
                                ?? (isDocker ? "http://woodify-product-service:5012" : "http://localhost:5012");

        var shopServiceUrl = Environment.GetEnvironmentVariable("ShopService_Url")
                             ?? config["ExternalServices:ShopServiceUrl"]
                             ?? (isDocker ? "http://woodify-shop-service:5011" : "http://localhost:5011");

        services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
        {
            client.BaseAddress = new Uri(orderServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
        {
            client.BaseAddress = new Uri(productServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
        {
            client.BaseAddress = new Uri(shopServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateShipmentValidator>();
        return services;
    }
}
