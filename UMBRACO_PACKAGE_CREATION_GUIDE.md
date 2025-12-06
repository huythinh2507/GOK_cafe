# Umbraco Package Creation Guide: Frontend & Backend Integration

## Table of Contents
1. [Overview](#overview)
2. [Package Types Available](#package-types-available)
3. [Current Implementation Analysis](#current-implementation-analysis)
4. [Creating a Complete Umbraco Package](#creating-a-complete-umbraco-package)
5. [Step-by-Step Implementation](#step-by-step-implementation)
6. [Package Structure](#package-structure)
7. [Testing Your Package](#testing-your-package)
8. [Publishing Your Package](#publishing-your-package)

---

## Overview

**Yes, it is possible** to create an Umbraco package that includes both Frontend (FE) and Backend (BE) components!

Umbraco packages can include:
- **Backend**: Services, Controllers, DTOs, Database migrations, APIs
- **Frontend**: Razor Views, Partial Views, CSS, JavaScript, Static Assets
- **Document Types**: Content types, templates, data types
- **Configuration**: App settings, dependencies, startup logic

---

## Package Types Available

### 1. **NuGet Package** (Recommended for your case)
- **Best for**: Distributable packages with both FE & BE
- **Format**: `.nupkg` file
- **Distribution**: NuGet.org or private NuGet feed
- **Includes**: DLLs, Views, Static files, Configuration

### 2. **Umbraco Package (Legacy)**
- **Best for**: Content types and backoffice extensions
- **Format**: `.zip` or `.umb` file
- **Distribution**: Umbraco Marketplace
- **Includes**: Document types, templates, data types

### 3. **Hybrid Approach** (Your Current Setup)
- Combine both NuGet package + Umbraco package
- NuGet for backend logic
- Umbraco package for content types and views

---

## Current Implementation Analysis

Based on your codebase, here's what you have:

### ✅ Already Packaged (Backend)
```
GOKCafe.Commerce.Package/
├── Controllers/          # API Controllers
├── Services/            # Business Logic
├── DTOs/               # Data Transfer Objects
├── Extensions/         # Service Registration
└── README.md           # Documentation
```

**Status**: Ready as NuGet package for backend APIs

### ❌ Not Yet Packaged (Frontend + Umbraco Integration)
```
GOKCafe.Web/
├── Views/
│   ├── Partials/
│   │   ├── Homepage/           # Homepage sections
│   │   ├── Products/           # Product grid with filters
│   │   ├── ProductDetail/      # Product detail page
│   │   └── Shared/            # Shared components
├── Controllers/
│   ├── HomepageController.cs          # Render controller
│   ├── ProductListRenderController.cs # Product list with filters
│   ├── ProductDetailPageController.cs # Product detail page
│   └── CategoryRenderController.cs    # Category page
├── wwwroot/                   # Static assets (CSS, JS, Images)
└── Models/                    # View models and DTOs
```

**Status**: Needs to be packaged as Umbraco package

---

## Creating a Complete Umbraco Package

### Strategy: Two-Package Approach

#### Package 1: Backend Commerce Services (Already Created ✅)
- **Name**: `GOKCafe.Commerce`
- **Type**: NuGet Package
- **Contains**: Services, DTOs, API Controllers
- **Usage**: Referenced by any .NET 8+ project

#### Package 2: Umbraco Frontend Package (To Be Created 📦)
- **Name**: `GOKCafe.Umbraco.ProductCatalog`
- **Type**: NuGet Package with Umbraco-specific content
- **Contains**:
  - Render Controllers
  - Razor Views (Products, Homepage, Details)
  - JavaScript filters
  - CSS styles
  - Document Type definitions
  - Startup configuration
- **Depends On**: `GOKCafe.Commerce` package

---

## Step-by-Step Implementation

### Phase 1: Create Umbraco Package Project Structure

#### Step 1: Create New Package Project

```bash
# Navigate to solution directory
cd d:\GOK_Cafe_BE\GOK_cafe

# Create new class library project
dotnet new classlib -n GOKCafe.Umbraco.ProductCatalog -f net9.0
dotnet sln add GOKCafe.Umbraco.ProductCatalog
```

#### Step 2: Configure Package Project File

Create `GOKCafe.Umbraco.ProductCatalog.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>

    <!-- NuGet Package Metadata -->
    <PackageId>GOKCafe.Umbraco.ProductCatalog</PackageId>
    <Version>1.0.0</Version>
    <Authors>GOK Cafe Team</Authors>
    <Company>GOK Cafe</Company>
    <Product>GOK Cafe Umbraco Product Catalog</Product>
    <Description>Complete Umbraco product catalog with dynamic filtering, product details, and homepage integration. Includes views, controllers, and frontend assets.</Description>
    <PackageTags>umbraco;ecommerce;product-catalog;frontend;razor;filters</PackageTags>
    <PackageProjectUrl>https://github.com/huythinh2507/GOK_cafe</PackageProjectUrl>
    <RepositoryUrl>https://github.com/huythinh2507/GOK_cafe</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>

    <!-- Important: Include content files in package -->
    <ContentTargetFolders>content</ContentTargetFolders>
    <EnableDefaultContentItems>false</EnableDefaultContentItems>
  </PropertyGroup>

  <!-- Include README in package -->
  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <!-- Umbraco Dependencies -->
  <ItemGroup>
    <PackageReference Include="Umbraco.Cms.Web.Website" Version="16.3.4" />
    <PackageReference Include="Umbraco.Cms.Web.BackOffice" Version="16.3.4" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  </ItemGroup>

  <!-- Commerce Package Dependency -->
  <ItemGroup>
    <PackageReference Include="GOKCafe.Commerce" Version="1.0.0" />
    <!-- OR use project reference during development -->
    <!-- <ProjectReference Include="..\GOKCafe.Commerce.Package\GOKCafe.Commerce.Package.csproj" /> -->
  </ItemGroup>

  <!-- Include Razor Views -->
  <ItemGroup>
    <Content Include="Views/**/*.cshtml">
      <Pack>true</Pack>
      <PackagePath>content/Views</PackagePath>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Include Static Assets (wwwroot) -->
  <ItemGroup>
    <Content Include="wwwroot/**/*.*">
      <Pack>true</Pack>
      <PackagePath>contentFiles/any/net9.0/wwwroot</PackagePath>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Include App_Plugins (Umbraco Backoffice) -->
  <ItemGroup>
    <Content Include="App_Plugins/**/*.*">
      <Pack>true</Pack>
      <PackagePath>content/App_Plugins</PackagePath>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Include Models/DTOs -->
  <ItemGroup>
    <Content Include="Models/**/*.cs">
      <Pack>true</Pack>
      <PackagePath>content/Models</PackagePath>
    </Content>
  </ItemGroup>

</Project>
```

---

### Phase 2: Organize Package Structure

Create this folder structure in `GOKCafe.Umbraco.ProductCatalog`:

```
GOKCafe.Umbraco.ProductCatalog/
├── Controllers/
│   ├── HomepageController.cs
│   ├── ProductListRenderController.cs
│   ├── ProductDetailPageController.cs
│   └── CategoryRenderController.cs
│
├── Views/
│   ├── Partials/
│   │   ├── Homepage/
│   │   │   ├── _OurTeaRangeSection.cshtml
│   │   │   └── _ShowAllTea.cshtml
│   │   ├── Products/
│   │   │   └── ProductsGrid.cshtml
│   │   ├── ProductDetail/
│   │   │   ├── _ProductInformation.cshtml
│   │   │   └── _RecommendProduct.cshtml
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   ├── Homepage.cshtml
│   ├── ProductList.cshtml
│   └── ProductDetails.cshtml
│
├── Models/
│   ├── DTOs/
│   │   ├── ProductDto.cs
│   │   ├── CategoryDto.cs
│   │   ├── ProductFiltersDto.cs
│   │   └── PaginatedResponse.cs
│   └── ViewModels/
│       ├── ProductListViewModel.cs
│       └── ProductDetailViewModel.cs
│
├── wwwroot/
│   ├── css/
│   │   └── products.css
│   ├── js/
│   │   ├── product-filters.js
│   │   └── product-details.js
│   └── images/
│       └── placeholder.png
│
├── App_Plugins/
│   └── GOKCafeProductCatalog/
│       ├── package.manifest
│       └── lang/
│           └── en-US.xml
│
├── Composing/
│   └── ProductCatalogComposer.cs
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
├── Migrations/
│   └── ProductCatalogMigration.cs
│
├── uSync/
│   └── v9/
│       ├── DocumentTypes/
│       │   ├── Homepage.config
│       │   ├── ProductList.config
│       │   └── ProductDetail.config
│       ├── DataTypes/
│       └── Templates/
│
├── README.md
└── INSTALLATION_GUIDE.md
```

---

### Phase 3: Move Files from GOKCafe.Web

#### Step 1: Copy Controllers

```bash
# Copy render controllers
cp GOKCafe.Web/Controllers/HomepageController.cs GOKCafe.Umbraco.ProductCatalog/Controllers/
cp GOKCafe.Web/Controllers/ProductListRenderController.cs GOKCafe.Umbraco.ProductCatalog/Controllers/
cp GOKCafe.Web/Controllers/ProductDetailPageController.cs GOKCafe.Umbraco.ProductCatalog/Controllers/
cp GOKCafe.Web/Controllers/CategoryRenderController.cs GOKCafe.Umbraco.ProductCatalog/Controllers/
```

Update namespace in each controller:
```csharp
namespace GOKCafe.Umbraco.ProductCatalog.Controllers;
```

#### Step 2: Copy Views

```bash
# Copy all partial views
cp -r GOKCafe.Web/Views/Partials/* GOKCafe.Umbraco.ProductCatalog/Views/Partials/

# Copy main views
cp GOKCafe.Web/Views/Homepage.cshtml GOKCafe.Umbraco.ProductCatalog/Views/
cp GOKCafe.Web/Views/ProductList.cshtml GOKCafe.Umbraco.ProductCatalog/Views/
cp GOKCafe.Web/Views/ProductDetails.cshtml GOKCafe.Umbraco.ProductCatalog/Views/
```

#### Step 3: Copy Models/DTOs

```bash
# Copy models
cp -r GOKCafe.Web/Models/* GOKCafe.Umbraco.ProductCatalog/Models/
```

#### Step 4: Copy Static Assets

```bash
# Copy wwwroot content (if any custom CSS/JS for products)
cp -r GOKCafe.Web/wwwroot/css/products.css GOKCafe.Umbraco.ProductCatalog/wwwroot/css/
cp -r GOKCafe.Web/wwwroot/js/product-*.js GOKCafe.Umbraco.ProductCatalog/wwwroot/js/
```

---

### Phase 4: Create Composer for Auto-Registration

Create `Composing/ProductCatalogComposer.cs`:

```csharp
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using GOKCafe.Commerce.Extensions;

namespace GOKCafe.Umbraco.ProductCatalog.Composing;

public class ProductCatalogComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register GOK Cafe Commerce backend services
        builder.Services.AddGOKCafeCommerce(builder.Config);

        // Register AutoMapper for DTOs
        builder.Services.AddAutoMapper(typeof(ProductCatalogComposer).Assembly);

        // Register any custom services for frontend
        // builder.Services.AddScoped<IProductViewService, ProductViewService>();
    }
}
```

---

### Phase 5: Create Extension Method (Optional)

Create `Extensions/ServiceCollectionExtensions.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GOKCafe.Commerce.Extensions;

namespace GOKCafe.Umbraco.ProductCatalog.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGOKCafeProductCatalog(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add commerce services
        services.AddGOKCafeCommerce(configuration);

        // Add AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        // Add any frontend-specific services here

        return services;
    }
}
```

---

### Phase 6: Export Document Types (uSync)

#### Option A: Use uSync to Export

1. In your current Umbraco backoffice, install uSync (if not already installed)
2. Create your document types for:
   - Homepage
   - Product List Page
   - Product Detail Page
3. Export them using uSync:

```bash
# Navigate to Umbraco backoffice
# Settings > uSync > Export All
```

4. Copy exported files to package:

```bash
cp -r GOKCafe.Web/uSync/v9/DocumentTypes/* GOKCafe.Umbraco.ProductCatalog/uSync/v9/DocumentTypes/
cp -r GOKCafe.Web/uSync/v9/DataTypes/* GOKCafe.Umbraco.ProductCatalog/uSync/v9/DataTypes/
cp -r GOKCafe.Web/uSync/v9/Templates/* GOKCafe.Umbraco.ProductCatalog/uSync/v9/Templates/
```

#### Option B: Create Migrations

Create `Migrations/ProductCatalogMigration.cs`:

```csharp
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Core.Services;

namespace GOKCafe.Umbraco.ProductCatalog.Migrations;

public class ProductCatalogMigration : MigrationBase
{
    private readonly IContentTypeService _contentTypeService;

    public ProductCatalogMigration(
        IMigrationContext context,
        IContentTypeService contentTypeService) : base(context)
    {
        _contentTypeService = contentTypeService;
    }

    protected override void Migrate()
    {
        // Create Homepage Document Type
        var homepage = _contentTypeService.Get("homepage");
        if (homepage == null)
        {
            homepage = new ContentType(_contentTypeService, -1)
            {
                Alias = "homepage",
                Name = "Homepage",
                Icon = "icon-home"
            };

            // Add properties
            // ... (add your properties here)

            _contentTypeService.Save(homepage);
        }

        // Create Product List Document Type
        // ... similar code

        // Create Product Detail Document Type
        // ... similar code
    }
}
```

---

### Phase 7: Create Package Documentation

Create `README.md`:

```markdown
# GOK Cafe Umbraco Product Catalog

Complete Umbraco product catalog package with dynamic filtering, product detail pages, and homepage integration.

## Features

- 🏠 **Homepage Integration**: Tea range showcase and featured products
- 📦 **Product Grid**: Dynamic product listing with advanced filters
  - Category filtering
  - Flavour profile filtering
  - Equipment filtering
  - Availability filtering (In Stock/Out of Stock)
  - Search functionality
  - Pagination support
- 🔍 **Product Detail Page**: Complete product information with recommendations
- 🎨 **Responsive Design**: Mobile-first Tailwind CSS styling
- ⚡ **Dynamic Filtering**: Real-time client-side filtering with URL parameters
- 🔗 **Backend Integration**: Seamless integration with GOKCafe.Commerce package

## Requirements

- Umbraco CMS 16.3.4+
- .NET 9.0+
- GOKCafe.Commerce 1.0.0+ (automatically installed as dependency)

## Installation

### 1. Install NuGet Package

```bash
dotnet add package GOKCafe.Umbraco.ProductCatalog
```

### 2. Install uSync (if not already installed)

```bash
dotnet add package uSync
```

### 3. Configure Backend Services

The package automatically registers all required services through the `ProductCatalogComposer`. No manual configuration needed!

**Optional**: If you need manual control, add this to your `Program.cs`:

```csharp
using GOKCafe.Umbraco.ProductCatalog.Extensions;

builder.Services.AddGOKCafeProductCatalog(builder.Configuration);
```

### 4. Configure appsettings.json

Add your backend API and database configuration:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Your-Umbraco-Database-Connection",
    "DefaultConnection": "Your-Commerce-Database-Connection"
  },
  "ApiSettings": {
    "BaseUrl": "https://your-api.com",
    "Timeout": 30
  },
  "Jwt": {
    "Key": "Your-Secret-Key-Min-32-Characters",
    "Issuer": "GOKCafe",
    "Audience": "GOKCafe"
  }
}
```

### 5. Import Document Types (uSync)

After installing the package:

1. Go to Umbraco Backoffice
2. Navigate to **Settings** > **uSync**
3. Click **Import**
4. The following document types will be created:
   - Homepage
   - Product List Page
   - Product Detail Page

### 6. Create Content

1. Create a **Homepage** node
2. Create a **Product List** node
3. Product detail pages are created dynamically from your backend

## Usage

### Homepage

The homepage automatically fetches and displays:
- Featured tea range (6 items)
- Featured products (4 items)

Views used:
- `/Views/Homepage.cshtml`
- `/Views/Partials/Homepage/_OurTeaRangeSection.cshtml`
- `/Views/Partials/Homepage/_ShowAllTea.cshtml`

### Product List Page

Create a node with document type "Product List Page". The page includes:
- Dynamic category filtering
- Flavour profile filters
- Equipment filters
- Availability filters
- Search functionality
- Pagination

View: `/Views/Partials/Products/ProductsGrid.cshtml`

### Product Detail Page

Products are accessible via: `/productdetails?id={productId}`

The page shows:
- Product images
- Product information
- Pricing (with discount support)
- Recommended products

Views:
- `/Views/ProductDetails.cshtml`
- `/Views/Partials/ProductDetail/_ProductInformation.cshtml`
- `/Views/Partials/ProductDetail/_RecommendProduct.cshtml`

## Customization

### Styling

The package uses Tailwind CSS. To customize:

1. Override styles in your main site CSS
2. Use Tailwind classes in views
3. Modify `/wwwroot/css/products.css` for product-specific styles

### Filtering Logic

JavaScript filtering is in `/wwwroot/js/product-filters.js`. Modify as needed.

### Views

All views can be overridden by creating files with the same path in your main project.

## API Endpoints Used

The package consumes these endpoints from GOKCafe.Commerce:

- `GET /api/products` - List products with filters
- `GET /api/products/{id}` - Get product details
- `GET /api/categories` - Get categories
- `GET /api/products/filters` - Get filter options

## Troubleshooting

### Views not showing
- Ensure Razor runtime compilation is enabled
- Check view paths match exactly
- Verify package is properly referenced

### Filters not working
- Check browser console for JavaScript errors
- Verify API endpoints are accessible
- Check CORS settings if API is on different domain

### Document types not imported
- Manually import via uSync
- Check uSync configuration
- Verify uSync files are in correct location

## Support

- GitHub: https://github.com/huythinh2507/GOK_cafe
- Issues: https://github.com/huythinh2507/GOK_cafe/issues

## License

MIT
```

---

## Package Structure

### Final Package Structure

```
GOKCafe.Umbraco.ProductCatalog.1.0.0.nupkg
├── content/
│   ├── Views/                    # Auto-copied to consuming project
│   ├── App_Plugins/             # Auto-copied to consuming project
│   └── Models/                  # Source files for reference
├── contentFiles/
│   └── any/net9.0/
│       └── wwwroot/             # Static assets
├── lib/
│   └── net9.0/
│       └── GOKCafe.Umbraco.ProductCatalog.dll  # Compiled assembly
├── README.md
└── .nuspec metadata
```

---

## Testing Your Package

### Test Locally Before Publishing

#### Step 1: Build Package

```bash
cd GOKCafe.Umbraco.ProductCatalog
dotnet pack -c Release -o ./nupkg
```

#### Step 2: Create Local NuGet Feed

```bash
# Create a local feed directory
mkdir C:\LocalNuGet

# Copy package
cp nupkg/GOKCafe.Umbraco.ProductCatalog.1.0.0.nupkg C:\LocalNuGet\
```

#### Step 3: Add Local Feed to NuGet

```bash
# Add local source
dotnet nuget add source C:\LocalNuGet -n LocalPackages
```

#### Step 4: Create Test Umbraco Site

```bash
# Create new Umbraco site
dotnet new umbraco -n TestSite
cd TestSite

# Install your package
dotnet add package GOKCafe.Umbraco.ProductCatalog --source LocalPackages

# Run and test
dotnet run
```

#### Step 5: Verify Installation

1. Check files copied:
   - `/Views/Partials/Products/ProductsGrid.cshtml`
   - `/wwwroot/css/products.css`
   - `/App_Plugins/GOKCafeProductCatalog/`

2. Check services registered:
   - Commerce services available
   - AutoMapper configured

3. Import document types via uSync

4. Create test content

---

## Publishing Your Package

### Option 1: Publish to NuGet.org (Public)

```bash
# Get API key from nuget.org
# Visit: https://www.nuget.org/account/apikeys

# Push package
dotnet nuget push GOKCafe.Umbraco.ProductCatalog.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Option 2: Publish to Private Feed (Azure Artifacts, GitHub Packages)

#### GitHub Packages

```bash
# Create Personal Access Token at github.com/settings/tokens
# Add package to GitHub

dotnet nuget add source \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/huythinh2507/index.json"

dotnet nuget push GOKCafe.Umbraco.ProductCatalog.1.0.0.nupkg \
  --source "github"
```

### Option 3: Umbraco Marketplace (For Discovery)

1. Create account at https://marketplace.umbraco.com/
2. Upload package
3. Add documentation, screenshots
4. Submit for review

---

## Package Installation for End Users

### End User Installation Steps

```bash
# 1. Install package
dotnet add package GOKCafe.Umbraco.ProductCatalog

# 2. Configure appsettings.json (see README)

# 3. Run migrations
dotnet ef database update

# 4. Start application
dotnet run

# 5. In Umbraco Backoffice:
#    - Settings > uSync > Import
#    - Create Homepage content
#    - Create Product List page
#    - View frontend
```

---

## Version Management

### Semantic Versioning

- **1.0.0** - Initial release
- **1.0.1** - Bug fixes
- **1.1.0** - New features (backward compatible)
- **2.0.0** - Breaking changes

### Update Package Version

In `.csproj`:
```xml
<Version>1.1.0</Version>
<PackageReleaseNotes>
  - Added discount badge support
  - Improved filter performance
  - Fixed pagination bug
</PackageReleaseNotes>
```

---

## Advanced: Multi-Package Strategy

### Complete Product Suite

1. **GOKCafe.Commerce** (Backend)
   - Services, DTOs, API Controllers
   - No UI dependencies

2. **GOKCafe.Umbraco.ProductCatalog** (Frontend)
   - Umbraco views and controllers
   - Depends on: GOKCafe.Commerce

3. **GOKCafe.Umbraco.Checkout** (Future)
   - Checkout flow views
   - Depends on: GOKCafe.Commerce

4. **GOKCafe.Umbraco.Admin** (Future)
   - Admin dashboard
   - Backoffice extensions

---

## Summary

### What You Get

✅ **Reusable Package**: Install in any Umbraco project
✅ **Complete Solution**: Backend + Frontend included
✅ **Auto-Configuration**: Composer handles setup
✅ **Document Types**: uSync exports included
✅ **Easy Updates**: NuGet package management
✅ **Customizable**: Override views and styles
✅ **Well-Documented**: README and guides included

### Next Steps

1. Create `GOKCafe.Umbraco.ProductCatalog` project
2. Move files from `GOKCafe.Web`
3. Update namespaces
4. Export document types via uSync
5. Build and test locally
6. Publish to NuGet
7. Install in clean Umbraco site to verify

---

## Questions & Answers

**Q: Can I include Tailwind CSS in the package?**
A: Yes! Include compiled CSS in `wwwroot/css/` or include Tailwind config for users to compile.

**Q: How do I handle database migrations?**
A: Use Umbraco migrations or Entity Framework migrations in the Commerce package.

**Q: Can I update views after package installation?**
A: Yes! Create same view path in consuming project to override package views.

**Q: Do I need to republish for view changes?**
A: No for consuming project. Yes for package updates.

**Q: How do I handle breaking changes?**
A: Bump major version (2.0.0) and document in release notes.

---

## Resources

- [Umbraco Package Documentation](https://docs.umbraco.com/umbraco-cms/extending/packages)
- [NuGet Package Creation](https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package)
- [uSync Documentation](https://docs.jumoo.co.uk/usync/)
- [Razor Class Libraries](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class)

---

**Created**: December 2024
**Version**: 1.0
**Author**: GOK Cafe Team
