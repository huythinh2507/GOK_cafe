using GOKCafe.Application.Services;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Data;
using GOKCafe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GOKCafe.Commerce.Extensions;

/// <summary>
/// Extension methods for registering GOK Cafe Commerce services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all GOK Cafe Commerce services including Cart, Order, Product, and Odoo integration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGOKCafeCommerce(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("GOKCafe.Infrastructure")));

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Commerce Services
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOdooService, OdooService>();

        // Register HttpClient for Odoo integration
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Registers only Cart and Order services (minimal e-commerce setup)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGOKCafeCommerceCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("GOKCafe.Infrastructure")));

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Core Commerce Services
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }

    /// <summary>
    /// Registers Odoo integration services only
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGOKCafeOdooIntegration(
        this IServiceCollection services)
    {
        services.AddScoped<IOdooService, OdooService>();
        services.AddHttpClient();

        return services;
    }
}
