using GOKCafe.Application.Services;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Data;
using GOKCafe.Infrastructure.Repositories;
using GOKCafe.Infrastructure.Services;
using GOKCafe.Web.Services.Implementations;
using Gotik.Commerce.Composing;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using WebProductService = GOKCafe.Web.Services.Interfaces.IProductService;
using WebCategoryService = GOKCafe.Web.Services.Interfaces.ICategoryService;
using AppProductService = GOKCafe.Application.Services.Interfaces.IProductService;
using AppCategoryService = GOKCafe.Application.Services.Interfaces.ICategoryService;
using AppCouponService = GOKCafe.Application.Services.Interfaces.ICouponService;

namespace GOKCafe.Web.Composers
{
    public class RegisterServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Configure DbContext with SQL Server (same as GOKCafe.API)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Config.GetConnectionString("GOKCafeConnection"),
                    b => b.MigrationsAssembly("GOKCafe.Infrastructure")));

            // Register repositories
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Configure caching
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSingleton<ICacheService, CacheService>();

            // Register infrastructure services
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Register application services (direct from GOKCafe.Application)
            builder.Services.AddScoped<AppProductService, GOKCafe.Application.Services.ProductService>();
            builder.Services.AddScoped<AppCategoryService, GOKCafe.Application.Services.CategoryService>();
            builder.Services.AddScoped<AppCouponService, GOKCafe.Application.Services.CouponService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            // Skip IOdooService registration - not needed for Umbraco Web

            // Register HttpClient with base URL for API
            var apiBaseUrl = builder.Config.GetSection("ApiSettings:BaseUrl").Value ?? "https://localhost:7045";
            builder.Services.AddHttpClient<GOKCafe.Web.Services.Interfaces.IApiHttpClient, GOKCafe.Web.Services.Implementations.ApiHttpClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
#if NET8_0_OR_GREATER
                return new SocketsHttpHandler
                {
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                    }
                };
#else
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidation = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
#endif
            });

            // Register Web layer services (Umbraco-specific)
            builder.Services.AddScoped<WebProductService, GOKCafe.Web.Services.Implementations.ProductService>();
            builder.Services.AddScoped<WebCategoryService, GOKCafe.Web.Services.Implementations.CategoryService>();

            // Register HttpClient for external API calls (Odoo, etc.)
            builder.Services.AddHttpClient();

            // Add logging
            builder.Services.AddLogging();
        }
    }
}
