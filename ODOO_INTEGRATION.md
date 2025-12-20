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
},
"OdooAttributeMapping": {
  "EnableAutoMapping": true,
  "DefaultProductType": "General",
  "CategoryToProductTypeMap": {
    "All / Saleable": "General",
    "Beverages": "Coffee",
    "Clothing": "Clothes",
    "Food": "Food"
  },
  "AttributeMapping": {
    "Coffee": {
      "Weight": "size",
      "Size": "size",
      "Grind": "grind",
      "Grind Type": "grind"
    },
    "Clothes": {
      "Size": "size",
      "Color": "color",
      "Colour": "color"
    }
  }
}
```

**Important:** Replace the placeholder values with your actual Odoo credentials:
- `Url`: Your Odoo instance URL (e.g., `https://mycompany.odoo.com`)
- `Database`: Your Odoo database name
- `Username`: Your Odoo login email
- `ApiKey`: Your Odoo API key (already provided: `f676ec4c1dbb4c4a4486ebf0d4c4226e818c2b22`)

### 2. Configure Attribute Mapping (Optional)

The `OdooAttributeMapping` configuration controls how Odoo product attributes are mapped to your dynamic ProductType system:

- **`EnableAutoMapping`**: Set to `true` to automatically map Odoo attributes to ProductTypes
- **`DefaultProductType`**: The ProductType to use when no category mapping is found
- **`CategoryToProductTypeMap`**: Maps Odoo category names to ProductType names
- **`AttributeMapping`**: Maps Odoo attribute names to your system's attribute names for each ProductType

**Example**: If an Odoo product has:
- Category: "Beverages" → Maps to ProductType "Coffee"
- Attribute "Weight: 250g" → Maps to ProductAttribute "size" with value "250g"

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
   - `description_sale`: Product description
   - `list_price`: Product price
   - `qty_available`: Available stock quantity
   - `image_1920`: Product image (base64 encoded)
   - `categ_id`: Category name
   - `product_template_attribute_value_ids`: Product variant attributes (Size, Color, etc.)
   - `active`: Product status

3. **Attribute Resolution**: For products with attributes:
   - Fetches attribute details from `product.template.attribute.value` model
   - Resolves attribute IDs to actual attribute names and values
   - Example: `[23]` → `{"Size": "Medium", "Color": "Blue"}`

4. **Category & ProductType Mapping**:
   - Maps Odoo category to ProductType based on `CategoryToProductTypeMap` configuration
   - Creates ProductType if it doesn't exist
   - Assigns products to both:
     - **Category**: "Odoo Products" (for filtering in the app)
     - **ProductType**: Based on Odoo category (for dynamic attributes)

5. **Attribute Mapping**:
   - For each Odoo attribute (e.g., "Weight: 250g"):
     - Maps attribute name using `AttributeMapping` config (e.g., "Weight" → "size")
     - Creates ProductAttribute if it doesn't exist
     - Creates ProductAttributeValue if it doesn't exist
     - Creates ProductAttributeSelection linking product to the attribute value
   - All attribute entities are auto-created with proper relationships

6. **Update Logic**:
   - **New Products**: If a product doesn't exist (matched by slug/name), it's created with all attributes
   - **Existing Products**: If a product exists, its data is updated including:
     - Basic fields (price, stock, description, etc.)
     - ProductType assignment
     - Attribute selections (cleared and re-created)

7. **Slug Generation**: Product names are converted to URL-friendly slugs (e.g., "Coffee Beans" → "coffee-beans")

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
- ✅ Map Odoo categories to ProductTypes
- ✅ **Automatic attribute mapping** - Odoo product variants → Dynamic ProductAttributeSelections
- ✅ **Auto-create ProductTypes** from Odoo categories
- ✅ **Auto-create ProductAttributes** and values from Odoo attributes
- ✅ Configurable category and attribute mappings
- ✅ Handle product images (base64 encoded)
- ✅ Maintain stock quantities
- ✅ Error handling and logging
- ✅ Preview mode (fetch without saving)
- ✅ Detailed sync statistics
- ✅ Batch processing for large datasets (1M+ products)

## Error Handling

The integration includes comprehensive error handling:
- Authentication failures
- Network errors
- Invalid product data
- Database errors

All errors are logged and returned in the API response.

## Database Schema

### Products Table
Products synced from Odoo are stored in the `Products` table with:
- `Id`: GUID (generated)
- `Name`: From Odoo
- `Description`: From Odoo `description_sale`
- `Slug`: Auto-generated
- `Price`: From Odoo `list_price`
- `StockQuantity`: From Odoo `qty_available`
- `ImageUrl`: From Odoo `image_1920`
- `CategoryId`: "Odoo Products" category
- **`ProductTypeId`**: Auto-assigned based on Odoo category mapping
- `IsActive`: From Odoo `active`
- `IsFeatured`: false (default)
- `DisplayOrder`: 0 (default)

### Dynamic Attribute System
When `EnableAutoMapping` is `true`, the sync automatically creates and manages:

1. **ProductTypes** (`ProductTypes` table):
   - Auto-created from Odoo categories
   - Example: "Beverages" → "Coffee" ProductType

2. **ProductAttributes** (`ProductAttributes` table):
   - Auto-created for each unique Odoo attribute per ProductType
   - Example: "Weight" attribute for "Coffee" ProductType

3. **ProductAttributeValues** (`ProductAttributeValues` table):
   - Auto-created for each unique value per attribute
   - Example: "250g", "500g", "1kg" for "Weight" attribute

4. **ProductAttributeSelections** (`ProductAttributeSelections` table):
   - Links products to their specific attribute values
   - Example: Product "Ethiopia Coffee" → Size: "250g", Grind: "Espresso"

All entities are managed automatically - no manual setup required!

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
