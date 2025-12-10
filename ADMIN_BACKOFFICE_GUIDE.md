# GOK Cafe Admin Backoffice Guide

## Overview
The GOK Cafe Admin Backoffice is a comprehensive product management system that allows administrators to manage the entire coffee product catalog through an intuitive web interface.

## Accessing the Admin Panel

### URL
```
https://localhost:44317/admin
```
or
```
http://localhost:25718/admin
```

## Features

### 1. Dashboard (`/admin/dashboard`)
- **Quick Statistics**: View total products, orders, revenue, and customers at a glance
- **Quick Actions**: Direct links to common tasks
- **System Information**: Monitor system status and components

### 2. Products Management (`/admin/products`)

#### Product List Page
- **Search & Filter**: Search products by name, filter by category, and stock status
- **Sortable Table**: View all products with:
  - Product image
  - Name and SKU
  - Categories
  - Price and discount price
  - Stock quantity
  - Status (Featured, In Stock)
- **Quick Actions**: Edit or delete products directly from the list
- **Pagination**: Navigate through large product catalogs

#### Create Product (`/admin/products/create`)
Create new products with complete information:
- **Basic Information**:
  - Product name
  - Full description
  - Short description
- **Pricing & Inventory**:
  - Regular price (VND)
  - Discount price (optional)
  - Stock quantity
  - SKU code
- **Product Options**:
  - Available sizes (e.g., 200g, 340g, 500g, 1kg)
  - Available grinds (e.g., Whole Bean, Espresso, Filter)
- **Category & Status**:
  - Multiple category selection
  - Featured product toggle
- **Product Image**:
  - Image URL with live preview

#### Edit Product (`/admin/products/edit/{id}`)
- Update all product information
- Pre-filled form with existing data
- Live image preview

### 3. Navigation Menu
- **Dashboard**: Overview and statistics
- **Products**: Full product management (ACTIVE)
- **Categories**: Category management (future)
- **Orders**: Order management (future)
- **Coupons**: Discount coupon management (future)
- **Customers**: Customer management (future)
- **Settings**: System settings
- **Umbraco**: Access Umbraco CMS backend

## Technical Architecture

### Backend APIs
All admin functions use the existing GOKCafe.API REST endpoints:

#### Products API (`https://localhost:7045/api/v1/products`)
- `GET /api/v1/products` - List products with pagination and filters
- `GET /api/v1/products/{id}` - Get product by ID
- `POST /api/v1/products` - Create new product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Frontend Components

#### Controllers
- `ProductsAdminController.cs` - Handles admin product pages
- `DashboardController.cs` - Handles dashboard page

#### Views
- `Views/Admin/Products/Index.cshtml` - Product list page
- `Views/Admin/Products/Create.cshtml` - Create product form
- `Views/Admin/Products/Edit.cshtml` - Edit product form
- `Views/Admin/Dashboard/Index.cshtml` - Dashboard page
- `Views/Shared/_AdminLayout.cshtml` - Admin layout with navigation

#### JavaScript
- `wwwroot/js/admin/products.js` - Product list functionality (load, filter, delete)
- `wwwroot/js/admin/product-form.js` - Create/Edit form handling

## How to Run

### Option 1: Run Both Projects
1. **Start the API** (GOKCafe.API):
   ```bash
   cd GOKCafe.API
   dotnet run
   ```
   API will run on: `https://localhost:7045`

2. **Start the Web** (GOKCafe.Web):
   ```bash
   cd GOKCafe.Web
   dotnet run
   ```
   Web will run on: `https://localhost:44317`

3. **Access Admin**:
   Navigate to: `https://localhost:44317/admin`

### Option 2: Use Visual Studio
1. Set **Multiple Startup Projects**:
   - Right-click solution → Properties
   - Select "Multiple startup projects"
   - Set both `GOKCafe.API` and `GOKCafe.Web` to "Start"
2. Press F5 to run both projects

## Screenshots Guide

To show your boss the admin backoffice, take screenshots of:

1. **Dashboard** (`/admin/dashboard`)
   - Shows overview and quick stats

2. **Product List** (`/admin/products`)
   - Shows all products in a table with filters

3. **Create Product Form** (`/admin/products/create`)
   - Shows the comprehensive product creation form

4. **Edit Product Form** (`/admin/products/edit/{id}`)
   - Shows how to edit existing products

## Key Features Highlights

✅ **Fully API-Driven**: All data comes from the backend REST API
✅ **Responsive Design**: Works on desktop and tablet
✅ **Real-time Preview**: Image preview when entering URLs
✅ **Advanced Filtering**: Search, category, and stock filters
✅ **Pagination**: Handle large product catalogs efficiently
✅ **Multiple Categories**: Assign products to multiple categories
✅ **Product Options**: Support for sizes and grind types
✅ **Discount Pricing**: Set sale prices for products
✅ **Stock Management**: Track inventory quantities
✅ **Featured Products**: Mark products for homepage display

## Integration with Existing Systems

- **Database**: Uses the same Azure SQL database as the main application
- **API**: Leverages all existing Product API endpoints
- **Authentication**: Can be integrated with existing auth system
- **Umbraco**: Direct link to Umbraco CMS for content management

## Future Enhancements

The admin panel is designed to be extended with:
- Category management pages
- Order management interface
- Coupon/discount management
- Customer management
- Analytics and reporting
- Bulk product import/export
- Image upload functionality

## Support

For technical questions or issues:
- Check API documentation at: `https://localhost:7045/swagger`
- Review application logs
- Contact development team
