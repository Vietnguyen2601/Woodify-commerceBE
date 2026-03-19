using OrderService.Infrastructure.Repositories.Repository;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using OrderService.Application.Consumers;
using OrderService.Infrastructure.Data.Context;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.PayOs;

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

            // ✨ Configure PayOS for payment integration
            services.Configure<PayOsOptions>(options =>
            {
                options.ClientId = configuration["PAYOS_CLIENT_ID"]
                    ?? configuration["PayOs:ClientId"]
                    ?? string.Empty;

                options.ApiKey = configuration["PAYOS_API_KEY"]
                    ?? configuration["PayOs:ApiKey"]
                    ?? string.Empty;

                options.ChecksumKey = configuration["PAYOS_CHECKSUM_KEY"]
                    ?? configuration["PayOs:ChecksumKey"]
                    ?? string.Empty;

                options.BaseUrl = configuration["PAYOS_BASE_URL"]
                    ?? configuration["PayOs:BaseUrl"]
                    ?? "https://api-merchant.payos.vn";
            });

            // Register HttpClient for PayOsService
            services.AddHttpClient<IPayOsService, PayOsService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }

        public static void StartEventConsumers(IServiceProvider serviceProvider)
        {
            // Start Product Event Consumer
            var productConsumer = serviceProvider.GetService<ProductEventConsumer>();
            productConsumer?.StartListening();

            // Start Shipping Fee Event Consumer
            var shippingFeeConsumer = serviceProvider.GetService<ShippingFeeEventConsumer>();
            shippingFeeConsumer?.StartListening();
        }
    }
}
