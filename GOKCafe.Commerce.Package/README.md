# GOK Cafe Commerce Backend Package

Backend commerce services for GOK Cafe e-commerce system including Cart, Order, Product management, and Odoo ERP integration.

## Features

- **Shopping Cart Management**: Session-based and user-based cart handling
- **Order Management**: Complete order lifecycle from creation to fulfillment
- **Product Management**: Product catalog with categories, filters, and search
- **Odoo Integration**: Optimized product synchronization supporting 1M+ products
- **Authentication**: JWT-based user authentication
- **RESTful APIs**: Complete API controllers ready to use

## Installation

```bash
dotnet add package GOKCafe.Commerce
```

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using GOKCafe.Commerce.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add GOK Cafe Commerce services
builder.Services.AddGOKCafeCommerce(builder.Configuration);

var app = builder.Build();
app.Run();
```

### 2. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your-Database-Connection-String"
  },
  "Jwt": {
    "Key": "Your-Secret-Key",
    "Issuer": "Your-Issuer",
    "Audience": "Your-Audience"
  },
  "Odoo": {
    "Url": "https://your-odoo-instance.com",
    "Database": "your-database",
    "Username": "your-username",
    "ApiKey": "your-api-key"
  }
}
```

### 3. Use the Services

```csharp
public class MyController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    
    public MyController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }
    
    // Your endpoints here
}
```

## API Controllers Included

- `CartController`: Shopping cart operations
- `OrdersController`: Order management
- `ProductsController`: Product catalog
- `CategoriesController`: Category management
- `AuthController`: User authentication

## Services

- `ICartService`: Cart operations
- `IOrderService`: Order management
- `IProductService`: Product operations
- `ICategoryService`: Category operations
- `IOdooService`: Odoo integration
- `IAuthService`: Authentication

## Compatibility

- .NET 8.0+
- Entity Framework Core 8.0+
- Compatible with Umbraco CMS
- Works with any .NET Web API project

## License

MIT

## Support

- GitHub: https://github.com/huythinh2507/GOK_cafe
- Issues: https://github.com/huythinh2507/GOK_cafe/issues
