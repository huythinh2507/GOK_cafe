using System.Text;
using GOKCafe.Application.Services;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Data;
using GOKCafe.Infrastructure.Repositories;
using GOKCafe.Infrastructure.Services;
using GOKCafe.Infrastructure.Services.Interfaces;
using GOKCafe.Web.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Umbraco.Commerce.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all network interfaces (for mobile access)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(25718); // HTTP
    serverOptions.ListenAnyIP(44317, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

// Allow all hosts for development
builder.Services.Configure<Microsoft.AspNetCore.HostFiltering.HostFilteringOptions>(options =>
{
    options.AllowedHosts.Clear();
});

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .AddUmbracoCommerce()
    .Build();

// ===== API Services Configuration =====
// Note: Do not re-register distributed cache as Umbraco already registers it

// Configure DbContext with SQL Server for API
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("GOKCafe.Infrastructure")));

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure cache service
builder.Services.AddSingleton<ICacheService, CacheService>();

// Register infrastructure services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
builder.Services.AddHttpClient<IEmailService, ResendEmailService>();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOdooService, OdooService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ILoyaltyPlatformService, LoyaltyPlatformService>();
builder.Services.AddScoped<IProductCommentService, ProductCommentService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IBlogCommentService, BlogCommentService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
// Register HttpClient for external API calls
builder.Services.AddHttpClient();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

if (!string.IsNullOrEmpty(secretKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
}

// Configure CORS for API endpoints
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GOK Cafe API",
        Version = "v1",
        Description = "API for GOK Cafe management system"
    });

    // Include only GOKCafe API controllers from the API assembly
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Only include controllers from GOKCafe.API assembly
        var controllerType = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        return controllerType != null &&
               controllerType.ControllerTypeInfo.Assembly.GetName().Name == "GOKCafe.API";
    });

    // Use fully qualified names for generic types to avoid schema ID conflicts
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    // Include XML comments if available
    var apiXmlFile = "GOKCafe.API.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
    if (File.Exists(apiXmlPath))
    {
        options.IncludeXmlComments(apiXmlPath);
    }
});

// Add API Controllers support (for Web API endpoints like ProductApiController)
// Include controllers from GOKCafe.API and GOKCafe.Web assemblies, exclude Gotik.Commerce
builder.Services.AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        // Remove Gotik.Commerce assembly to prevent duplicate API controllers
        var gotikCommercePart = manager.ApplicationParts
            .OfType<Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart>()
            .FirstOrDefault(p => p.Assembly.GetName().Name == "Gotik.Commerce");

        if (gotikCommercePart != null)
        {
            manager.ApplicationParts.Remove(gotikCommercePart);
        }

        // Ensure GOKCafe.API assembly is added
        if (!manager.ApplicationParts.Any(p => p is Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart ap && ap.Assembly.GetName().Name == "GOKCafe.API"))
        {
            manager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(
                typeof(GOKCafe.API.Controllers.AuthController).Assembly));
        }
    })
    .AddControllersAsServices();

// Add Response Caching
builder.Services.AddResponseCaching();

WebApplication app = builder.Build();

// Seed API database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbSeeder.SeedAsync(context);

        // Check if --seed-prices argument is provided
        if (args.Contains("--seed-prices"))
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting product price seeding...");
            await PriceSeeder.SeedProductPricesAsync(context);
            logger.LogInformation("Product price seeding completed.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the API database.");
    }
}

await app.BootUmbracoAsync();


// CRITICAL: Response Caching must come BEFORE Umbraco
app.UseResponseCaching();

// Enable Swagger in all environments (you can restrict to Development if needed)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GOK Cafe API V1");
    options.RoutePrefix = "api/swagger"; // Access Swagger at /api/swagger
});

// Enable CORS for API endpoints
app.UseCors("AllowAll");

// Enable Authentication and Authorization for API
app.UseAuthentication();
app.UseAuthorization();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

// Map API controllers
app.MapControllers();

// Display mobile access URLs
app.Lifetime.ApplicationStarted.Register(MobileAccessHelper.DisplayMobileAccessInfo);

await app.RunAsync();
