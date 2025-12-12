# Changelog

All notable changes to Gotik Commerce will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2024-12-06

### ✨ Added - Document Types & Content Structure

This release adds complete Umbraco content structure to the package.

#### Document Types (via uSync)
- ✅ **Product Page** - Document type for individual product pages
- ✅ **Product List** - Document type for product listing pages
- ✅ **Category Pages** - Document type for category pages
- ✅ **Homepage** - Document type for homepage with e-commerce features
- ✅ **All Data Types** - Associated data types for content properties
- ✅ **Templates** - Razor templates for rendering content
- ✅ **Content Types** - Complete content structure definitions

#### Package Improvements
- ✅ **uSync Integration** - Automatic import of document types on package installation
- ✅ **Content Structure** - Ready-to-use content architecture
- ✅ **Backoffice Ready** - Create product pages directly from Umbraco backoffice
- ✅ **Complete Solution** - Now includes both API and CMS content structure

### 📦 What's New

Users can now:
1. Install the package
2. Run uSync import (automatic on first load)
3. Create product pages directly in Umbraco backoffice
4. Use pre-defined document types for e-commerce content
5. Get a complete working e-commerce system

### 🔧 Technical Changes

- Added uSync v16 configuration files
- Included ContentTypes, DataTypes, and Templates
- Package now includes complete content structure
- Compatible with Umbraco 16.3.4+ and uSync 16.0.0+

### 📊 Package Size

- Previous: 3.7 MB (API only)
- Current: ~4.5 MB (API + Content Structure)

---

## [1.0.0] - 2024-12-06

### 🎉 Initial Release

This is the first public release of Gotik Commerce - a complete e-commerce solution for Umbraco CMS.

### Added

#### Backend Features
- ✅ **Product Management**
  - Complete CRUD operations for products
  - Product image gallery support with primary image designation
  - Product categories with hierarchical structure
  - Product attributes (FlavourProfile, Equipment)
  - Stock management with reserved quantity tracking
  - Display order support for custom sorting
  - Soft delete functionality

- ✅ **Shopping Cart**
  - Session-based cart for anonymous users
  - User-authenticated cart for logged-in customers
  - Automatic cart expiration
  - Price snapshot at add-to-cart time
  - Cart item quantity management
  - Stock validation before checkout

- ✅ **Order Management**
  - Complete order lifecycle (Pending → Confirmed → Processing → Shipped → Delivered)
  - Order cancellation support
  - Multiple payment methods (Cash, Credit Card, Debit Card, Online Payment)
  - Payment status tracking (Pending, Paid, Failed, Refunded)
  - Shipping fee calculation
  - Tax calculation support
  - Order number generation
  - Guest checkout support

- ✅ **Dynamic Product Filtering**
  - Filter by multiple categories
  - Filter by flavour profiles
  - Filter by equipment
  - Stock availability filtering (in stock / out of stock)
  - Text search (product name and description)
  - Pagination with configurable page size
  - Filter count statistics

- ✅ **Category Management**
  - Hierarchical category structure
  - Category slugs for SEO-friendly URLs
  - Category descriptions
  - Active/inactive status

- ✅ **User Authentication**
  - JWT-based authentication
  - User registration
  - Secure login with password hashing (BCrypt)
  - Token refresh mechanism
  - Token revocation support
  - Role-based authorization (Customer, Staff, Admin)
  - User profile management

- ✅ **Odoo ERP Integration**
  - Optimized product synchronization
  - Support for 1M+ products
  - Batch processing for large datasets
  - N+1 query elimination
  - Memory-efficient processing
  - Automatic category creation
  - Product slug generation
  - Progress logging

#### Frontend Features
- ✅ **Homepage Components**
  - Featured products section
  - Category showcase
  - Hero section
  - About section
  - Mission section
  - Partner logos
  - Newsletter signup

- ✅ **Product List Page**
  - Responsive grid layout
  - Real-time filtering
  - Category filter
  - Flavour profile filter
  - Equipment filter
  - Stock availability filter
  - Search functionality
  - Pagination controls
  - Sort options
  - Filter count display

- ✅ **Product Detail Page**
  - Product image gallery with lightbox
  - Primary image display
  - Product information (name, price, description)
  - Stock availability status
  - Add to cart functionality
  - Quantity selector
  - Product attributes display
  - Related products (if configured)

- ✅ **Responsive Design**
  - Mobile-first approach
  - Tailwind CSS framework
  - Touch-friendly interfaces
  - Optimized for all screen sizes

#### API Endpoints
- ✅ **Products API** (`/api/v1/products`)
  - `GET /` - List products with filters
  - `GET /filters` - Get available filter options
  - `GET /{id}` - Get product details
  - `POST /` - Create product
  - `PUT /{id}` - Update product
  - `DELETE /{id}` - Delete product
  - `GET /odoo/fetch` - Preview Odoo products
  - `POST /odoo/sync` - Sync from Odoo

- ✅ **Cart API** (`/api/v1/cart`)
  - `GET /` - Get cart
  - `POST /items` - Add to cart
  - `PUT /items/{id}` - Update cart item
  - `DELETE /items/{id}` - Remove from cart
  - `DELETE /` - Clear cart
  - `GET /count` - Get item count
  - `POST /checkout` - Checkout

- ✅ **Orders API** (`/api/v1/orders`)
  - `GET /` - List all orders
  - `GET /my-orders` - Get user orders
  - `GET /{id}` - Get order details
  - `GET /number/{orderNumber}` - Get order by number
  - `POST /` - Create order
  - `PATCH /{id}/status` - Update status
  - `POST /{id}/cancel` - Cancel order

- ✅ **Categories API** (`/api/v1/categories`)
  - `GET /` - List categories
  - `GET /{id}` - Get category details
  - `POST /` - Create category
  - `PUT /{id}` - Update category
  - `DELETE /{id}` - Delete category

- ✅ **Authentication API** (`/api/v1/auth`)
  - `POST /register` - Register user
  - `POST /login` - User login
  - `POST /refresh` - Refresh token
  - `POST /logout` - User logout

#### Infrastructure
- ✅ **Architecture**
  - Clean architecture with layered structure
  - Repository pattern
  - Unit of Work pattern
  - Dependency injection throughout
  - Service-oriented design

- ✅ **Data Access**
  - Entity Framework Core 9.0
  - SQL Server support
  - Automatic migrations
  - Soft delete with global query filters
  - Audit trails (CreatedAt, UpdatedAt)

- ✅ **Performance**
  - Distributed memory caching
  - 30-minute cache TTL for product lists
  - Optimized database queries
  - Batch operations for large datasets
  - N+1 query prevention

- ✅ **Security**
  - JWT Bearer authentication
  - Password hashing with BCrypt
  - Role-based authorization
  - Token revocation
  - Secure session management

- ✅ **Developer Experience**
  - AutoMapper for DTO mapping
  - FluentValidation for data validation
  - Swagger API documentation support
  - Comprehensive error handling
  - Detailed logging

#### Umbraco Integration
- ✅ **Auto-Registration**
  - Composer for automatic service registration
  - No manual configuration required
  - Works out of the box

- ✅ **Backoffice Integration**
  - Custom dashboard
  - Package manifest
  - Admin-friendly interface

- ✅ **Document Types**
  - Homepage document type
  - Product List document type
  - Product Detail document type
  - Category document type

- ✅ **Render Controllers**
  - Homepage controller
  - Product List controller
  - Product Detail controller
  - Category controller

#### Package Features
- ✅ **NuGet Package**
  - Versioned releases
  - Symbol packages for debugging
  - Content file inclusion
  - Dependency management

- ✅ **Documentation**
  - Comprehensive README
  - Detailed installation guide
  - API documentation
  - Troubleshooting guide
  - Usage examples

- ✅ **Flexible Configuration**
  - Extension methods for DI
  - Multiple configuration options
  - Environment-specific settings
  - Optional Odoo integration

### Technical Details

**Dependencies:**
- Umbraco CMS 16.3.4
- Umbraco Commerce 16.0.0
- .NET 9.0
- Entity Framework Core 9.0
- AutoMapper 12.0.1
- FluentValidation 12.1.0
- JWT Bearer Authentication 9.0.0

**Database:**
- SQL Server 2016+
- 23 entity types
- 7 migrations included
- Soft delete support
- Audit trail support

**Performance Metrics:**
- Supports 1M+ products (via Odoo sync)
- 30-minute cache for product lists
- Optimized batch processing
- Memory-efficient operations

### Breaking Changes

None - this is the initial release.

### Known Issues

None identified at this time.

### Upgrade Path

None - this is the initial release.

## Future Releases

### Planned for v1.1.0
- Payment gateway integration (Stripe, PayPal)
- Email notification system
- Shipping method configuration
- Product reviews and ratings
- Wishlist functionality
- Product comparison
- Advanced reporting

### Planned for v1.2.0
- Multi-currency support
- Multi-language support
- Gift cards
- Discount codes and promotions
- Customer groups
- Inventory alerts
- Export functionality

### Planned for v2.0.0
- Marketplace features
- Vendor management
- Commission tracking
- Advanced analytics
- AI-powered recommendations
- Mobile app API

## Support

For issues, feature requests, or questions:
- GitHub Issues: https://github.com/huythinh2507/GOK_cafe/issues
- Documentation: [README.md](README.md)
- Installation Guide: [INSTALLATION.md](INSTALLATION.md)

## Contributors

- GOK Cafe Team
- Community Contributors (welcome!)

## License

MIT License - see LICENSE file for details

---

[1.0.0]: https://github.com/huythinh2507/GOK_cafe/releases/tag/v1.0.0
