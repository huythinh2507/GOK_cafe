# Gotik Commerce - Installation Guide

This guide will walk you through installing and configuring Gotik Commerce in your Umbraco project.

## Prerequisites

Before you begin, ensure you have:

- ✅ .NET 9.0 SDK or higher
- ✅ Umbraco CMS 16.3.4 or higher installed
- ✅ SQL Server 2016 or higher
- ✅ Visual Studio 2022 or VS Code (recommended)

## Step 1: Install the Package

### Via NuGet Package Manager (Visual Studio)

1. Right-click on your project in Solution Explorer
2. Select "Manage NuGet Packages"
3. Search for "Gotik.Commerce"
4. Click "Install"

### Via Package Manager Console

```powershell
Install-Package Gotik.Commerce
```

### Via .NET CLI

```bash
dotnet add package Gotik.Commerce
```

## Step 2: Configure Database

### 2.1 Update Connection Strings

Add or update the connection string in your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=.;Database=MyUmbracoSite;Integrated Security=true;TrustServerCertificate=true;",
    "GotikCommerceDb": "Server=.;Database=GotikCommerceDb;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**Options:**
- Use separate database: `GotikCommerceDb` (recommended for production)
- Use same database: Set `GotikCommerceDb` to same value as `umbracoDbDSN`

### 2.2 Run Migrations

#### Option A: Using Package Manager Console

```powershell
Update-Database
```

#### Option B: Using .NET CLI

```bash
dotnet ef database update
```

#### Option C: Automatic Migration on Startup (Not Recommended for Production)

Add to `Program.cs` (before `app.Run()`):

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

## Step 3: Configure Application Settings

Update your `appsettings.json` with all required configurations:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=.;Database=MyUmbracoSite;Integrated Security=true;TrustServerCertificate=true;",
    "GotikCommerceDb": "Server=.;Database=GotikCommerceDb;Integrated Security=true;TrustServerCertificate=true;"
  },

  "Jwt": {
    "Key": "YourSuperSecretKeyMustBeAtLeast32CharactersLong!",
    "Issuer": "GotikCommerce",
    "Audience": "GotikCommerce",
    "ExpiryMinutes": 60
  },

  "Odoo": {
    "Url": "https://your-odoo-instance.com",
    "Database": "your-database-name",
    "Username": "your-api-username",
    "ApiKey": "your-api-key"
  },

  "Caching": {
    "ProductListCacheDurationMinutes": 30
  }
}
```

### Configuration Details:

**JWT Settings** (Required for authentication):
- `Key`: Must be at least 32 characters (generate a secure random string)
- `Issuer`: Your application name
- `Audience`: Your application name
- `ExpiryMinutes`: Token expiration time (60 minutes recommended)

**Odoo Settings** (Optional - only if using Odoo integration):
- `Url`: Your Odoo instance URL
- `Database`: Odoo database name
- `Username`: Odoo API username
- `ApiKey`: Odoo API key

**Caching Settings** (Optional):
- `ProductListCacheDurationMinutes`: How long to cache product lists (default: 30)

## Step 4: Verify Installation

### 4.1 Check Auto-Registration

Gotik Commerce uses a Composer that automatically registers all services. To verify:

1. Start your application:
```bash
dotnet run
```

2. Check the console output for:
```
Gotik Commerce: Registering services...
Gotik Commerce: Services registered successfully!
```

### 4.2 Test API Endpoints

Visit these URLs to verify the API is working:

```
https://localhost:44317/api/v1/products
https://localhost:44317/api/v1/categories
https://localhost:44317/api/v1/cart
```

You should receive JSON responses (empty arrays if no data exists yet).

### 4.3 Check Backoffice

1. Log in to Umbraco backoffice
2. Navigate to **Content** section
3. You should see the Gotik Commerce dashboard

## Step 5: Optional Configurations

### 5.1 Enable CORS (for external API access)

If you need to access the API from external applications, add to `Program.cs`:

```csharp
// After services registration
var app = builder.Build();

// Add CORS before UseUmbraco
app.UseCors("GotikCommercePolicy");

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

app.Run();
```

### 5.2 Enable Sessions

Sessions are automatically configured, but ensure middleware is added in correct order in `Program.cs`:

```csharp
app.UseSession(); // Add before UseUmbraco
app.UseUmbraco()...
```

### 5.3 Manual Service Registration (Advanced)

If you need custom service registration instead of using the Composer:

```csharp
using Gotik.Commerce.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Option 1: Full package (recommended)
builder.Services.AddGotikCommerce(builder.Configuration);

// Option 2: Core services only (no Odoo)
builder.Services.AddGotikCommerceCore(builder.Configuration);

// Option 3: Odoo integration only
builder.Services.AddGotikOdooIntegration();
```

## Step 6: Create Initial Content

### 6.1 Create Homepage

1. Go to Umbraco backoffice
2. Navigate to **Content** section
3. Right-click on root → **Create** → **Gotik Homepage**
4. Fill in the required fields
5. **Save and Publish**

### 6.2 Create Product List Page

1. Right-click on Homepage → **Create** → **Gotik Product List**
2. Configure:
   - Page Title: "Products"
   - Items Per Page: 12 (recommended)
3. **Save and Publish**

### 6.3 Create Product Detail Page

1. Right-click on Product List → **Create** → **Gotik Product Detail**
2. This will be used as a template for all product details
3. **Save and Publish**

## Step 7: Import Sample Data (Optional)

### 7.1 Create Categories

Use the API or create via code:

```csharp
POST /api/v1/categories
{
  "name": "Coffee",
  "description": "Premium coffee products",
  "slug": "coffee"
}
```

### 7.2 Create Products

```csharp
POST /api/v1/products
{
  "name": "Arabica Coffee Beans",
  "description": "Premium arabica coffee beans",
  "price": 29.99,
  "categoryId": "guid-from-step-7-1",
  "stockQuantity": 100,
  "isActive": true
}
```

### 7.3 Import from Odoo (if configured)

```bash
POST /api/v1/products/odoo/sync
```

This will sync all products from your Odoo instance.

## Step 8: Test the Frontend

1. Visit your homepage: `https://localhost:44317/`
2. Navigate to Product List page
3. Verify:
   - ✅ Products display correctly
   - ✅ Filters work
   - ✅ Pagination works
   - ✅ Product detail links work
   - ✅ Add to cart works

## Troubleshooting

### Database Connection Issues

**Error**: "Cannot open database"

**Solution**:
1. Verify SQL Server is running
2. Check connection string is correct
3. Ensure database user has sufficient permissions
4. Try using `TrustServerCertificate=true` in connection string

### Migration Errors

**Error**: "Unable to create an object of type 'ApplicationDbContext'"

**Solution**:
```bash
# Specify the startup project
dotnet ef database update --startup-project YourUmbracoProject
```

### Services Not Registered

**Error**: "Unable to resolve service for type 'IProductService'"

**Solution**:
1. Verify package is installed correctly
2. Check that `GotikCommerceComposer` is being loaded (check console logs)
3. Try manual registration in `Program.cs`

### API Returns 404

**Error**: API endpoints return 404

**Solution**:
1. Ensure controllers are being discovered
2. Add to `Program.cs` before building:
```csharp
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Gotik.Commerce.Controllers.Api.ProductsController).Assembly);
```

### Views Not Found

**Error**: "The view 'ProductList' or its master was not found"

**Solution**:
1. Verify views are included in package
2. Check view file paths match document type template names
3. Ensure runtime compilation is enabled (development):
```csharp
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
```

### CORS Errors

**Error**: "Access to XMLHttpRequest blocked by CORS policy"

**Solution**:
```csharp
// In Program.cs, add before UseUmbraco
app.UseCors("GotikCommercePolicy");
```

### Session Issues

**Error**: "Session has not been configured for this application"

**Solution**:
```csharp
// Ensure session middleware is added
app.UseSession(); // Before UseUmbraco
```

## Advanced Installation

### Using with Existing Commerce Database

If you already have a commerce database:

1. Update `GotikCommerceDb` connection string to point to existing database
2. Ensure schema is compatible
3. Run migrations to apply any missing tables/columns

### Multi-Tenant Setup

For multi-tenant scenarios:

1. Create separate databases per tenant
2. Configure connection strings dynamically
3. Use separate API keys for each tenant

### Load Balanced Environments

For load-balanced setups:

1. Use distributed cache (Redis recommended):
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "your-redis-connection-string";
});
```

2. Use persistent session storage
3. Configure sticky sessions on load balancer

## Next Steps

After installation:

1. ✅ Read the [README.md](README.md) for usage examples
2. ✅ Explore the API endpoints via Swagger (if enabled)
3. ✅ Customize views to match your brand
4. ✅ Configure payment providers
5. ✅ Set up email notifications
6. ✅ Configure shipping methods

## Getting Help

- **Documentation**: [README.md](README.md)
- **GitHub Issues**: https://github.com/huythinh2507/GOK_cafe/issues
- **Email**: support@gotikcommerce.com (if available)

## Version Compatibility

| Gotik Commerce | Umbraco CMS | .NET | EF Core |
|----------------|-------------|------|---------|
| 1.0.x          | 16.3.4+     | 9.0+ | 9.0+    |

---

**Installation successful?** Star us on [GitHub](https://github.com/huythinh2507/GOK_cafe)! ⭐
