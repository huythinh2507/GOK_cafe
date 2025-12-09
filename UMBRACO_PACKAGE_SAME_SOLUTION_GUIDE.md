# Creating Umbraco Package - Same Solution Strategy

## Overview

Since your **Frontend (GOKCafe.Web)** and **Backend (GOKCafe.API + GOKCafe.Application + GOKCafe.Infrastructure)** are in the **same solution**, you have two excellent options:

### ✅ Option 1: Single Unified Package (Recommended)
Create ONE package that includes everything a user needs.

### ✅ Option 2: Keep Separate Packages
Backend package + Frontend package (already documented in main guide)

---

## Option 1: Single Unified Package (Best for Your Case!)

### Why This Works Well:

1. ✅ **Simplified Distribution**: One package to install
2. ✅ **Version Consistency**: FE and BE always in sync
3. ✅ **Easier Maintenance**: Update once, deploy once
4. ✅ **No Dependency Issues**: Everything included
5. ✅ **User-Friendly**: Install and go!

---

## Architecture: Layered Package Approach

```
GOKCafe.Umbraco (Single Package)
├── Backend Layer (from GOKCafe.Application, Infrastructure, Domain)
│   ├── Services
│   ├── Repositories
│   ├── DTOs
│   └── Database Models
│
├── API Layer (from GOKCafe.API Controllers)
│   ├── API Controllers
│   └── Middleware
│
└── Frontend Layer (from GOKCafe.Web)
    ├── Render Controllers
    ├── Razor Views
    ├── Static Assets
    └── Document Types
```

---

## Step-by-Step: Create Single Package

### Step 1: Create Package Project

```bash
# In your solution directory
cd d:\GOK_Cafe_BE\GOK_cafe

# Create new Razor Class Library
dotnet new razorclasslib -n GOKCafe.Umbraco -f net9.0

# Add to solution
dotnet sln add GOKCafe.Umbraco/GOKCafe.Umbraco.csproj
```

### Step 2: Configure Project File

Edit `GOKCafe.Umbraco/GOKCafe.Umbraco.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>

    <!-- NuGet Package Metadata -->
    <PackageId>GOKCafe.Umbraco</PackageId>
    <Version>1.0.0</Version>
    <Authors>GOK Cafe Team</Authors>
    <Company>GOK Cafe</Company>
    <Product>GOK Cafe Complete E-Commerce Solution for Umbraco</Product>
    <Description>
      Complete e-commerce solution for Umbraco including backend services, API controllers,
      product catalog with dynamic filtering, product details, cart management, and order processing.
      Includes Odoo ERP integration.
    </Description>
    <PackageTags>umbraco;ecommerce;product-catalog;cart;orders;odoo;fullstack</PackageTags>
    <PackageProjectUrl>https://github.com/huythinh2507/GOK_cafe</PackageProjectUrl>
    <RepositoryUrl>https://github.com/huythinh2507/GOK_cafe</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>

    <!-- Include content files -->
    <ContentTargetFolders>content;contentFiles</ContentTargetFolders>
    <EnableDefaultContentItems>false</EnableDefaultContentItems>
  </PropertyGroup>

  <!-- README -->
  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
    <None Include="INSTALLATION.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <!-- Umbraco Dependencies -->
  <ItemGroup>
    <PackageReference Include="Umbraco.Cms.Web.Website" Version="16.3.4" />
    <PackageReference Include="Umbraco.Cms.Web.BackOffice" Version="16.3.4" />
    <PackageReference Include="Umbraco.Commerce" Version="16.0.0" />
  </ItemGroup>

  <!-- Other Dependencies -->
  <ItemGroup>
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.15.0" />
  </ItemGroup>

  <!-- Reference your existing projects -->
  <ItemGroup>
    <ProjectReference Include="..\GOKCafe.Domain\GOKCafe.Domain.csproj" />
    <ProjectReference Include="..\GOKCafe.Application\GOKCafe.Application.csproj" />
    <ProjectReference Include="..\GOKCafe.Infrastructure\GOKCafe.Infrastructure.csproj" />
  </ItemGroup>

  <!-- Include Views -->
  <ItemGroup>
    <Content Include="Views/**/*.cshtml">
      <Pack>true</Pack>
      <PackagePath>contentFiles/any/net9.0/Views;content/Views</PackagePath>
      <BuildAction>Content</BuildAction>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Include Static Assets -->
  <ItemGroup>
    <Content Include="wwwroot/**/*.*">
      <Pack>true</Pack>
      <PackagePath>contentFiles/any/net9.0/wwwroot;content/wwwroot</PackagePath>
      <BuildAction>Content</BuildAction>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Include App_Plugins -->
  <ItemGroup>
    <Content Include="App_Plugins/**/*.*">
      <Pack>true</Pack>
      <PackagePath>contentFiles/any/net9.0/App_Plugins;content/App_Plugins</PackagePath>
      <BuildAction>Content</BuildAction>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Include uSync Document Types -->
  <ItemGroup>
    <Content Include="uSync/**/*.*">
      <Pack>true</Pack>
      <PackagePath>contentFiles/any/net9.0/uSync;content/uSync</PackagePath>
      <BuildAction>Content</BuildAction>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

### Step 3: Create Folder Structure

```
GOKCafe.Umbraco/
├── Controllers/
│   ├── Api/                           # From GOKCafe.API
│   │   ├── CartController.cs
│   │   ├── OrdersController.cs
│   │   ├── ProductsController.cs
│   │   └── CategoriesController.cs
│   └── Render/                        # From GOKCafe.Web
│       ├── HomepageController.cs
│       ├── ProductListRenderController.cs
│       ├── ProductDetailPageController.cs
│       └── CategoryRenderController.cs
│
├── Services/                          # From GOKCafe.Application
│   ├── CartService.cs
│   ├── OrderService.cs
│   ├── ProductService.cs
│   └── OdooService.cs
│
├── DTOs/                             # From GOKCafe.Application
│   ├── Product/
│   ├── Cart/
│   └── Order/
│
├── Views/                            # From GOKCafe.Web
│   ├── Partials/
│   │   ├── Homepage/
│   │   ├── Products/
│   │   ├── ProductDetail/
│   │   └── Shared/
│   ├── Homepage.cshtml
│   ├── ProductList.cshtml
│   └── ProductDetails.cshtml
│
├── wwwroot/                          # From GOKCafe.Web
│   ├── css/
│   ├── js/
│   └── images/
│
├── App_Plugins/
│   └── GOKCafe/
│       └── package.manifest
│
├── uSync/                            # Document Type Exports
│   └── v9/
│       ├── DocumentTypes/
│       ├── DataTypes/
│       └── Templates/
│
├── Composing/
│   └── GOKCafeComposer.cs           # Auto-registration
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
├── Migrations/
│   └── GOKCafeMigration.cs
│
└── README.md
```

---

## Step 4: Copy Files from Existing Projects

### A. Copy Backend Services (Application Layer)

```bash
# Copy entire Application layer
cp -r GOKCafe.Application/Services/* GOKCafe.Umbraco/Services/
cp -r GOKCafe.Application/DTOs/* GOKCafe.Umbraco/DTOs/
cp -r GOKCafe.Application/Interfaces/* GOKCafe.Umbraco/Interfaces/
```

**OR** (Better approach): Reference the existing projects
- Already done in `.csproj` with `<ProjectReference>`
- No need to copy, just reference!

### B. Copy API Controllers

```bash
# Copy API controllers
mkdir -p GOKCafe.Umbraco/Controllers/Api
cp GOKCafe.API/Controllers/*.cs GOKCafe.Umbraco/Controllers/Api/
```

Update namespaces:
```csharp
namespace GOKCafe.Umbraco.Controllers.Api;
```

### C. Copy Frontend Controllers

```bash
# Copy render controllers
mkdir -p GOKCafe.Umbraco/Controllers/Render
cp GOKCafe.Web/Controllers/HomepageController.cs GOKCafe.Umbraco/Controllers/Render/
cp GOKCafe.Web/Controllers/ProductListRenderController.cs GOKCafe.Umbraco/Controllers/Render/
cp GOKCafe.Web/Controllers/ProductDetailPageController.cs GOKCafe.Umbraco/Controllers/Render/
cp GOKCafe.Web/Controllers/CategoryRenderController.cs GOKCafe.Umbraco/Controllers/Render/
```

Update namespaces:
```csharp
namespace GOKCafe.Umbraco.Controllers.Render;
```

### D. Copy Views

```bash
# Copy all views
cp -r GOKCafe.Web/Views/Partials/* GOKCafe.Umbraco/Views/Partials/
cp GOKCafe.Web/Views/Homepage.cshtml GOKCafe.Umbraco/Views/
cp GOKCafe.Web/Views/ProductList.cshtml GOKCafe.Umbraco/Views/
cp GOKCafe.Web/Views/ProductDetails.cshtml GOKCafe.Umbraco/Views/
```

Update using statements in views if needed:
```cshtml
@using GOKCafe.Umbraco.DTOs
@using GOKCafe.Application.DTOs.Product
```

### E. Copy Static Assets

```bash
# Copy wwwroot
cp -r GOKCafe.Web/wwwroot/* GOKCafe.Umbraco/wwwroot/
```

---

## Step 5: Create Composer for Auto-Setup

Create `GOKCafe.Umbraco/Composing/GOKCafeComposer.cs`:

```csharp
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using GOKCafe.Application.Services;
using GOKCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Umbraco.Composing;

public class GOKCafeComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register DbContext
        var connectionString = builder.Config.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Application Services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOdooService, OdooService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Register Repositories
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();

        // Register HttpClient for Odoo
        builder.Services.AddHttpClient<IOdooService, OdooService>();

        // Register AutoMapper
        builder.Services.AddAutoMapper(typeof(GOKCafeComposer).Assembly);

        // Add Session Support
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }
}
```

---

## Step 6: Create Extension Method

Create `GOKCafe.Umbraco/Extensions/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GOKCafe.Application.Services;
using GOKCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Umbraco.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add GOK Cafe services to the application
    /// </summary>
    public static IServiceCollection AddGOKCafe(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOdooService, OdooService>();
        services.AddScoped<IAuthService, AuthService>();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // HttpClient
        services.AddHttpClient<IOdooService, OdooService>();

        // Session
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }
}
```

---

## Step 7: Export Document Types (uSync)

### Option A: Export from Current Site

1. Install uSync in GOKCafe.Web (if not already):
```bash
cd GOKCafe.Web
dotnet add package uSync
```

2. In Umbraco Backoffice:
   - Go to **Settings** > **uSync**
   - Click **Export All**

3. Copy exports to package:
```bash
cp -r GOKCafe.Web/uSync/v9/* GOKCafe.Umbraco/uSync/v9/
```

### Option B: Create Document Types Programmatically

Create `GOKCafe.Umbraco/Migrations/CreateDocumentTypesMigration.cs`:

```csharp
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace GOKCafe.Umbraco.Migrations;

public class CreateDocumentTypesMigration : MigrationBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;

    public CreateDocumentTypesMigration(
        IMigrationContext context,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService) : base(context)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
    }

    protected override void Migrate()
    {
        CreateHomepageDocumentType();
        CreateProductListDocumentType();
        CreateProductDetailDocumentType();
    }

    private void CreateHomepageDocumentType()
    {
        var exists = _contentTypeService.Get("homepage");
        if (exists != null) return;

        var contentType = new ContentType(_contentTypeService, -1)
        {
            Alias = "homepage",
            Name = "Homepage",
            Icon = "icon-home",
            AllowedAsRoot = true
        };

        // Add properties
        var titlePropertyType = new PropertyType(_dataTypeService.GetDataType("Textstring"), "title")
        {
            Name = "Title",
            Description = "Page title"
        };
        contentType.AddPropertyType(titlePropertyType, "Content");

        // Add allowed templates
        // contentType.AllowedTemplates = new[] { template };
        // contentType.SetDefaultTemplate(template);

        _contentTypeService.Save(contentType);
    }

    private void CreateProductListDocumentType()
    {
        // Similar implementation
    }

    private void CreateProductDetailDocumentType()
    {
        // Similar implementation
    }
}
```

---

## Step 8: Create README

Create `GOKCafe.Umbraco/README.md`:

```markdown
# GOK Cafe - Complete Umbraco E-Commerce Solution

Complete e-commerce package for Umbraco CMS including backend APIs, product catalog, cart, orders, and Odoo integration.

## Features

### Backend
- 🛒 Shopping Cart Management
- 📦 Order Processing
- 🏷️ Product Catalog with Categories
- 🔍 Advanced Product Filtering
- 🔗 Odoo ERP Integration
- 🔐 JWT Authentication
- 📊 RESTful APIs

### Frontend
- 🏠 Homepage with Featured Products
- 📋 Product List with Dynamic Filters
  - Category filtering
  - Flavour profile filtering
  - Equipment filtering
  - Stock availability filtering
  - Search functionality
  - Pagination
- 🔍 Product Detail Pages
- 🎨 Responsive Tailwind CSS Design
- ⚡ Real-time Client-Side Filtering

## Installation

### 1. Install Package

\`\`\`bash
dotnet add package GOKCafe.Umbraco
\`\`\`

### 2. Configure Database

In \`appsettings.json\`:

\`\`\`json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=.;Database=UmbracoDb;Integrated Security=true;",
    "DefaultConnection": "Server=.;Database=GOKCafeCommerce;Integrated Security=true;"
  }
}
\`\`\`

### 3. Run Migrations

\`\`\`bash
# Create commerce database
dotnet ef database update --project GOKCafe.Umbraco

# Or use Package Manager Console
Update-Database
\`\`\`

### 4. Configure API Settings

In \`appsettings.json\`:

\`\`\`json
{
  "ApiSettings": {
    "BaseUrl": "https://your-api-domain.com",
    "Timeout": 30
  },
  "Jwt": {
    "Key": "Your-Super-Secret-Key-Min-32-Characters-Long",
    "Issuer": "GOKCafe",
    "Audience": "GOKCafe",
    "ExpiryMinutes": 60
  },
  "Odoo": {
    "Url": "https://your-odoo-instance.com",
    "Database": "your-database",
    "Username": "api-user",
    "ApiKey": "your-api-key"
  }
}
\`\`\`

### 5. Import Document Types

The package automatically registers a Composer that sets up everything.

**With uSync (Recommended):**
1. Go to Umbraco Backoffice
2. Settings > uSync > Import
3. Document types will be created automatically

**Without uSync:**
Document types are created automatically on first run via migrations.

### 6. Start Using!

1. Create a Homepage node
2. Create a Product List page
3. Your products from the API will automatically appear!

## Usage

### API Endpoints

All API controllers are automatically registered:

- \`GET /api/products\` - List products with filters
- \`GET /api/products/{id}\` - Get product details
- \`GET /api/categories\` - List categories
- \`POST /api/cart/add\` - Add to cart
- \`GET /api/cart\` - Get cart
- \`POST /api/orders\` - Create order
- \`GET /api/orders\` - List orders

### Render Controllers

- \`HomepageController\` - Homepage rendering
- \`ProductListRenderController\` - Product list with filters
- \`ProductDetailPageController\` - Product details
- \`CategoryRenderController\` - Category pages

### Views

All views are included and can be customized:

- \`Views/Homepage.cshtml\`
- \`Views/ProductList.cshtml\`
- \`Views/ProductDetails.cshtml\`
- \`Views/Partials/Products/ProductsGrid.cshtml\`
- \`Views/Partials/ProductDetail/_ProductInformation.cshtml\`
- \`Views/Partials/Homepage/_ShowAllTea.cshtml\`

## Customization

### Override Views

Create a file with the same path in your project to override package views.

Example: To customize product grid, create:
\`\`\`
/Views/Partials/Products/ProductsGrid.cshtml
\`\`\`

### Override Styles

Package uses Tailwind CSS. Override in your main CSS file.

### Extend Services

All services are registered with DI. Inject and extend:

\`\`\`csharp
public class MyCustomProductService : IProductService
{
    private readonly IProductService _baseService;

    public MyCustomProductService(IProductService baseService)
    {
        _baseService = baseService;
    }

    // Override methods as needed
}
\`\`\`

## Configuration Options

### Composer (Automatic)

The package uses \`GOKCafeComposer\` which runs automatically. No configuration needed!

### Manual Setup (Optional)

If you need manual control, in \`Program.cs\`:

\`\`\`csharp
using GOKCafe.Umbraco.Extensions;

builder.Services.AddGOKCafe(builder.Configuration);
\`\`\`

## Troubleshooting

### Views Not Showing
Ensure Razor runtime compilation is enabled in development.

### API Not Working
Check connection strings and API configuration in appsettings.json.

### Database Errors
Run migrations: \`dotnet ef database update\`

### CORS Issues
Configure CORS in your startup if API is on different domain.

## Requirements

- .NET 9.0+
- Umbraco CMS 16.3.4+
- SQL Server 2016+
- Entity Framework Core 9.0+

## Support

- GitHub: https://github.com/huythinh2507/GOK_cafe
- Issues: https://github.com/huythinh2507/GOK_cafe/issues

## License

MIT
```

---

## Step 9: Build the Package

```bash
cd GOKCafe.Umbraco

# Build in Release mode
dotnet build -c Release

# Create NuGet package
dotnet pack -c Release -o ./nupkg

# Output: GOKCafe.Umbraco.1.0.0.nupkg
```

---

## Step 10: Test Locally

### Create Test Environment

```bash
# Create test Umbraco site
dotnet new umbraco -n TestGOKCafe
cd TestGOKCafe

# Add local package source
dotnet nuget add source d:\GOK_Cafe_BE\GOK_cafe\GOKCafe.Umbraco\nupkg -n LocalGOKCafe

# Install your package
dotnet add package GOKCafe.Umbraco --source LocalGOKCafe

# Run
dotnet run
```

### Verify Installation

1. **Check Files Copied:**
   - `/Views/Partials/Products/ProductsGrid.cshtml` ✅
   - `/wwwroot/css/` ✅
   - `/uSync/v9/DocumentTypes/` ✅

2. **Check Services Registered:**
   - Browse to `/api/products` - should work ✅
   - Check Umbraco logs for Composer execution ✅

3. **Check Frontend:**
   - Create Homepage node
   - Create Product List node
   - View frontend - products should display ✅

---

## Benefits of Single Package Approach

### For You (Developer):
1. ✅ Single codebase to maintain
2. ✅ Consistent versioning
3. ✅ Simpler CI/CD pipeline
4. ✅ Easier testing
5. ✅ One README to maintain

### For Users:
1. ✅ One command to install: `dotnet add package GOKCafe.Umbraco`
2. ✅ No dependency version conflicts
3. ✅ Everything works out of the box
4. ✅ Simpler configuration
5. ✅ Guaranteed FE/BE compatibility

---

## Publishing

### To NuGet.org

```bash
cd GOKCafe.Umbraco/nupkg

# Push to NuGet
dotnet nuget push GOKCafe.Umbraco.1.0.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### To GitHub Packages

```bash
# Configure GitHub source
dotnet nuget add source \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/huythinh2507/index.json"

# Push
dotnet nuget push GOKCafe.Umbraco.1.0.0.nupkg --source "github"
```

---

## Comparison: Single vs Multiple Packages

| Aspect | Single Package | Multiple Packages |
|--------|---------------|-------------------|
| **Installation** | `dotnet add package GOKCafe.Umbraco` | `dotnet add package GOKCafe.Commerce`<br>`dotnet add package GOKCafe.Frontend` |
| **Version Management** | One version | Must sync versions |
| **Dependency Hell** | None | Possible |
| **Maintenance** | Easier | More complex |
| **Flexibility** | Use backend only? ❌ | Use backend only? ✅ |
| **Size** | Larger | Smaller per package |
| **Best For** | Integrated solutions | Modular systems |

### Our Recommendation: **Single Package** ✅

Since your FE and BE are tightly coupled (FE uses BE APIs directly), a single package makes more sense!

---

## Next Steps

1. **Create the GOKCafe.Umbraco project** (Step 1)
2. **Copy controllers and views** (Steps 4-5)
3. **Create Composer** (Step 6)
4. **Export document types** (Step 7)
5. **Write README** (Step 8)
6. **Build and test locally** (Steps 9-10)
7. **Publish to NuGet** (once tested)

---

## Quick Start Script

```bash
#!/bin/bash
# Create and setup GOKCafe.Umbraco package

# 1. Create project
dotnet new razorclasslib -n GOKCafe.Umbraco -f net9.0
dotnet sln add GOKCafe.Umbraco/GOKCafe.Umbraco.csproj

# 2. Create folders
cd GOKCafe.Umbraco
mkdir -p Controllers/{Api,Render}
mkdir -p Views/Partials/{Homepage,Products,ProductDetail,Shared}
mkdir -p wwwroot/{css,js,images}
mkdir -p App_Plugins/GOKCafe
mkdir -p uSync/v9/{DocumentTypes,DataTypes,Templates}
mkdir -p Composing
mkdir -p Extensions
mkdir -p Migrations

# 3. Copy files
cp ../GOKCafe.Web/Controllers/HomepageController.cs Controllers/Render/
cp ../GOKCafe.Web/Controllers/ProductListRenderController.cs Controllers/Render/
cp ../GOKCafe.Web/Controllers/ProductDetailPageController.cs Controllers/Render/
cp -r ../GOKCafe.Web/Views/Partials/* Views/Partials/
cp -r ../GOKCafe.API/Controllers/* Controllers/Api/

# 4. Copy assets
cp -r ../GOKCafe.Web/wwwroot/* wwwroot/

# 5. Export document types (run uSync export first)
cp -r ../GOKCafe.Web/uSync/v9/* uSync/v9/

echo "Package structure created! Now:"
echo "1. Update GOKCafe.Umbraco.csproj"
echo "2. Create GOKCafeComposer.cs"
echo "3. Create README.md"
echo "4. Build: dotnet pack -c Release"
```

---

**Created**: December 2024
**For**: GOK Cafe Solution with FE+BE in same solution
**Recommended Approach**: Single Unified Package ✅
