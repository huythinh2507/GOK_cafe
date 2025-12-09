# GOK Cafe Commerce Backend Package - Usage Guide

## Package Information

- **Package ID**: `GOKCafe.Commerce`
- **Version**: 1.0.0
- **Package File**: `GOKCafe.Commerce.1.0.0.nupkg`
- **Symbols Package**: `GOKCafe.Commerce.1.0.0.snupkg`

## What's Included

### Services
- `ICartService` - Shopping cart management
- `IOrderService` - Order processing and management
- `IOdooService` - Odoo ERP integration with optimized sync (supports 1M+ products)

### Controllers (Ready to Use)
- `CartController` - Complete cart API endpoints
- `OrdersController` - Complete order management API endpoints

### DTOs
- Cart DTOs (CartDto, AddToCartDto, UpdateCartItemDto, CheckoutDto)
- Order DTOs (OrderDto, CreateOrderDto, etc.)
- Common DTOs (ApiResponse, PaginatedResponse)
- Odoo DTOs (OdooProductDto, OdooSyncResultDto)

### Features
- Session-based and user-based cart support
- Stock reservation during checkout
- Batch operations for performance
- Comprehensive error handling
- Logging integration

## Installation Options

### Option 1: Local Installation (For Development/Testing)

If your friend is working on the same machine or network:

```bash
# Add the local package source
dotnet nuget add source "D:\GOK_Cafe_BE\GOK_cafe\GOKCafe.Commerce.Package\nupkg" -n "GOKCafeLocal"

# Install the package in their project
dotnet add package GOKCafe.Commerce --version 1.0.0 --source GOKCafeLocal
```

### Option 2: File Share Installation

1. Copy the `.nupkg` file to a shared location
2. In their project:

```bash
dotnet add package GOKCafe.Commerce --version 1.0.0 --source "\shared\path\to\packages"
```

### Option 3: Private NuGet Feed (Recommended for Production)

1. **Using Azure Artifacts** (if you have Azure DevOps):
   ```bash
   az artifacts universal publish --organization https://dev.azure.com/yourorg \
     --feed yourfeed --name GOKCafe.Commerce --version 1.0.0 \
     --path ./GOKCafe.Commerce.1.0.0.nupkg
   ```

2. **Using GitHub Packages**:
   ```bash
   dotnet nuget push GOKCafe.Commerce.1.0.0.nupkg \
     --source "https://nuget.pkg.github.com/huythinh2507/index.json" \
     --api-key YOUR_GITHUB_TOKEN
   ```

3. **Using a simple file server** (easiest):
   - Host the `.nupkg` files on a web server or file share
   - Add as NuGet source

## Quick Start Guide

### 1. Install the Package

```bash
dotnet add package GOKCafe.Commerce --version 1.0.0
```

### 2. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GOKCafeDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long",
    "Issuer": "GOKCafe",
    "Audience": "GOKCafe"
  },
  "Odoo": {
    "Url": "https://your-odoo-instance.com",
    "Database": "your-database-name",
    "Username": "your-odoo-username",
    "ApiKey": "your-odoo-api-key"
  }
}
```

### 3. Register Services in Program.cs

```csharp
using GOKCafe.Commerce.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add GOK Cafe Commerce services
builder.Services.AddGOKCafeCommerce(builder.Configuration);

// Or for minimal setup (just cart and orders):
// builder.Services.AddGOKCafeCommerceCore(builder.Configuration);

// Or for Odoo integration only:
// builder.Services.AddGOKCafeOdooIntegration();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 4. Use the Controllers

The package includes ready-to-use controllers:

**Cart Endpoints** (from `CartController`):
- `GET /api/v1/cart` - Get cart
- `POST /api/v1/cart/items` - Add to cart
- `PUT /api/v1/cart/items/{id}` - Update cart item
- `DELETE /api/v1/cart/items/{id}` - Remove from cart
- `DELETE /api/v1/cart` - Clear cart
- `GET /api/v1/cart/count` - Get cart item count
- `POST /api/v1/cart/checkout` - Checkout

**Order Endpoints** (from `OrdersController`):
- `GET /api/v1/orders` - Get all orders (admin)
- `GET /api/v1/orders/my-orders` - Get user orders
- `GET /api/v1/orders/{id}` - Get order by ID
- `POST /api/v1/orders` - Create order
- `PATCH /api/v1/orders/{id}/status` - Update status
- `POST /api/v1/orders/{id}/cancel` - Cancel order

## Integration with Umbraco Package

### For Your Friend Creating the Umbraco Package:

Your backend package provides all the commerce logic. The Umbraco package should focus on:

1. **Frontend UI Components**:
   - Cart UI widget
   - Checkout form
   - Order history page
   - Product listing (using your APIs)

2. **Umbraco-specific Features**:
   - Document types for product management
   - Templates for product pages
   - Admin dashboard widgets

3. **API Integration**:
   - Use JavaScript/TypeScript to call your backend APIs
   - Example:

```typescript
// In Umbraco frontend
async function addToCart(productId: string, quantity: number) {
  const response = await fetch('/api/v1/cart/items', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      productId,
      quantity
    })
  });
  return await response.json();
}
```

### Hybrid Architecture

```
┌─────────────────────────────────────────────┐
│          Umbraco Frontend Package           │
│  ┌────────────┐  ┌──────────┐  ┌─────────┐ │
│  │   Cart UI  │  │ Checkout │  │  Orders │ │
│  └──────┬─────┘  └────┬─────┘  └────┬────┘ │
└─────────┼─────────────┼─────────────┼──────┘
          │             │             │
          │   HTTP/REST API Calls     │
          │             │             │
┌─────────▼─────────────▼─────────────▼──────┐
│       GOKCafe.Commerce (Your Package)       │
│  ┌────────────┐  ┌──────────┐  ┌─────────┐ │
│  │ CartService│  │OrderService│ │OdooService│
│  └────────────┘  └──────────┘  └─────────┘ │
└─────────────────────────────────────────────┘
```

## Testing the Package

### Test in a New Project

1. Create a test project:
```bash
dotnet new webapi -n TestCommerce
cd TestCommerce
dotnet add package GOKCafe.Commerce --version 1.0.0 --source "path/to/nupkg"
```

2. Configure and run:
```bash
# Update appsettings.json
dotnet run
```

3. Test endpoints:
```bash
# Test cart endpoint
curl http://localhost:5000/api/v1/cart?sessionId=test123

# Test add to cart
curl -X POST http://localhost:5000/api/v1/cart/items \
  -H "Content-Type: application/json" \
  -d '{"productId":"guid-here","quantity":1}'
```

## Updating the Package

When you make changes:

1. Update version in `.csproj`:
```xml
<Version>1.0.1</Version>
```

2. Rebuild and repack:
```bash
dotnet pack -c Release -o ./nupkg
```

3. Your friend updates:
```bash
dotnet add package GOKCafe.Commerce --version 1.0.1
```

## Troubleshooting

### Package Not Found
```bash
# List available sources
dotnet nuget list source

# Add source again
dotnet nuget add source "path/to/nupkg" -n "GOKCafeLocal"
```

### Dependency Issues
The package requires:
- .NET 8.0
- Entity Framework Core 8.x
- ASP.NET Core 8.0

Make sure the consuming project targets `net8.0`.

### Database Migration
The package uses existing Domain and Infrastructure projects. Run migrations from the Infrastructure project:
```bash
dotnet ef migrations add InitialCreate --project GOKCafe.Infrastructure
dotnet ef database update --project GOKCafe.Infrastructure
```

## Support

- GitHub: https://github.com/huythinh2507/GOK_cafe
- Issues: https://github.com/huythinh2507/GOK_cafe/issues

## License

MIT
