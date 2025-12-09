# Gotik Commerce - Complete E-Commerce Solution for Umbraco

[![NuGet](https://img.shields.io/nuget/v/Gotik.Commerce.svg)](https://www.nuget.org/packages/Gotik.Commerce/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Transform your Umbraco CMS into a powerful e-commerce platform with Gotik Commerce. Built with enterprise-grade architecture and clean code principles, this package provides everything you need to run a modern online store.

## ✨ Features

### Backend Services
- 🛒 **Shopping Cart Management** - Session-based and user-authenticated carts
- 📦 **Order Processing** - Complete order lifecycle from creation to fulfillment
- 🏷️ **Product Catalog** - Advanced product management with categories and attributes
- 🔍 **Dynamic Filtering** - Category, flavour profile, equipment, and stock filtering
- 🔗 **Odoo ERP Integration** - Optimized product synchronization supporting 1M+ products
- 🔐 **JWT Authentication** - Secure user authentication and authorization
- 📊 **RESTful APIs** - Complete API controllers ready to use

### Frontend Components
- 🏠 **Homepage** - Featured products and promotional sections
- 📋 **Product List** - Dynamic product listing with real-time filters
- 🔍 **Product Detail** - Detailed product information with image galleries
- 🎨 **Responsive Design** - Built with Tailwind CSS
- ⚡ **Client-Side Filtering** - Fast, interactive product filtering
- 📱 **Mobile Optimized** - Works perfectly on all devices

### Enterprise Features
- 💾 **Stock Management** - Real-time stock tracking with reservation
- 💰 **Payment Tracking** - Multiple payment methods and status tracking
- 📈 **Order Status Workflow** - Complete order lifecycle management
- 🗄️ **Distributed Caching** - Performance-optimized with caching
- 🔄 **Soft Delete** - Safe data deletion with recovery options
- 📝 **Audit Trails** - Automatic timestamp tracking
- 🔍 **Search Functionality** - Advanced product search

## 🚀 Quick Start

### Installation

```bash
dotnet add package Gotik.Commerce
```

### Database Setup

1. Update your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=.;Database=UmbracoDb;Integrated Security=true;",
    "GotikCommerceDb": "Server=.;Database=GotikCommerceDb;Integrated Security=true;"
  }
}
```

2. Run EF Core migrations:

```bash
dotnet ef database update --project Gotik.Commerce
```

### Configuration

Add the following to your `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "Your-Super-Secret-Key-Minimum-32-Characters-Long",
    "Issuer": "GotikCommerce",
    "Audience": "GotikCommerce",
    "ExpiryMinutes": 60
  },
  "Odoo": {
    "Url": "https://your-odoo-instance.com",
    "Database": "your-database",
    "Username": "api-user",
    "ApiKey": "your-api-key"
  }
}
```

### Auto-Registration

Gotik Commerce automatically registers all services when installed in Umbraco using the built-in Composer. No additional setup required!

## 📖 Usage

### API Endpoints

All API controllers are automatically registered at `/api/v1/`:

#### Products
```http
GET    /api/v1/products                    # List products with filters
GET    /api/v1/products/filters            # Get available filter options
GET    /api/v1/products/{id}               # Get product details
POST   /api/v1/products                    # Create product (admin)
PUT    /api/v1/products/{id}               # Update product (admin)
DELETE /api/v1/products/{id}               # Delete product (admin)
```

#### Shopping Cart
```http
GET    /api/v1/cart                        # Get cart
POST   /api/v1/cart/items                  # Add to cart
PUT    /api/v1/cart/items/{id}             # Update cart item
DELETE /api/v1/cart/items/{id}             # Remove from cart
DELETE /api/v1/cart                        # Clear cart
POST   /api/v1/cart/checkout               # Checkout
```

#### Orders
```http
GET    /api/v1/orders                      # List orders
GET    /api/v1/orders/my-orders            # Get user's orders
GET    /api/v1/orders/{id}                 # Get order details
POST   /api/v1/orders                      # Create order
PATCH  /api/v1/orders/{id}/status          # Update order status
POST   /api/v1/orders/{id}/cancel          # Cancel order
```

#### Categories
```http
GET    /api/v1/categories                  # List categories
GET    /api/v1/categories/{id}             # Get category details
POST   /api/v1/categories                  # Create category (admin)
PUT    /api/v1/categories/{id}             # Update category (admin)
DELETE /api/v1/categories/{id}             # Delete category (admin)
```

#### Authentication
```http
POST   /api/v1/auth/register               # Register new user
POST   /api/v1/auth/login                  # Login
POST   /api/v1/auth/refresh                # Refresh token
POST   /api/v1/auth/logout                 # Logout
```

### Using Services in Code

```csharp
using Gotik.Commerce.Controllers.Api;
using GOKCafe.Application.Services.Interfaces;

public class MyController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public MyController(
        IProductService productService,
        ICartService cartService)
    {
        _productService = productService;
        _cartService = cartService;
    }

    // Use the services
}
```

### Render Controllers

The package includes Umbraco render controllers for frontend pages:

- `HomepageController` - Homepage rendering
- `ProductListRenderController` - Product list with filters
- `ProductDetailPageController` - Product detail pages
- `CategoryRenderController` - Category pages

### Views

All views are included and can be customized:

```
Views/
├── Homepage.cshtml
├── ProductList.cshtml
├── ProductDetails.cshtml
└── Partials/
    ├── Homepage/
    ├── Products/
    ├── ProductDetail/
    └── Shared/
```

## 🎨 Customization

### Override Views

Create a file with the same path in your project to override package views:

```
/Views/Partials/Products/ProductsGrid.cshtml  (your custom version)
```

Your view will be used instead of the package version.

### Override Styles

The package uses Tailwind CSS. Override styles in your main CSS file:

```css
/* Override product card styling */
.product-card {
    /* Your custom styles */
}
```

### Extend Services

Inject and extend any service:

```csharp
public class MyCustomProductService : IProductService
{
    private readonly IProductService _baseService;

    public MyCustomProductService(IProductService baseService)
    {
        _baseService = baseService;
    }

    // Override or extend methods
}
```

## 🔧 Advanced Configuration

### Manual Service Registration (Optional)

If you need more control, you can manually register services in `Program.cs`:

```csharp
using Gotik.Commerce.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Option 1: Full package
builder.Services.AddGotikCommerce(builder.Configuration);

// Option 2: Core only (no Odoo)
builder.Services.AddGotikCommerceCore(builder.Configuration);

// Option 3: Odoo integration only
builder.Services.AddGotikOdooIntegration();
```

### Database Context

The package uses Entity Framework Core with SQL Server. Connection string name:
- Primary: `GotikCommerceDb`
- Fallback: `DefaultConnection`

### Caching Configuration

Distributed memory cache is used by default. Configure in `appsettings.json`:

```json
{
  "Caching": {
    "ProductListCacheDurationMinutes": 30
  }
}
```

### CORS Configuration

CORS policy `GotikCommercePolicy` is automatically registered. Apply in `Program.cs`:

```csharp
app.UseCors("GotikCommercePolicy");
```

## 📦 What's Included

### Controllers
- ✅ CartController
- ✅ OrdersController
- ✅ ProductsController
- ✅ CategoriesController
- ✅ AuthController
- ✅ HomepageController (Render)
- ✅ ProductListRenderController (Render)
- ✅ ProductDetailPageController (Render)

### Services
- ✅ IProductService
- ✅ ICartService
- ✅ IOrderService
- ✅ ICategoryService
- ✅ IOdooService
- ✅ IAuthService
- ✅ IPasswordHasher
- ✅ ICacheService

### Models
- ✅ Product, Category, ProductImage
- ✅ Cart, CartItem
- ✅ Order, OrderItem
- ✅ User, RevokedToken
- ✅ FlavourProfile, Equipment
- ✅ Complete DTO set

### Infrastructure
- ✅ Repository pattern
- ✅ Unit of Work
- ✅ Entity Framework Core
- ✅ AutoMapper
- ✅ FluentValidation

## 🔍 Features in Detail

### Dynamic Product Filtering

```csharp
// Filter by multiple criteria
var filters = new ProductFiltersDto
{
    CategoryIds = new List<Guid> { categoryId },
    FlavourProfileIds = new List<Guid> { flavourId },
    InStockOnly = true,
    SearchTerm = "coffee",
    PageNumber = 1,
    PageSize = 12
};

var products = await _productService.GetFilteredProductsAsync(filters);
```

### Shopping Cart

```csharp
// Add to cart (anonymous)
await _cartService.AddToCartAsync(sessionId, productId, quantity);

// Add to cart (authenticated)
await _cartService.AddToCartAsync(userId, productId, quantity);

// Checkout
var order = await _cartService.CheckoutAsync(sessionId, checkoutDto);
```

### Order Management

```csharp
// Create order
var order = await _orderService.CreateOrderAsync(createOrderDto);

// Update status
await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Shipped);

// Get user orders
var orders = await _orderService.GetUserOrdersAsync(userId);
```

### Odoo Integration

```csharp
// Sync products from Odoo
var result = await _odooService.SyncProductsAsync();

// Supports 1M+ products with optimized batch processing
```

## 🧪 Testing

The package has been tested with:
- ✅ Umbraco 16.3.4+
- ✅ .NET 9.0
- ✅ SQL Server 2016+
- ✅ Entity Framework Core 9.0+

## 📋 Requirements

- .NET 9.0 or higher
- Umbraco CMS 16.3.4 or higher
- SQL Server 2016 or higher
- Entity Framework Core 9.0 or higher

## 🤝 Support

- **Documentation**: See [INSTALLATION.md](INSTALLATION.md) for detailed setup
- **GitHub**: https://github.com/huythinh2507/GOK_cafe
- **Issues**: https://github.com/huythinh2507/GOK_cafe/issues
- **License**: MIT

## 📝 Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

## 🙏 Credits

Built with ❤️ by the GOK Cafe Team

Powered by:
- Umbraco CMS
- Entity Framework Core
- AutoMapper
- FluentValidation
- Tailwind CSS

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Made with ☕ by Gotik Commerce**
