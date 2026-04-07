using FluentValidation;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Repositories.Repository;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Application.Consumers;
using ProductService.Application.Validators;
using ProductService.Infrastructure.Data.Context;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.APIService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProductServices(this IServiceCollection services)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IProductMasterRepository>(sp =>
                new ProductMasterRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IProductVersionRepository>(sp =>
                new ProductVersionRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<ICategoryRepository>(sp =>
                new CategoryRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IProductReviewRepository>(sp =>
                new ProductReviewRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IImageUrlRepository>(sp =>
                new ImageUrlRepository(sp.GetRequiredService<ProductDbContext>()));

            // Services
            services.AddScoped<IProductMasterService, ProductMasterService>();
            services.AddScoped<IProductVersionService, ProductVersionService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductReviewService, ProductReviewService>();
            services.AddScoped<IImageUrlService, ImageUrlService>();

            // Event Publisher
            services.AddSingleton<ProductEventPublisher>();

            // Shop name cache (populated by ShopEventConsumer via RabbitMQ)
            services.AddSingleton<ShopNameCacheService>();

            // Bestseller cache (populated by OrderCreatedConsumer via RabbitMQ)
            services.AddSingleton<BestSellerCacheService>();

            return services;
        }

        public static void StartEventConsumers(IServiceProvider serviceProvider)
        {
            var shopEventConsumer = serviceProvider.GetService<ShopEventConsumer>();
            shopEventConsumer?.StartListening();

            var orderCreatedConsumer = serviceProvider.GetService<OrderCreatedConsumer>();
            orderCreatedConsumer?.StartListening();
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateProductMasterValidator>();

            return services;
        }
    }
}
