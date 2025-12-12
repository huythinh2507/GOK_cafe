# Gotik Commerce Package - Complete Build Plan

## Overview

This document outlines the complete step-by-step plan to build "Gotik Commerce" - an Umbraco Commerce package similar to Umbraco Commerce in the marketplace.

**Package Name**: Gotik Commerce
**Target Platform**: Umbraco CMS 16.3.4+
**Framework**: .NET 9.0
**Package Type**: Single Unified Package (Recommended)

---

## Phase 1: Preparation & Planning

### 1. Choose Your Package Strategy

**✅ Recommended: Single Unified Package (Gotik.Commerce)**
- Easier to maintain
- One-step installation for users
- Guaranteed FE/BE compatibility
- Consistent versioning

**Alternative: Separate Packages**
- Gotik.Commerce.Core (Backend)
- Gotik.Commerce.Frontend (UI)
- More modular but complex to manage

**Decision**: Single unified package approach

### 2. Define Package Scope

Your package will include:
- ✅ Product catalog with dynamic filtering
- ✅ Shopping cart (session-based + authenticated)
- ✅ Order management system
- ✅ Odoo ERP integration
- ✅ Category management
- ✅ Frontend views (product list, details, cart)
- ✅ API controllers
- ✅ Authentication system (JWT)
- ✅ User management
- ✅ Stock management with reservation
- ✅ Product attributes (FlavourProfile, Equipment)
- ✅ Payment tracking
- ✅ Order status workflow

---

## Phase 2: Create Package Structure

### 3. Create the Package Project

```bash
cd d:\GOK_Cafe_BE\GOK_cafe
dotnet new razorclasslib -n Gotik.Commerce -f net9.0
dotnet sln add Gotik.Commerce/Gotik.Commerce.csproj
```

**Deliverable**: New Gotik.Commerce project in solution

### 4. Set Up Project File

Edit `Gotik.Commerce/Gotik.Commerce.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>

    <!-- NuGet Package Metadata -->
    <PackageId>Gotik.Commerce</PackageId>
    <Version>1.0.0</Version>
    <Authors>GOK Cafe Team</Authors>
    <Company>GOK Cafe</Company>
    <Product>Gotik Commerce - Complete E-Commerce Solution for Umbraco</Product>
    <Description>
      Complete e-commerce solution for Umbraco CMS including backend services, API controllers,
      product catalog with dynamic filtering, shopping cart, order management, and Odoo ERP integration.
      Built with clean architecture principles and enterprise-grade features.
    </Description>
    <PackageTags>umbraco;ecommerce;product-catalog;cart;orders;odoo;commerce;fullstack</PackageTags>
    <PackageProjectUrl>https://github.com/huythinh2507/GOK_cafe</PackageProjectUrl>
    <RepositoryUrl>https://github.com/huythinh2507/GOK_cafe</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>

    <!-- Include content files -->
    <ContentTargetFolders>content;contentFiles</ContentTargetFolders>
    <EnableDefaultContentItems>false</EnableDefaultContentItems>
  </PropertyGroup>

  <!-- README and Icon -->
  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
    <None Include="INSTALLATION.md" Pack="true" PackagePath="\" />
    <None Include="CHANGELOG.md" Pack="true" PackagePath="\" />
    <None Include="icon.png" Pack="true" PackagePath="\" />
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

**Deliverable**: Configured .csproj file with all dependencies and content inclusion rules

### 5. Create Folder Structure

```bash
cd Gotik.Commerce

# Create all necessary folders
mkdir Controllers\Api
mkdir Controllers\Render
mkdir Services
mkdir DTOs\Product
mkdir DTOs\Cart
mkdir DTOs\Order
mkdir DTOs\Category
mkdir DTOs\Common
mkdir Views\Partials\Homepage
mkdir Views\Partials\Products
mkdir Views\Partials\ProductDetail
mkdir Views\Partials\Shared
mkdir wwwroot\css
mkdir wwwroot\js
mkdir wwwroot\images
mkdir App_Plugins\Gotik
mkdir uSync\v9\DocumentTypes
mkdir uSync\v9\DataTypes
mkdir uSync\v9\Templates
mkdir Composing
mkdir Extensions
mkdir Migrations
```

**Complete Structure**:
```
Gotik.Commerce/
├── Controllers/
│   ├── Api/                           # API endpoints
│   │   ├── CartController.cs
│   │   ├── OrdersController.cs
│   │   ├── ProductsController.cs
│   │   └── CategoriesController.cs
│   └── Render/                        # Umbraco page controllers
│       ├── HomepageController.cs
│       ├── ProductListRenderController.cs
│       ├── ProductDetailPageController.cs
│       └── CategoryRenderController.cs
│
├── Services/                          # Business logic (optional if using project references)
│   ├── IProductService.cs
│   ├── ICartService.cs
│   ├── IOrderService.cs
│   └── IOdooService.cs
│
├── DTOs/                              # Data transfer objects
│   ├── Product/
│   ├── Cart/
│   ├── Order/
│   ├── Category/
│   └── Common/
│
├── Views/                             # Razor views
│   ├── Partials/
│   │   ├── Homepage/
│   │   ├── Products/
│   │   ├── ProductDetail/
│   │   └── Shared/
│   ├── Homepage.cshtml
│   ├── ProductList.cshtml
│   └── ProductDetails.cshtml
│
├── wwwroot/                           # Static assets
│   ├── css/
│   ├── js/
│   └── images/
│
├── App_Plugins/                       # Backoffice extensions
│   └── Gotik/
│       ├── package.manifest
│       └── backoffice/
│
├── uSync/                             # Document type definitions
│   └── v9/
│       ├── DocumentTypes/
│       ├── DataTypes/
│       └── Templates/
│
├── Composing/                         # Auto-registration
│   └── GotikCommerceComposer.cs
│
├── Extensions/                        # DI extensions
│   └── ServiceCollectionExtensions.cs
│
├── Migrations/                        # Database setup
│   └── CreateDocumentTypesMigration.cs
│
├── README.md
├── INSTALLATION.md
├── CHANGELOG.md
└── icon.png
```

**Deliverable**: Complete folder structure

---

## Phase 3: Migrate Code

### 6. Copy/Reference Backend Services

**Option A (Recommended): Use Project References**

Already configured in .csproj:
```xml
<ItemGroup>
  <ProjectReference Include="..\GOKCafe.Domain\GOKCafe.Domain.csproj" />
  <ProjectReference Include="..\GOKCafe.Application\GOKCafe.Application.csproj" />
  <ProjectReference Include="..\GOKCafe.Infrastructure\GOKCafe.Infrastructure.csproj" />
</ItemGroup>
```

✅ **Advantage**: No code duplication, easier maintenance
✅ **Package includes**: Domain, Application, and Infrastructure as dependencies

**Option B: Copy Services to Package**

Only if you want a standalone package:
```bash
xcopy GOKCafe.Application\Services\*.cs Gotik.Commerce\Services\ /S
xcopy GOKCafe.Application\DTOs\*.cs Gotik.Commerce\DTOs\ /S
```

Then update namespaces to `Gotik.Commerce.*`

**Deliverable**: Backend services accessible in package

### 7. Migrate API Controllers

Copy controllers from GOKCafe.API:

```bash
copy GOKCafe.API\Controllers\ProductsController.cs Gotik.Commerce\Controllers\Api\
copy GOKCafe.API\Controllers\CartController.cs Gotik.Commerce\Controllers\Api\
copy GOKCafe.API\Controllers\OrdersController.cs Gotik.Commerce\Controllers\Api\
copy GOKCafe.API\Controllers\CategoriesController.cs Gotik.Commerce\Controllers\Api\
copy GOKCafe.API\Controllers\AuthController.cs Gotik.Commerce\Controllers\Api\
```

**Update namespaces** in each controller:
```csharp
namespace Gotik.Commerce.Controllers.Api;
```

**Update using statements**:
```csharp
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services;
// etc.
```

**Deliverable**: API controllers in package with correct namespaces

### 8. Migrate Frontend Controllers

Copy render controllers from GOKCafe.Web:

```bash
copy GOKCafe.Web\Controllers\ProductDetailPageController.cs Gotik.Commerce\Controllers\Render\
copy GOKCafe.Web\Controllers\HomepageController.cs Gotik.Commerce\Controllers\Render\
# Copy other render controllers as needed
```

**Update namespaces**:
```csharp
namespace Gotik.Commerce.Controllers.Render;
```

**Deliverable**: Render controllers for Umbraco pages

### 9. Migrate Views

Copy all views from GOKCafe.Web:

```bash
# Copy main views
copy GOKCafe.Web\Views\Homepage.cshtml Gotik.Commerce\Views\
copy GOKCafe.Web\Views\ProductList.cshtml Gotik.Commerce\Views\
copy GOKCafe.Web\Views\ProductDetails.cshtml Gotik.Commerce\Views\

# Copy partials
xcopy GOKCafe.Web\Views\Partials\Homepage\*.cshtml Gotik.Commerce\Views\Partials\Homepage\ /S
xcopy GOKCafe.Web\Views\Partials\Products\*.cshtml Gotik.Commerce\Views\Partials\Products\ /S
xcopy GOKCafe.Web\Views\Partials\ProductDetail\*.cshtml Gotik.Commerce\Views\Partials\ProductDetail\ /S
xcopy GOKCafe.Web\Views\Partials\Shared\*.cshtml Gotik.Commerce\Views\Partials\Shared\ /S
```

**Update using statements** in views:
```cshtml
@using GOKCafe.Application.DTOs.Product
@using Gotik.Commerce.Controllers.Render
```

**Deliverable**: All views copied with updated references

### 10. Migrate Static Assets

Copy static files:

```bash
# Copy CSS
xcopy GOKCafe.Web\wwwroot\css\*.* Gotik.Commerce\wwwroot\css\ /S

# Copy JavaScript
xcopy GOKCafe.Web\wwwroot\js\*.* Gotik.Commerce\wwwroot\js\ /S

# Copy images
xcopy GOKCafe.Web\wwwroot\images\*.* Gotik.Commerce\wwwroot\images\ /S
```

**Deliverable**: All static assets in package

---

## Phase 4: Auto-Registration & Setup

### 11. Create Umbraco Composer

Create `Composing/GotikCommerceComposer.cs`:

```csharp
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using GOKCafe.Application.Services;
using GOKCafe.Infrastructure.Data;
using GOKCafe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gotik.Commerce.Composing;

/// <summary>
/// Composer that automatically registers Gotik Commerce services when the package is installed
/// </summary>
public class GotikCommerceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register DbContext
        var connectionString = builder.Config.GetConnectionString("GotikCommerceDb")
            ?? builder.Config.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

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
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();

        // Register HttpClient for Odoo
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

        // Add CORS (if needed for API calls)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("GotikCommercePolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add JWT Authentication
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

        // Log successful registration
        builder.Services.AddSingleton<ILogger>(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger("GotikCommerce"));
    }
}
```

**Deliverable**: Auto-registration composer

### 12. Create Extension Methods

Create `Extensions/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GOKCafe.Application.Services;
using GOKCafe.Infrastructure.Data;
using GOKCafe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gotik.Commerce.Extensions;

/// <summary>
/// Extension methods for registering Gotik Commerce services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all Gotik Commerce services to the application
    /// </summary>
    public static IServiceCollection AddGotikCommerce(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("GotikCommerceDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOdooService, OdooService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHomeService, HomeService>();

        // Infrastructure
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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

    /// <summary>
    /// Add only core commerce services (without Odoo integration)
    /// </summary>
    public static IServiceCollection AddGotikCommerceCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddGotikCommerce(configuration);
        // Can selectively remove services if needed
        return services;
    }

    /// <summary>
    /// Add only Odoo integration
    /// </summary>
    public static IServiceCollection AddGotikOdooIntegration(
        this IServiceCollection services)
    {
        services.AddScoped<IOdooService, OdooService>();
        services.AddHttpClient<IOdooService, OdooService>();
        return services;
    }
}
```

**Deliverable**: Flexible extension methods for DI

---

## Phase 5: Document Types & Content

### 13. Export Document Types

**Option A (Recommended): Use uSync**

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
xcopy GOKCafe.Web\uSync\v9\*.* Gotik.Commerce\uSync\v9\ /S
```

**Option B: Create Document Types Programmatically**

Create `Migrations/CreateDocumentTypesMigration.cs`:

```csharp
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Gotik.Commerce.Migrations;

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
        var exists = _contentTypeService.Get("gotikHomepage");
        if (exists != null) return;

        var contentType = new ContentType(_contentTypeService, -1)
        {
            Alias = "gotikHomepage",
            Name = "Gotik Homepage",
            Icon = "icon-home",
            AllowedAsRoot = true,
            Description = "Homepage for Gotik Commerce site"
        };

        // Add properties
        var titleProp = new PropertyType(_dataTypeService.GetDataType("Textstring"), "title")
        {
            Name = "Title",
            Description = "Page title",
            Mandatory = true
        };
        contentType.AddPropertyType(titleProp, "Content");

        _contentTypeService.Save(contentType);
    }

    private void CreateProductListDocumentType()
    {
        var exists = _contentTypeService.Get("gotikProductList");
        if (exists != null) return;

        var contentType = new ContentType(_contentTypeService, -1)
        {
            Alias = "gotikProductList",
            Name = "Gotik Product List",
            Icon = "icon-shopping-basket",
            Description = "Product listing page with filters"
        };

        // Add properties
        var pageTitle = new PropertyType(_dataTypeService.GetDataType("Textstring"), "pageTitle")
        {
            Name = "Page Title",
            Mandatory = true
        };
        contentType.AddPropertyType(pageTitle, "Content");

        var itemsPerPage = new PropertyType(_dataTypeService.GetDataType("Numeric"), "itemsPerPage")
        {
            Name = "Items Per Page",
            Description = "Number of products to display per page"
        };
        contentType.AddPropertyType(itemsPerPage, "Settings");

        _contentTypeService.Save(contentType);
    }

    private void CreateProductDetailDocumentType()
    {
        var exists = _contentTypeService.Get("gotikProductDetail");
        if (exists != null) return;

        var contentType = new ContentType(_contentTypeService, -1)
        {
            Alias = "gotikProductDetail",
            Name = "Gotik Product Detail",
            Icon = "icon-barcode",
            Description = "Individual product detail page"
        };

        // Add properties
        var productId = new PropertyType(_dataTypeService.GetDataType("Label"), "productId")
        {
            Name = "Product ID",
            Description = "GUID of the product from commerce database"
        };
        contentType.AddPropertyType(productId, "Content");

        _contentTypeService.Save(contentType);
    }
}
```

**Deliverable**: Document types available for installation

### 14. Create App_Plugins Configuration

Create `App_Plugins/Gotik/package.manifest`:

```json
{
  "name": "Gotik Commerce",
  "version": "1.0.0",
  "allowPackageTelemetry": false,
  "javascript": [
    "~/App_Plugins/Gotik/backoffice/gotik.controller.js"
  ],
  "css": [
    "~/App_Plugins/Gotik/backoffice/gotik.css"
  ],
  "sections": [],
  "dashboards": [
    {
      "alias": "gotikCommerceDashboard",
      "view": "~/App_Plugins/Gotik/backoffice/dashboard.html",
      "sections": ["content"],
      "weight": 10,
      "access": [
        {
          "grant": "admin"
        }
      ]
    }
  ]
}
```

Create `App_Plugins/Gotik/backoffice/dashboard.html`:

```html
<div ng-controller="GotikCommerceDashboardController">
    <h1>Gotik Commerce</h1>
    <div class="umb-box">
        <div class="umb-box-header">
            <h3>Welcome to Gotik Commerce</h3>
        </div>
        <div class="umb-box-content">
            <p>Your e-commerce solution is ready!</p>
            <ul>
                <li>✅ Product Management</li>
                <li>✅ Shopping Cart</li>
                <li>✅ Order Processing</li>
                <li>✅ Odoo Integration</li>
            </ul>
        </div>
    </div>
</div>
```

Create `App_Plugins/Gotik/backoffice/gotik.controller.js`:

```javascript
angular.module("umbraco").controller("GotikCommerceDashboardController",
    function ($scope) {
        console.log("Gotik Commerce Dashboard Loaded");
    }
);
```

**Deliverable**: Backoffice integration configured

---

## Phase 6: Documentation

### 15. Create README.md

Create `README.md` (see full content in separate section below)

**Key sections**:
- Features overview
- Installation instructions
- Quick start guide
- API documentation
- Configuration options
- Customization guide

**Deliverable**: Comprehensive README

### 16. Create INSTALLATION.md

Create detailed installation guide with:
- Prerequisites
- Step-by-step installation
- Database setup
- Configuration
- First-time setup
- Troubleshooting

**Deliverable**: Installation guide

### 17. Create CHANGELOG.md

```markdown
# Changelog

All notable changes to Gotik Commerce will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-06

### Added
- Initial release
- Product catalog with dynamic filtering
- Shopping cart (session-based and authenticated)
- Order management system
- Odoo ERP integration
- Category management
- JWT authentication
- User management
- Stock management with reservation
- Product attributes (FlavourProfile, Equipment)
- Payment tracking
- Order status workflow
- Responsive frontend views
- RESTful API controllers
- Auto-registration via Umbraco Composer
- Document type definitions
- Backoffice dashboard integration

### Features
- Dynamic product filtering (category, flavour, equipment, stock)
- Advanced search functionality
- Pagination support
- Distributed caching for performance
- Soft delete support
- Audit trails (CreatedAt, UpdatedAt)
- Batch operations for large datasets
- Product image gallery support

[1.0.0]: https://github.com/huythinh2507/GOK_cafe/releases/tag/v1.0.0
```

**Deliverable**: Changelog for version tracking

### 18. Create Package Icon

Create `icon.png`:
- Size: 128x128 or 256x256 pixels
- Format: PNG with transparency
- Design: Gotik Commerce logo/branding

**Deliverable**: Package icon image

---

## Phase 7: Build & Package

### 19. Configure .csproj for Packaging

Already configured in Step 4, verify:

```xml
<!-- Ensure these are present -->
<PropertyGroup>
  <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

**Deliverable**: Project file ready for packaging

### 20. Build the Package

```bash
cd d:\GOK_Cafe_BE\GOK_cafe\Gotik.Commerce

# Clean previous builds
dotnet clean

# Build in Release mode
dotnet build -c Release

# Create NuGet package
dotnet pack -c Release -o ./nupkg

# Verify output
dir nupkg
```

**Expected Output**:
- `Gotik.Commerce.1.0.0.nupkg` - Main package
- `Gotik.Commerce.1.0.0.snupkg` - Symbol package for debugging

**Deliverable**: NuGet package files

### 21. Inspect Package Contents

```bash
# Verify package structure (using NuGet Package Explorer or CLI)
dotnet tool install -g NuGetPackageExplorer
nuget-explorer Gotik.Commerce.1.0.0.nupkg
```

**Verify includes**:
- ✅ Controllers (Api & Render)
- ✅ Views (.cshtml files)
- ✅ wwwroot (CSS, JS, images)
- ✅ App_Plugins
- ✅ uSync folder
- ✅ README.md
- ✅ Dependencies (GOKCafe.Domain, Application, Infrastructure)

**Deliverable**: Verified package contents

---

## Phase 8: Testing

### 22. Create Test Environment

```bash
# Create new test Umbraco site
cd d:\GOK_Cafe_BE
dotnet new umbraco -n TestGotikCommerce
cd TestGotikCommerce

# Add local package source
dotnet nuget add source d:\GOK_Cafe_BE\GOK_cafe\Gotik.Commerce\nupkg -n LocalGotik

# Install Gotik Commerce package
dotnet add package Gotik.Commerce --source LocalGotik

# Run the site
dotnet run
```

**Deliverable**: Test Umbraco site with package installed

### 23. Verify Installation

**A. Check Files Copied**

```bash
# Verify Views
dir Views\Partials\Products
dir Views\ProductList.cshtml

# Verify wwwroot
dir wwwroot\css
dir wwwroot\js

# Verify uSync
dir uSync\v9\DocumentTypes
```

**B. Check Services Registered**

```bash
# Start the application
dotnet run

# Browse to:
# https://localhost:44317/api/products
# Should return product list (or empty array)
```

**C. Check Umbraco Backoffice**

1. Complete Umbraco installation wizard
2. Login to backoffice
3. Check **Settings** > **Document Types**
   - Should see: Gotik Homepage, Gotik Product List, Gotik Product Detail
4. Check **Content** > Create new content
   - Should see Gotik document types available

**Deliverable**: Verified installation checklist

### 24. Test Core Functionality

**A. Test Product API**

```bash
# Test product listing
curl https://localhost:44317/api/v1/products

# Test product filtering
curl https://localhost:44317/api/v1/products?categoryIds=<guid>&pageNumber=1&pageSize=10

# Test product detail
curl https://localhost:44317/api/v1/products/<product-id>
```

**B. Test Cart Functionality**

```bash
# Add to cart
curl -X POST https://localhost:44317/api/v1/cart/items \
  -H "Content-Type: application/json" \
  -d '{"productId":"<guid>","quantity":1}'

# Get cart
curl https://localhost:44317/api/v1/cart?sessionId=<session-id>
```

**C. Test Order Creation**

```bash
# Create order
curl -X POST https://localhost:44317/api/v1/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName":"Test User",
    "email":"test@example.com",
    "phone":"1234567890",
    "shippingAddress":"123 Test St",
    "items":[{"productId":"<guid>","quantity":1}]
  }'
```

**D. Test Frontend Rendering**

1. Create Homepage node in Umbraco
2. Create Product List node
3. Browse to frontend URL
4. Verify:
   - ✅ Products display
   - ✅ Filters work
   - ✅ Pagination works
   - ✅ Product detail links work

**Deliverable**: Tested core functionality checklist

### 25. Test Database Migrations

```bash
# Check if migrations run automatically
dotnet ef migrations list --project Gotik.Commerce

# If needed, run migrations manually
dotnet ef database update --project Gotik.Commerce
```

**Verify database tables created**:
- Products
- Categories
- Carts
- CartItems
- Orders
- OrderItems
- Users
- FlavourProfiles
- Equipment
- ProductFlavourProfiles
- ProductEquipment

**Deliverable**: Database properly configured

### 26. Performance Testing

**A. Test Caching**

```bash
# First request (cache miss)
time curl https://localhost:44317/api/v1/products

# Second request (cache hit - should be faster)
time curl https://localhost:44317/api/v1/products
```

**B. Test Large Dataset**

```bash
# Test with many products (if Odoo integration available)
curl -X POST https://localhost:44317/api/v1/products/odoo/sync
```

**Deliverable**: Performance benchmarks

---

## Phase 9: Publishing

### 27. Publish to NuGet.org

**Prerequisites**:
1. Create account at https://www.nuget.org
2. Generate API key
3. Verify package metadata

**Publish**:

```bash
cd d:\GOK_Cafe_BE\GOK_cafe\Gotik.Commerce\nupkg

# Push to NuGet.org
dotnet nuget push Gotik.Commerce.1.0.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json

# Verify on NuGet.org
# https://www.nuget.org/packages/Gotik.Commerce/
```

**Deliverable**: Package published on NuGet.org

### 28. Publish to Umbraco Marketplace

**Prerequisites**:
1. Create account at https://marketplace.umbraco.com
2. Prepare marketing materials:
   - Screenshots (product list, product detail, backoffice)
   - Feature descriptions
   - Pricing information (free or paid)

**Steps**:
1. Go to https://marketplace.umbraco.com/submit
2. Fill in package details:
   - Name: Gotik Commerce
   - Description: (from README)
   - Category: Commerce
   - Compatibility: Umbraco 16.3.4+
3. Upload package file (.nupkg)
4. Upload screenshots
5. Set pricing
6. Submit for review

**Deliverable**: Package listed on Umbraco Marketplace

### 29. Publish to GitHub Packages (Optional)

```bash
# Configure GitHub source
dotnet nuget add source \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/huythinh2507/index.json"

# Push package
cd d:\GOK_Cafe_BE\GOK_cafe\Gotik.Commerce\nupkg
dotnet nuget push Gotik.Commerce.1.0.0.nupkg --source "github"
```

**Deliverable**: Package available on GitHub Packages

### 30. Create GitHub Release

```bash
# Tag the release
git tag -a v1.0.0 -m "Gotik Commerce v1.0.0 - Initial Release"
git push origin v1.0.0

# Create release on GitHub
# Go to: https://github.com/huythinh2507/GOK_cafe/releases/new
# - Tag: v1.0.0
# - Title: Gotik Commerce v1.0.0
# - Description: (from CHANGELOG.md)
# - Attach: Gotik.Commerce.1.0.0.nupkg
```

**Deliverable**: GitHub release created

---

## Phase 10: Marketing & Maintenance

### 31. Create Marketing Materials

**A. Package Description** (for marketplace):

```
Gotik Commerce - Complete E-Commerce Solution for Umbraco

Transform your Umbraco CMS into a powerful e-commerce platform with Gotik Commerce.
Built with enterprise-grade architecture, this package provides everything you need
to run a modern online store.

✨ KEY FEATURES:
• Product Catalog with Advanced Filtering
• Shopping Cart (Session-based & Authenticated)
• Order Management System
• Odoo ERP Integration
• Category Management
• JWT Authentication
• RESTful API
• Responsive Frontend Views
• Stock Management
• Payment Tracking

🎯 PERFECT FOR:
• Coffee shops & cafes
• Tea retailers
• Food & beverage e-commerce
• Any product-based online store

🚀 QUICK SETUP:
Install via NuGet, run migrations, and you're ready to sell!

📚 FULL DOCUMENTATION:
Complete installation guide, API reference, and customization examples included.

💪 ENTERPRISE READY:
Built with clean architecture, Entity Framework Core, soft deletes,
audit trails, and distributed caching.
```

**B. Screenshots**:
1. Product list with filters
2. Product detail page
3. Shopping cart
4. Order management
5. Backoffice dashboard
6. Odoo sync interface

**C. Feature Video** (optional):
- 2-3 minute demo
- Show installation
- Demonstrate key features
- Highlight unique selling points

**Deliverable**: Marketing assets created

### 32. Create Documentation Site

**Options**:

**A. GitHub Pages**:
```bash
# Create docs folder
mkdir docs
cd docs

# Create documentation structure
# Use tools like: MkDocs, Docusaurus, or Jekyll
```

**B. ReadTheDocs.org**:
```bash
# Create Sphinx documentation
pip install sphinx
sphinx-quickstart
```

**C. Simple Markdown**:
```
docs/
├── index.md
├── installation.md
├── configuration.md
├── api-reference.md
├── customization.md
├── troubleshooting.md
└── faq.md
```

**Deliverable**: Documentation site

### 33. Set Up Support Channels

**A. GitHub Issues**:
- Enable issue templates
- Create labels (bug, enhancement, question)
- Set up issue automation

**B. Discussions**:
- Enable GitHub Discussions
- Create categories (Q&A, Ideas, Show and Tell)

**C. Email Support** (optional):
- support@gotikcommerce.com

**Deliverable**: Support infrastructure

### 34. Version Management Strategy

**Semantic Versioning**:
- MAJOR.MINOR.PATCH (e.g., 1.0.0)
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

**Release Schedule**:
- Patch releases: As needed (bug fixes)
- Minor releases: Quarterly (new features)
- Major releases: Annually (breaking changes)

**Deliverable**: Version management plan

### 35. Create Announcement Blog Post

```markdown
# Introducing Gotik Commerce - E-Commerce Made Easy for Umbraco

We're excited to announce the release of Gotik Commerce v1.0.0,
a complete e-commerce solution for Umbraco CMS!

## What is Gotik Commerce?

Gotik Commerce transforms your Umbraco CMS into a powerful e-commerce
platform with enterprise-grade features...

## Key Features

[List features with examples]

## Getting Started

Installation is as simple as:
```bash
dotnet add package Gotik.Commerce
```

## What's Next?

We're already working on v1.1.0 with exciting new features...

## Try It Now!

Download from NuGet or Umbraco Marketplace today!
```

**Publish to**:
- Your company blog
- Dev.to
- Medium
- Umbraco Community blog

**Deliverable**: Announcement published

---

## Maintenance Checklist

### Regular Maintenance Tasks

**Weekly**:
- ✅ Monitor GitHub issues
- ✅ Respond to questions
- ✅ Review pull requests

**Monthly**:
- ✅ Update dependencies
- ✅ Security audit
- ✅ Performance review
- ✅ Documentation updates

**Quarterly**:
- ✅ Minor version release
- ✅ Feature additions
- ✅ User feedback review
- ✅ Roadmap planning

**Annually**:
- ✅ Major version planning
- ✅ Architecture review
- ✅ Umbraco version compatibility
- ✅ Technology stack updates

---

## Success Metrics

Track these metrics to measure package success:

1. **Downloads**: NuGet download count
2. **Stars**: GitHub repository stars
3. **Issues**: Number and resolution time
4. **Feedback**: User reviews and ratings
5. **Adoption**: Number of live sites using package
6. **Community**: Contributors and PR submissions

---

## Quick Reference Commands

```bash
# Build package
cd Gotik.Commerce
dotnet pack -c Release -o ./nupkg

# Test locally
dotnet nuget add source ./nupkg -n LocalGotik
dotnet add package Gotik.Commerce --source LocalGotik

# Publish to NuGet
dotnet nuget push Gotik.Commerce.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json

# Create release tag
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

---

## Resources

- **Umbraco Docs**: https://docs.umbraco.com/
- **Umbraco Package Development**: https://docs.umbraco.com/umbraco-cms/extending/packages
- **NuGet Docs**: https://docs.microsoft.com/nuget/
- **Semantic Versioning**: https://semver.org/

---

**Document Version**: 1.0
**Last Updated**: 2024-12-06
**Status**: Ready for Implementation
