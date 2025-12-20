# Loyalty Platform Integration - Usage Examples

This document provides practical examples of using the Loyalty Platform integration.

## Prerequisites

1. Loyalty Platform is running (e.g., at http://localhost:8079)
2. GOK Cafe API is running
3. You have an Admin JWT token for authentication

## Example 1: First-Time Setup

### Step 1: Configure the Loyalty Platform URL

Edit `appsettings.Development.json`:
```json
{
  "LoyaltyPlatform": {
    "Url": "http://localhost:8079",
    "ApiKey": ""
  }
}
```

### Step 2: Preview Available Vouchers

**Request:**
```http
GET http://localhost:5000/api/v1/loyaltyplatform/vouchers/fetch
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response:**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": "v001",
      "code": "WELCOME10",
      "name": "Welcome Discount",
      "description": "10% off for new customers",
      "type": "percentage",
      "value": 10,
      "maxDiscount": 50,
      "minOrderAmount": 100,
      "isActive": true,
      "isSystemWide": true
    },
    {
      "id": "v002",
      "code": "FREESHIP",
      "name": "Free Shipping",
      "description": "Free shipping on orders over $50",
      "type": "fixed",
      "value": 15,
      "minOrderAmount": 50,
      "isActive": true,
      "isSystemWide": true
    }
  ],
  "errors": null
}
```

### Step 3: Sync Vouchers to GOK Cafe

**Request:**
```http
POST http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Vouchers synchronized successfully",
  "data": {
    "totalFetched": 2,
    "created": 2,
    "updated": 0,
    "skipped": 0,
    "errors": []
  },
  "errors": null
}
```

### Step 4: Verify Coupons Were Created

**Request:**
```http
GET http://localhost:5000/api/v1/coupons/code/WELCOME10
```

**Expected Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid-here",
    "code": "WELCOME10",
    "name": "Welcome Discount",
    "description": "10% off for new customers",
    "type": 1,
    "typeDisplay": "One-Time Use",
    "discountType": 1,
    "discountValue": 10,
    "maxDiscountAmount": 50,
    "minOrderAmount": 100,
    "isSystemCoupon": true,
    "isActive": true,
    "canBeUsed": true
  }
}
```

## Example 2: User-Specific Vouchers

### Scenario
Loyalty platform generates personal vouchers for user "abc123-def456-..." (e.g., birthday discount)

### Step 1: Fetch User Vouchers from Loyalty Platform

**Request:**
```http
GET http://localhost:5000/api/v1/loyaltyplatform/vouchers/user/abc123-def456-...
Authorization: Bearer {admin-token}
```

**Expected Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "v100",
      "code": "BDAY2025",
      "name": "Birthday Bonus",
      "description": "Happy birthday! Enjoy 20% off",
      "type": "percentage",
      "value": 20,
      "maxDiscount": 100,
      "targetUserId": "abc123-def456-...",
      "isSystemWide": false,
      "isActive": true
    }
  ]
}
```

### Step 2: Sync User Vouchers

**Request:**
```http
POST http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync/user/abc123-def456-...
Authorization: Bearer {admin-token}
```

**Response:**
```json
{
  "success": true,
  "message": "User vouchers synchronized successfully",
  "data": {
    "totalFetched": 1,
    "created": 1,
    "updated": 0,
    "skipped": 0,
    "errors": []
  }
}
```

### Step 3: User Gets Their Personal Coupons

**Request:**
```http
GET http://localhost:5000/api/v1/coupons/user/abc123-def456-...
Authorization: Bearer {user-token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "code": "BDAY2025",
        "name": "Birthday Bonus",
        "description": "Happy birthday! Enjoy 20% off",
        "discountValue": 20,
        "isSystemCoupon": false,
        "canBeUsed": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 1
  }
}
```

## Example 3: Complete Purchase Flow with Loyalty Voucher

### Scenario
Customer wants to buy coffee worth $150 and apply WELCOME10 voucher

### Step 1: Add Items to Cart

**Request:**
```http
POST http://localhost:5000/api/v1/cart/items
Content-Type: application/json
Authorization: Bearer {user-token}

{
  "productId": "product-guid",
  "quantity": 3,
  "selectedSize": "500g",
  "selectedGrind": "Espresso"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "subtotal": 150,
    "discountAmount": 0,
    "total": 150,
    "cartItems": [...]
  }
}
```

### Step 2: Validate Coupon

**Request:**
```http
POST http://localhost:5000/api/v1/coupons/validate
Content-Type: application/json

{
  "couponCode": "WELCOME10",
  "orderAmount": 150,
  "userId": "user-guid"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "message": "Coupon is valid",
    "coupon": {
      "code": "WELCOME10",
      "name": "Welcome Discount",
      "discountValue": 10,
      "discountType": 1
    },
    "estimatedDiscount": 15
  }
}
```

### Step 3: Apply Coupon to Cart

**Request:**
```http
POST http://localhost:5000/api/v1/cart/apply-coupon
Content-Type: application/json
Authorization: Bearer {user-token}

{
  "couponCode": "WELCOME10"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Coupon applied successfully",
  "data": {
    "subtotal": 150,
    "discountAmount": 15,
    "total": 135,
    "appliedCouponCode": "WELCOME10"
  }
}
```

### Step 4: Checkout

**Request:**
```http
POST http://localhost:5000/api/v1/cart/checkout
Content-Type: application/json
Authorization: Bearer {user-token}

{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "123-456-7890",
  "shippingAddress": "123 Main St",
  "paymentMethod": 3
}
```

**Response:**
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "orderNumber": "ORD-20251211-001",
    "subTotal": 150,
    "discountAmount": 15,
    "totalAmount": 135,
    "status": 0,
    "paymentStatus": 0
  }
}
```

### Result
- Customer saved $15 (10% of $150)
- Coupon usage was tracked in `CouponUsage` table
- Coupon `UsageCount` was incremented
- If it was a one-time coupon, `IsUsed` was set to `true`

## Example 4: Gradual/Recurring Voucher

### Scenario
Loyalty platform gives user a $100 gradual voucher that can be used across multiple orders

### Step 1: Loyalty Platform Creates Gradual Voucher
```json
{
  "code": "LOYALTY100",
  "name": "Loyalty Reward",
  "type": "gradual",
  "value": 100,
  "remainingBalance": 100,
  "targetUserId": "user-guid",
  "isSystemWide": false
}
```

### Step 2: Sync to GOK Cafe
```http
POST http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync/user/{user-guid}
```

### Step 3: First Purchase ($60)

**Apply Coupon:**
```http
POST http://localhost:5000/api/v1/coupons/apply
Content-Type: application/json

{
  "couponCode": "LOYALTY100",
  "orderAmount": 60,
  "userId": "user-guid"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "originalAmount": 60,
    "discountAmount": 60,
    "finalAmount": 0,
    "remainingBalance": 40,
    "noticeMessage": "This is a gradual coupon. Remaining balance: $40"
  }
}
```

### Step 4: Second Purchase ($50)

**Apply Same Coupon:**
```http
POST http://localhost:5000/api/v1/coupons/apply
Content-Type: application/json

{
  "couponCode": "LOYALTY100",
  "orderAmount": 50,
  "userId": "user-guid"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "originalAmount": 50,
    "discountAmount": 40,
    "finalAmount": 10,
    "remainingBalance": 0,
    "noticeMessage": "This is a gradual coupon. Balance depleted."
  }
}
```

### Result
- First order: Used $60 from voucher, $40 remaining
- Second order: Used remaining $40, customer pays $10
- Voucher is now depleted and cannot be used again

## Example 5: Scheduled Sync (Background Job)

### Using Manual Trigger

You can set up a Windows Task Scheduler or cron job to periodically call the sync endpoint:

**PowerShell Script (Windows):**
```powershell
# sync-loyalty-vouchers.ps1

$apiUrl = "http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync"
$adminToken = "your-admin-jwt-token"

$headers = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers
    Write-Host "Sync completed successfully"
    Write-Host "Created: $($response.data.created)"
    Write-Host "Updated: $($response.data.updated)"
    Write-Host "Skipped: $($response.data.skipped)"
} catch {
    Write-Host "Sync failed: $_"
}
```

**Schedule in Windows Task Scheduler:**
```
Action: Start a program
Program: powershell.exe
Arguments: -File "C:\scripts\sync-loyalty-vouchers.ps1"
Trigger: Daily at 2:00 AM
```

**Bash Script (Linux/Mac):**
```bash
#!/bin/bash
# sync-loyalty-vouchers.sh

API_URL="http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync"
ADMIN_TOKEN="your-admin-jwt-token"

response=$(curl -s -X POST "$API_URL" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json")

echo "Sync completed: $response"
```

**Cron Job (Linux/Mac):**
```cron
# Run sync daily at 2:00 AM
0 2 * * * /usr/local/bin/sync-loyalty-vouchers.sh >> /var/log/loyalty-sync.log 2>&1
```

## Example 6: Error Handling

### Scenario 1: Loyalty Platform Unreachable

**Request:**
```http
POST http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync
```

**Response:**
```json
{
  "success": false,
  "message": "An error occurred while fetching vouchers from Loyalty Platform",
  "data": null,
  "errors": [
    "Connection refused to http://localhost:8079/api/vouchers"
  ]
}
```

**Solution:** Check if loyalty platform is running and accessible.

### Scenario 2: Partial Sync Failure

**Request:**
```http
POST http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync
```

**Response:**
```json
{
  "success": true,
  "message": "Vouchers synchronized successfully",
  "data": {
    "totalFetched": 10,
    "created": 7,
    "updated": 2,
    "skipped": 1,
    "errors": [
      "Error syncing INVALID_CODE: Coupon code already exists with different type"
    ]
  }
}
```

**Result:** 9 out of 10 vouchers synced successfully, 1 failed but didn't stop the process.

## Example 7: Testing with Postman

### Collection Setup

1. Create a new Postman Collection: "Loyalty Platform Integration"

2. Add environment variables:
   - `base_url`: http://localhost:5000
   - `admin_token`: your-admin-jwt-token
   - `user_token`: your-user-jwt-token

3. Add requests:

**Folder: Loyalty Platform**
- `GET` Fetch All Vouchers: `{{base_url}}/api/v1/loyaltyplatform/vouchers/fetch`
- `POST` Sync All Vouchers: `{{base_url}}/api/v1/loyaltyplatform/vouchers/sync`
- `POST` Sync User Vouchers: `{{base_url}}/api/v1/loyaltyplatform/vouchers/sync/user/{{userId}}`

**Folder: Coupons**
- `GET` List Coupons: `{{base_url}}/api/v1/coupons`
- `GET` Get by Code: `{{base_url}}/api/v1/coupons/code/{{couponCode}}`
- `POST` Validate: `{{base_url}}/api/v1/coupons/validate`
- `POST` Apply: `{{base_url}}/api/v1/coupons/apply`

**Headers for all requests:**
```json
{
  "Authorization": "Bearer {{admin_token}}",
  "Content-Type": "application/json"
}
```

## Summary

The Loyalty Platform integration provides:
- ✅ Simple REST API for voucher synchronization
- ✅ Automatic mapping to GOK Cafe's coupon system
- ✅ Support for both system-wide and user-specific vouchers
- ✅ Seamless integration with existing cart/checkout flow
- ✅ Comprehensive error handling and logging
- ✅ Preview before sync capability
- ✅ Support for one-time and gradual vouchers

All synced vouchers work exactly like native GOK Cafe coupons!
