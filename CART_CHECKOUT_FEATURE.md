# Cart & Checkout Feature Documentation

## Overview
This document describes the complete shopping cart and checkout functionality with stock reservation system implemented for GOK Cafe e-commerce platform.

## Features Implemented

### 1. Shopping Cart System ✅
Full-featured shopping cart supporting both authenticated and anonymous users.

**Endpoints:**
- `GET /api/v1/cart` - Get current cart
- `POST /api/v1/cart/items` - Add product to cart
- `PUT /api/v1/cart/items/{id}` - Update item quantity
- `DELETE /api/v1/cart/items/{id}` - Remove item from cart
- `DELETE /api/v1/cart` - Clear entire cart
- `GET /api/v1/cart/count` - Get total item count

**Features:**
- Supports authenticated users via JWT token
- Supports anonymous users via sessionId
- Real-time stock validation before adding items
- Auto-calculates cart totals
- 30-day cart expiration for abandoned carts

### 2. Checkout from Cart ✅ NEW!
Convert cart items directly to orders with automatic stock reservation.

**Endpoint:**
```
POST /api/v1/cart/checkout
```

**Request Body:**
```json
{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+1234567890",
  "shippingAddress": "123 Main St, City, Country",
  "notes": "Please deliver before 5 PM",
  "paymentMethod": "CreditCard",
  "shippingFee": 5.00
}
```

**Response:**
```json
{
  "success": true,
  "message": "Checkout successful! Order #ORD-20251127-001 has been created.",
  "data": {
    "id": "guid",
    "orderNumber": "ORD-20251127-001",
    "customerName": "John Doe",
    "totalAmount": 115.50,
    "status": "Pending",
    "items": [...]
  }
}
```

### 3. Stock Reservation System ✅ NEW!
Prevents overselling by reserving stock for pending orders.

**How it works:**

1. **Product Table** now includes:
   - `StockQuantity`: Total physical stock available
   - `ReservedQuantity`: Stock reserved for pending orders
   - **Available Stock** = StockQuantity - ReservedQuantity

2. **During Checkout:**
   - System validates available stock for each cart item
   - If sufficient, stock is reserved (ReservedQuantity increased)
   - Order is created with status "Pending"
   - Cart is cleared after successful checkout

3. **Stock Lifecycle:**
   ```
   Order Created → Reserve Stock (ReservedQuantity += quantity)
   Order Delivered → Deduct Stock (StockQuantity -= quantity, ReservedQuantity -= quantity)
   Order Cancelled → Release Stock (ReservedQuantity -= quantity)
   ```

## Example Scenarios

### Scenario 1: Customer tries to buy more than available
**Initial State:**
- Product A: StockQuantity = 10, ReservedQuantity = 0
- Customer 1 orders 8 items → ReservedQuantity = 8

**Result:**
- Available Stock = 10 - 8 = 2
- Customer 2 tries to order 5 items → **REJECTED** ❌
- Error: "Product A: Only 2 items available (you requested 5)"

### Scenario 2: Multiple customers ordering simultaneously
**Initial State:**
- Product A: StockQuantity = 10, ReservedQuantity = 0

**Timeline:**
1. Customer 1 adds 5 items to cart
2. Customer 2 adds 7 items to cart
3. Customer 1 checks out first → ReservedQuantity = 5 ✅
4. Customer 2 tries to checkout → **REJECTED** ❌ (only 5 available)

### Scenario 3: Order delivered
**Initial State:**
- Product A: StockQuantity = 10, ReservedQuantity = 5

**After Delivery:**
- StockQuantity = 10 - 5 = 5
- ReservedQuantity = 5 - 5 = 0
- Available Stock = 5

## API Usage Examples

### 1. Add items to cart
```bash
POST /api/v1/cart/items?sessionId=abc123
Content-Type: application/json

{
  "productId": "guid",
  "quantity": 2
}
```

### 2. View cart
```bash
GET /api/v1/cart?sessionId=abc123
```

### 3. Checkout
```bash
POST /api/v1/cart/checkout?sessionId=abc123
Content-Type: application/json

{
  "customerName": "Jane Smith",
  "customerEmail": "jane@example.com",
  "customerPhone": "+1234567890",
  "shippingAddress": "456 Oak Ave",
  "paymentMethod": "Cash",
  "shippingFee": 0
}
```

## Order Status Flow

```
Pending → Processing → Shipped → Delivered
                    ↘ Cancelled
```

**Stock Actions per Status:**
- **Pending**: Stock reserved (ReservedQuantity increased)
- **Processing**: Stock still reserved
- **Shipped**: Stock still reserved
- **Delivered**: Stock deducted (both StockQuantity and ReservedQuantity decreased)
- **Cancelled**: Stock released (ReservedQuantity decreased)

## Database Migration Required

**Migration:** `20251127041933_AddReservedQuantityToProduct`

Adds `ReservedQuantity` column to Products table:
```sql
ALTER TABLE Products ADD ReservedQuantity int NOT NULL DEFAULT 0;
```

**To apply:**
```bash
dotnet ef database update --project GOKCafe.Infrastructure --startup-project GOKCafe.API
```

## Payment Methods Supported

- `Cash` - Cash on delivery
- `CreditCard` - Credit card payment
- `DebitCard` - Debit card payment
- `OnlinePayment` - Online payment gateway

## Tax Calculation

Currently set to 10% of subtotal (configurable):
```csharp
var tax = subtotal * 0.10m;
var totalAmount = subtotal + tax + shippingFee;
```

## Error Handling

**Common Errors:**
1. `"Cart is empty"` - No items in cart during checkout
2. `"Only X items available"` - Insufficient stock
3. `"Product not found"` - Invalid product ID
4. `"Stock validation failed"` - Multiple products out of stock

## Testing Checklist

- [ ] Add product to cart as anonymous user
- [ ] Add product to cart as authenticated user
- [ ] Update cart item quantity
- [ ] Remove item from cart
- [ ] Clear cart
- [ ] Checkout with sufficient stock
- [ ] Checkout with insufficient stock
- [ ] Verify stock reservation after checkout
- [ ] Verify cart is cleared after successful checkout
- [ ] Test concurrent orders for same product
- [ ] Verify order creation with correct totals

## Future Enhancements

1. **Automatic Stock Release** - Release reserved stock after N days if order not processed
2. **Low Stock Alerts** - Notify admins when available stock is low
3. **Discount Codes** - Apply promo codes during checkout
4. **Multiple Payment Gateways** - Integrate Stripe, PayPal, etc.
5. **Wishlist** - Save items for later
6. **Order Tracking** - Real-time order status updates

## Notes

- Cart sessions expire after 30 days
- Anonymous users should provide a consistent sessionId (e.g., generated client-side)
- Authenticated users' carts are linked to their userId
- Stock is reserved using database transactions to prevent race conditions
- All monetary values use `decimal` type with 2 decimal precision

---

**Last Updated:** November 27, 2025
**Version:** 1.0
