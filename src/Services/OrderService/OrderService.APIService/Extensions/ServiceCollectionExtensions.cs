using OrderService.Infrastructure.Repositories.Repository;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using OrderService.Application.Consumers;
using OrderService.Infrastructure.Data.Context;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.APIService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // Repositories
            services.AddScoped<ICartRepository>(sp =>
                new CartRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<ICartItemRepository>(sp =>
                new CartItemRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<IProductVersionCacheRepository>(sp =>
                new ProductVersionCacheRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<IProductMasterCacheRepository>(sp =>
                new ProductMasterCacheRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<ICategoryCacheRepository>(sp =>
                new CategoryCacheRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<IOrderRepository>(sp =>
                new OrderRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<IShopInfoCacheRepository>(sp =>
                new ShopInfoCacheRepository(sp.GetRequiredService<OrderDbContext>()));
            services.AddScoped<IAccountDirectoryRepository>(sp =>
                new AccountDirectoryRepository(sp.GetRequiredService<OrderDbContext>()));

            // Services
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, Application.Services.OrderService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            return services;
        }

        public static void StartEventConsumers(IServiceProvider serviceProvider)
        {
            // Start Product Event Consumer
            var productConsumer = serviceProvider.GetService<ProductEventConsumer>();
            productConsumer?.StartListening();

            var productMasterConsumer = serviceProvider.GetService<ProductMasterEventConsumer>();
            productMasterConsumer?.StartListening();

            var categoryConsumer = serviceProvider.GetService<CategoryEventConsumer>();
            categoryConsumer?.StartListening();

            // Start Shipping Fee Event Consumer
            var shippingFeeConsumer = serviceProvider.GetService<ShippingFeeEventConsumer>();
            shippingFeeConsumer?.StartListening();

            var shopConsumer = serviceProvider.GetService<ShopEventConsumer>();
            shopConsumer?.StartListening();

            var paymentPaidConsumer = serviceProvider.GetService<PaymentOrdersPaidConsumer>();
            paymentPaidConsumer?.StartListening();

            var shipmentStatusConsumer = serviceProvider.GetService<ShipmentStatusChangedConsumer>();
            shipmentStatusConsumer?.StartListening();

            var accountConsumer = serviceProvider.GetService<AccountEventConsumer>();
            accountConsumer?.StartListening();
        }
    }
}
