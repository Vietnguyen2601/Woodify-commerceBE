using FluentValidation;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Repositories.IRepositories;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Application.Validators;
using IdentityService.Infrastructure.Data.Context;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.APIService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccountServices(this IServiceCollection services)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IAccountRepository>(sp =>
                new AccountRepository(sp.GetRequiredService<AccountDbContext>()));
            services.AddScoped<IRoleRepository>(sp =>
                new RoleRepository(sp.GetRequiredService<AccountDbContext>()));

            // Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<IJwtTokenService, JWTTokenService>();
            services.AddScoped<IAuthenService, AuthenService>();

            // Email Background Queue
            services.AddSingleton<IEmailBackgroundQueue, EmailBackgroundQueue>();
            services.AddHostedService<EmailBackgroundService>();

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateAccountValidator>();

            return services;
        }
    }
}
