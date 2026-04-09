using FluentValidation;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Services;
using ShipmentService.Application.Validators;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Persistence;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using ShipmentService.Infrastructure.Repositories.Repository;

namespace ShipmentService.APIService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShipmentServices(this IServiceCollection services)
    {
        services.AddSingleton<IOrderInfoCacheRepository, OrderInfoCacheRepository>();
        services.AddScoped<IShopInfoCacheRepository, ShopInfoCacheEfRepository>();

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
        services.AddSingleton<IShippingFeeCalculator, ShippingFeeCalculator>();

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateShipmentValidator>();
        return services;
    }
}
