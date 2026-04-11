using FluentValidation;
using ShopService.Infrastructure.Repositories;
using ShopService.Infrastructure.Repositories.IRepositories;
using ShopService.Infrastructure.UnitOfWork;
using ShopService.Application.Interfaces;
using ShopService.Application.Services;
using ShopService.Application.Validators;
using ShopService.Infrastructure.Data.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;

namespace ShopService.APIService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShopServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IShopRepository, ShopRepository>();
            
            // Dashboard Repository
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            // Services
            services.AddScoped<IShopService>(sp =>
            {
                var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
                var publisher = sp.GetService<RabbitMQPublisher>();
                return new ShopService.Application.Services.ShopService(unitOfWork, publisher);
            });
            
            // Dashboard Service
            services.AddScoped<IDashboardService, DashboardService>();

            // Redis Cache for Dashboard metrics
            services.AddStackExchangeRedisCache(options =>
            {
                var redisConnectionString = configuration.GetConnectionString("Redis") 
                    ?? Environment.GetEnvironmentVariable("Redis_ConnectionString") 
                    ?? "localhost:6379";
                options.Configuration = redisConnectionString;
            });

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateShopValidator>();

            return services;
        }
    }
}
