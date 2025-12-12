using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GOKCafe.Application.Services;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Infrastructure.Data;
using GOKCafe.Infrastructure.Repositories;
using GOKCafe.Infrastructure.Services;
using GOKCafe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gotik.Commerce.Extensions;

/// <summary>
/// Extension methods for registering Gotik Commerce services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Gotik Commerce services including Cart, Order, Product, and Odoo integration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGotikCommerce(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("GotikCommerceDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Application Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOdooService, OdooService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHomeService, HomeService>();

        // Register Infrastructure Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICacheService, CacheService>();

        // Register HttpClient for Odoo integration
        services.AddHttpClient<IOdooService, OdooService>();

        // Register AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        // Add Distributed Memory Cache
        services.AddDistributedMemoryCache();

        // Add Session Support
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".GotikCommerce.Session";
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("GotikCommercePolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add JWT Authentication (if configured)
        var jwtKey = configuration["Jwt:Key"];
        if (!string.IsNullOrEmpty(jwtKey))
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey))
                    };
                });
        }

        return services;
    }

    /// <summary>
    /// Registers only core commerce services (Cart, Order, Product) without Odoo integration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGotikCommerceCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("GotikCommerceDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Core Commerce Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAuthService, AuthService>();

        // Register Infrastructure Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICacheService, CacheService>();

        // Add Session Support
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }

    /// <summary>
    /// Registers Odoo integration services only
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGotikOdooIntegration(
        this IServiceCollection services)
    {
        services.AddScoped<IOdooService, OdooService>();
        services.AddHttpClient<IOdooService, OdooService>();

        return services;
    }
}
