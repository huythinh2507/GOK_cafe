# GOK Cafe - Backend API

A comprehensive .NET 8.0 Web API for GOK Cafe management system built with Clean Architecture principles.

## Architecture

This project follows Clean Architecture with the following layers:

- **GOKCafe.Domain** - Core business entities and interfaces
- **GOKCafe.Infrastructure** - Data access with Entity Framework Core
- **GOKCafe.Application** - Business logic and services
- **GOKCafe.API** - RESTful API endpoints

## Features

- RESTful API with Swagger/OpenAPI documentation
- Entity Framework Core 8.0 with SQL Server
- Repository and Unit of Work patterns
- Soft delete functionality
- Pagination support
- Transaction management for orders
- Inventory tracking
- CORS enabled for frontend integration

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or Full version)
- Visual Studio Code with C# Dev Kit (or Visual Studio 2022)

## Database Setup

The database is configured to use SQL Server with the following connection string:

```
Server=localhost;Database=GOKCafeDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### Database Tables

- **Categories** - Product categories (Coffee, Tea, etc.)
- **Products** - Coffee and tea products
- **ProductImages** - Multiple images per product
- **Users** - Customer and admin users
- **Orders** - Customer orders
- **OrderItems** - Order line items
- **Offers** - Special offers and promotions
- **Partners** - Business partners
- **ContactMessages** - Contact form submissions

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd GOK_cafe
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Update Database Connection String (if needed)

Edit `GOKCafe.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  }
}
```

### 4. Run the Application

The database will be created and seeded automatically on first run:

```bash
cd GOKCafe.API
dotnet run
```

The API will start at: **http://localhost:5142**

### 5. Access Swagger UI

Open your browser and navigate to:

**http://localhost:5142/swagger**

## Sample Data

The application automatically seeds the database with sample data including:

### Categories
- Coffee
- Tea
- Specialty Drinks

### Products
- Natural Cold Brew Coffee (Featured)
- Espresso (Featured)
- Cappuccino (Featured)
- Latte
- Green Tea (Featured)
- Chamomile Tea
- Earl Grey Tea (Featured)
- Iced Matcha Latte (Featured)

### Offers
- Morning Special (20% off)
- Tea Time Deal
- Cold Brew Summer Special

### Partners
- Arabica Beans Co.
- Tea Masters
- Organic Valley

## API Endpoints

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `GET /api/categories/slug/{slug}` - Get category by slug
- `POST /api/categories` - Create new category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Products
- `GET /api/products` - Get all products (paginated)
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/slug/{slug}` - Get product by slug
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

Query Parameters:
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)
- `categoryId` - Filter by category
- `isFeatured` - Filter featured products

### Orders
- `GET /api/orders` - Get all orders (paginated)
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/number/{orderNumber}` - Get order by number
- `POST /api/orders` - Create new order
- `PATCH /api/orders/{id}/status` - Update order status
- `POST /api/orders/{id}/cancel` - Cancel order

## Testing the API

### Using Swagger UI

1. Navigate to http://localhost:5142/swagger
2. Expand any endpoint
3. Click "Try it out"
4. Fill in parameters
5. Click "Execute"

### Using .http File

Open `GOKCafe.API/GOKCafe.API.http` in VS Code with C# Dev Kit installed:

1. Click "Send Request" above any HTTP request
2. View the response in the output panel

### Example: Get All Products

```http
GET http://localhost:5142/api/products?pageNumber=1&pageSize=10
```

### Example: Create Order

```http
POST http://localhost:5142/api/orders
Content-Type: application/json

{
  "customerName": "John Doe",
  "customerEmail": "john.doe@example.com",
  "customerPhone": "+1234567890",
  "shippingAddress": "123 Main St, New York, NY 10001",
  "paymentMethod": "CreditCard",
  "items": [
    {
      "productId": "guid-from-products-endpoint",
      "quantity": 2
    }
  ]
}
```

## Debugging

### Using C# Dev Kit in VS Code

1. Open the project in VS Code
2. Press `F5` or click Run → Start Debugging
3. Set breakpoints by clicking in the left margin
4. The debugger will attach automatically

### Useful Shortcuts
- `F5` - Start/Continue debugging
- `F9` - Toggle breakpoint
- `F10` - Step over
- `F11` - Step into
- `Shift+F5` - Stop debugging

## Database Migrations

### Create a new migration

```bash
dotnet ef migrations add MigrationName -p GOKCafe.Infrastructure -s GOKCafe.API
```

### Apply migrations

```bash
dotnet ef database update -p GOKCafe.Infrastructure -s GOKCafe.API
```

### Remove last migration

```bash
dotnet ef migrations remove -p GOKCafe.Infrastructure -s GOKCafe.API
```

## Project Structure

```
GOKCafe/
├── GOKCafe.API/              # Web API layer
│   ├── Controllers/          # API controllers
│   ├── Program.cs           # Application entry point
│   └── appsettings.json     # Configuration
│
├── GOKCafe.Application/      # Business logic layer
│   ├── DTOs/                # Data transfer objects
│   └── Services/            # Business services
│
├── GOKCafe.Domain/           # Core domain layer
│   ├── Entities/            # Domain entities
│   └── Interfaces/          # Repository interfaces
│
└── GOKCafe.Infrastructure/   # Data access layer
    ├── Data/                # DbContext and seeding
    ├── Configurations/      # EF Core configurations
    └── Repositories/        # Repository implementations
```

## Technologies Used

- .NET 8.0
- Entity Framework Core 8.0
- SQL Server
- Swagger/OpenAPI
- Clean Architecture
- Repository Pattern
- Unit of Work Pattern

## Contributing

1. Create a feature branch
2. Make your changes
3. Submit a pull request

## License

This project is proprietary and confidential.
