using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

namespace Gotik.Commerce.Composing;

/// <summary>
/// Composer that automatically registers Gotik Commerce services when the package is installed in Umbraco
/// </summary>
public class GotikCommerceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register DbContext
        var connectionString = builder.Config["ConnectionStrings:GotikCommerceDb"]
            ?? builder.Config["ConnectionStrings:DefaultConnection"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        // Register Unit of Work and Repositories
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Application Services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOdooService, OdooService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IHomeService, HomeService>();

        // Register Infrastructure Services
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<ICacheService, CacheService>();

        // Register HttpClient for Odoo integration
        builder.Services.AddHttpClient<IOdooService, OdooService>();

        // Register AutoMapper
        builder.Services.AddAutoMapper(typeof(GotikCommerceComposer).Assembly);

        // Add Distributed Memory Cache
        builder.Services.AddDistributedMemoryCache();

        // Add Session Support
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".GotikCommerce.Session";
        });

        // Add CORS (configurable)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("GotikCommercePolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add JWT Authentication (if configured)
        var jwtKey = builder.Config["Jwt:Key"];
        if (!string.IsNullOrEmpty(jwtKey))
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Config["Jwt:Issuer"],
                        ValidAudience = builder.Config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey))
                    };
                });
        }
    }
}
