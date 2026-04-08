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
            services.AddScoped<IOrderRepository>(sp =>
                new OrderRepository(sp.GetRequiredService<OrderDbContext>()));

            // Services
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, Application.Services.OrderService>();
            services.AddScoped<IDashboardService, DashboardService>();

            // Register event consumers
            services.AddScoped<ProductEventConsumer>();
            services.AddScoped<ImageUrlEventConsumer>();
            services.AddScoped<ShippingFeeEventConsumer>();

            // Register HttpClient for ProductServiceClient to query product data
            services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            return services;
        }

        public static void StartEventConsumers(IServiceProvider serviceProvider)
        {
            // Start Product Event Consumer
            var productConsumer = serviceProvider.GetService<ProductEventConsumer>();
            productConsumer?.StartListening();

            // Start Image URL Event Consumer
            var imageUrlConsumer = serviceProvider.GetService<ImageUrlEventConsumer>();
            imageUrlConsumer?.StartListening();

            // Start Shipping Fee Event Consumer
            var shippingFeeConsumer = serviceProvider.GetService<ShippingFeeEventConsumer>();
            shippingFeeConsumer?.StartListening();
        }
    }
}
