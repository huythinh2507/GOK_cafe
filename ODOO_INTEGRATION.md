# Odoo Integration for GOK Cafe

## Overview
This integration allows GOK Cafe to fetch and synchronize products from an Odoo instance using the Odoo XML-RPC API.

## Configuration

### 1. Update appsettings.json
Add your Odoo credentials to `GOKCafe.API/appsettings.json`:

```json
"Odoo": {
  "Url": "https://your-odoo-instance.odoo.com",
  "Database": "your-database-name",
  "Username": "your-username",
  "ApiKey": "f676ec4c1dbb4c4a4486ebf0d4c4226e818c2b22"
}
```

**Important:** Replace the placeholder values with your actual Odoo credentials:
- `Url`: Your Odoo instance URL (e.g., `https://mycompany.odoo.com`)
- `Database`: Your Odoo database name
- `Username`: Your Odoo login email
- `ApiKey`: Your Odoo API key (already provided: `f676ec4c1dbb4c4a4486ebf0d4c4226e818c2b22`)

## API Endpoints

### 1. Fetch Products from Odoo (Preview)
**GET** `/api/v1/products/odoo/fetch`

This endpoint fetches products from Odoo but does NOT save them to the database. Use this to preview what products will be synced.

**Response Example:**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": 1,
      "name": "Coffee Beans - Premium",
      "description": "High-quality arabica beans",
      "price": 25.50,
      "stockQuantity": 100,
      "imageUrl": "data:image/png;base64,...",
      "categoryName": "Beverages",
      "active": true
    }
  ],
  "errors": []
}
```

### 2. Sync Products from Odoo
**POST** `/api/v1/products/odoo/sync`

This endpoint fetches products from Odoo and saves/updates them in the GOK Cafe database.

**Response Example:**
```json
{
  "success": true,
  "message": "Products synchronized successfully",
  "data": {
    "totalFetched": 50,
    "created": 30,
    "updated": 20,
    "skipped": 0,
    "errors": []
  },
  "errors": []
}
```

## How It Works

### Product Synchronization Logic

1. **Authentication**: The service authenticates with Odoo using your API key
2. **Fetch Products**: Products are fetched from Odoo's `product.product` model with the following fields:
   - `id`: Odoo product ID
   - `name`: Product name
   - `description`: Product description
   - `list_price`: Product price
   - `qty_available`: Available stock quantity
   - `image_1920`: Product image (base64 encoded)
   - `categ_id`: Category name
   - `active`: Product status

3. **Category Mapping**: Products are assigned to an "Odoo Products" category (created automatically if it doesn't exist)

4. **Update Logic**:
   - **New Products**: If a product doesn't exist (matched by slug/name), it's created
   - **Existing Products**: If a product exists, its data is updated (price, stock, description, etc.)

5. **Slug Generation**: Product names are converted to URL-friendly slugs (e.g., "Coffee Beans" → "coffee-beans")

## Usage Examples

### Using cURL

#### Fetch Products (Preview):
```bash
curl -X GET "https://localhost:7001/api/v1/products/odoo/fetch"
```

#### Sync Products:
```bash
curl -X POST "https://localhost:7001/api/v1/products/odoo/sync"
```

### Using Swagger
1. Navigate to `https://localhost:7001/swagger`
2. Find the "Products" section
3. Use the `/api/v1/products/odoo/fetch` or `/api/v1/products/odoo/sync` endpoints

## Features

- ✅ Fetch products from Odoo
- ✅ Create new products in GOK Cafe database
- ✅ Update existing products
- ✅ Map Odoo categories
- ✅ Handle product images (base64 encoded)
- ✅ Maintain stock quantities
- ✅ Error handling and logging
- ✅ Preview mode (fetch without saving)
- ✅ Detailed sync statistics

## Error Handling

The integration includes comprehensive error handling:
- Authentication failures
- Network errors
- Invalid product data
- Database errors

All errors are logged and returned in the API response.

## Database Schema

Products synced from Odoo are stored in the `Products` table with:
- `Id`: GUID (generated)
- `Name`: From Odoo
- `Description`: From Odoo
- `Slug`: Auto-generated
- `Price`: From Odoo `list_price`
- `StockQuantity`: From Odoo `qty_available`
- `ImageUrl`: From Odoo `image_1920`
- `CategoryId`: "Odoo Products" category
- `IsActive`: From Odoo `active`
- `IsFeatured`: false (default)
- `DisplayOrder`: 0 (default)

## Troubleshooting

### Authentication Failed
- Verify your Odoo URL, database name, username, and API key
- Ensure your Odoo user has API access enabled

### No Products Returned
- Check that products exist in your Odoo instance
- Verify the `product.product` model is accessible

### Sync Errors
- Check the API response for specific error messages
- Review application logs for detailed error information

## Next Steps

To fully configure the integration:
1. Update the Odoo credentials in `appsettings.json`
2. Test the `/fetch` endpoint to preview products
3. Run the `/sync` endpoint to import products
4. Verify products appear in GOK Cafe

## Security Notes

- Store API keys securely (consider using environment variables or Azure Key Vault)
- Use HTTPS for all Odoo communication
- Implement authentication/authorization for the sync endpoints in production
