using FluentValidation;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
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

            // Services
            services.AddScoped<IProductMasterService, ProductMasterService>();
            services.AddScoped<IProductVersionService, ProductVersionService>();
            services.AddScoped<ICategoryService, CategoryService>();

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateProductMasterValidator>();

            return services;
        }
    }
}
