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
            services.AddScoped<IReviewPurchaseEligibilityRepository>(sp =>
                new ReviewPurchaseEligibilityRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IOrderDeliveredStockLedgerRepository>(sp =>
                new OrderDeliveredStockLedgerRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IImageUrlRepository>(sp =>
                new ImageUrlRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IShopRegistryRepository>(sp =>
                new ShopRegistryRepository(sp.GetRequiredService<ProductDbContext>()));
            services.AddScoped<IOrderProductMirrorRepository>(sp =>
                new OrderProductMirrorRepository(sp.GetRequiredService<ProductDbContext>()));

            // Services
            services.AddScoped<IProductMasterService, ProductMasterService>();
            services.AddScoped<IProductVersionService, ProductVersionService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductReviewService, ProductReviewService>();
            services.AddScoped<IImageUrlService, ImageUrlService>();

            // Event Publisher
            services.AddSingleton<ProductEventPublisher>();

            services.AddScoped<OrderReviewEligibilityIngestService>();
            services.AddScoped<OrderDeliveredStockIngestService>();
            services.AddScoped<OrderProductMirrorIngestService>();

            return services;
        }

        public static void StartEventConsumers(IServiceProvider serviceProvider)
        {
            var shopEventConsumer = serviceProvider.GetService<ShopEventConsumer>();
            shopEventConsumer?.StartListening();

            var orderReviewEligibilityConsumer = serviceProvider.GetService<OrderReviewEligibilityConsumer>();
            orderReviewEligibilityConsumer?.StartListening();

            var orderDeliveredStockConsumer = serviceProvider.GetService<OrderDeliveredStockConsumer>();
            orderDeliveredStockConsumer?.StartListening();

            var orderProductMirrorConsumer = serviceProvider.GetService<OrderProductMirrorConsumer>();
            orderProductMirrorConsumer?.StartListening();
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateProductMasterValidator>();

            return services;
        }
    }
}
