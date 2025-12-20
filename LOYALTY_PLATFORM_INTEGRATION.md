# Loyalty Platform Integration Guide

## Overview

This document describes the integration between GOK Cafe and the external Loyalty Platform for voucher/coupon synchronization.

## Architecture

The integration follows GOK Cafe's existing patterns (similar to the Odoo integration):

```
Loyalty Platform → LoyaltyPlatformService → Coupon System → Cart/Checkout
```

### Components

1. **DTOs** (`GOKCafe.Application/DTOs/LoyaltyPlatform/`)
   - `LoyaltyVoucherDto` - Represents a voucher from the loyalty platform
   - `LoyaltyVouchersResponse` - API response wrapper
   - `LoyaltySyncResultDto` - Sync operation results

2. **Service Interface** (`ILoyaltyPlatformService`)
   - `FetchVouchersFromLoyaltyPlatformAsync()` - Fetch all vouchers (preview)
   - `FetchUserVouchersAsync(userId)` - Fetch user-specific vouchers (preview)
   - `SyncVouchersFromLoyaltyPlatformAsync()` - Sync all vouchers to database
   - `SyncUserVouchersAsync(userId)` - Sync user vouchers to database

3. **Service Implementation** (`LoyaltyPlatformService`)
   - HTTP client for API calls
   - Voucher mapping to GOK Cafe coupons
   - Batch processing and error handling

4. **API Controller** (`LoyaltyPlatformController`)
   - Admin-only endpoints for voucher management
   - RESTful API design

## Configuration

### appsettings.json / appsettings.Development.json

```json
{
  "LoyaltyPlatform": {
    "Url": "http://localhost:8079",
    "ApiKey": ""
  }
}
```

**Configuration Options:**
- `Url` - Base URL of the loyalty platform (required)
- `ApiKey` - Optional bearer token for authentication (leave empty if not needed)

## API Endpoints

All endpoints require **Admin role** authorization.

### 1. Fetch Vouchers (Preview)
```http
GET /api/v1/loyaltyplatform/vouchers/fetch
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": "uuid",
      "code": "WELCOME10",
      "name": "Welcome Discount",
      "description": "10% off for new customers",
      "type": "percentage",
      "value": 10,
      "maxDiscount": 50,
      "minOrderAmount": 100,
      "remainingBalance": null,
      "isActive": true,
      "startDate": "2025-01-01T00:00:00Z",
      "endDate": "2025-12-31T23:59:59Z",
      "maxUsageCount": 1000,
      "usageCount": 0,
      "targetUserId": null,
      "isSystemWide": true
    }
  ],
  "errors": null
}
```

### 2. Fetch User Vouchers (Preview)
```http
GET /api/v1/loyaltyplatform/vouchers/user/{userId}
Authorization: Bearer {jwt-token}
```

### 3. Sync All Vouchers
```http
POST /api/v1/loyaltyplatform/vouchers/sync
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "success": true,
  "message": "Vouchers synchronized successfully",
  "data": {
    "totalFetched": 50,
    "created": 35,
    "updated": 15,
    "skipped": 0,
    "errors": []
  },
  "errors": null
}
```

### 4. Sync User Vouchers
```http
POST /api/v1/loyaltyplatform/vouchers/sync/user/{userId}
Authorization: Bearer {jwt-token}
```

## Voucher Mapping

Loyalty Platform vouchers are automatically mapped to GOK Cafe coupons:

| Loyalty Platform Field | GOK Cafe Coupon Field | Notes |
|----------------------|---------------------|-------|
| `code` | `Code` | Unique identifier |
| `name` | `Name` | Display name |
| `description` | `Description` | Optional details |
| `type` | `Type` + `DiscountType` | See mapping below |
| `value` | `DiscountValue` | Percentage or fixed amount |
| `maxDiscount` | `MaxDiscountAmount` | Cap on discount |
| `minOrderAmount` | `MinOrderAmount` | Minimum order requirement |
| `remainingBalance` | `RemainingBalance` | For gradual coupons |
| `isSystemWide` | `IsSystemCoupon` | System vs user-specific |
| `targetUserId` | `UserId` | Null for system coupons |
| `isActive` | `IsActive` | Active status |
| `startDate` | `StartDate` | Valid from |
| `endDate` | `EndDate` | Valid until |
| `maxUsageCount` | `MaxUsageCount` | Usage limit |
| `usageCount` | `UsageCount` | Current usage |

### Type Mapping

**Voucher Type → Coupon Type:**
- `"onetime"`, `"one-time"`, `"single"` → `CouponType.OneTime`
- `"gradual"`, `"recurring"`, `"multiple"` → `CouponType.Gradual`
- Default → `CouponType.OneTime`

**Voucher Type → Discount Type:**
- `"percentage"`, `"percent"` → `DiscountType.Percentage`
- `"fixed"`, `"fixedamount"`, `"fixed-amount"` → `DiscountType.FixedAmount`
- Default → `DiscountType.Percentage`

## Expected Loyalty Platform API Format

The loyalty platform should expose the following endpoints:

### Get All Vouchers
```http
GET /api/vouchers
Authorization: Bearer {api-key} (if configured)
```

**Response Format (Option 1 - Wrapped):**
```json
{
  "success": true,
  "message": null,
  "vouchers": [...]
}
```

**Response Format (Option 2 - Direct Array):**
```json
[
  {
    "id": "string",
    "code": "string",
    "name": "string",
    "description": "string",
    "type": "percentage|fixed|onetime|gradual",
    "value": 10.0,
    "maxDiscount": 50.0,
    "minOrderAmount": 100.0,
    "remainingBalance": null,
    "isActive": true,
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-12-31T23:59:59Z",
    "maxUsageCount": 1000,
    "usageCount": 0,
    "targetUserId": null,
    "isSystemWide": true
  }
]
```

### Get User Vouchers
```http
GET /api/vouchers/user/{userId}
Authorization: Bearer {api-key} (if configured)
```

Same response format as above.

## Usage Workflow

### 1. **Preview Vouchers** (Optional)
Before syncing, you can preview what vouchers are available:
```bash
GET /api/v1/loyaltyplatform/vouchers/fetch
```

### 2. **Sync Vouchers to GOK Cafe**
Synchronize vouchers to make them available in the coupon system:
```bash
POST /api/v1/loyaltyplatform/vouchers/sync
```

### 3. **Use Synced Coupons**
Once synced, vouchers become regular coupons in GOK Cafe and can be used via:
- Existing coupon validation endpoint: `POST /api/v1/coupons/validate`
- Existing coupon application endpoint: `POST /api/v1/coupons/apply`
- Cart checkout flow with coupon codes

### 4. **Periodic Sync** (Recommended)
Set up a scheduled job (using Hangfire, cron, or Windows Task Scheduler) to periodically sync vouchers:
```csharp
// Example: Daily sync at midnight
var result = await _loyaltyPlatformService.SyncVouchersFromLoyaltyPlatformAsync();
```

## Integration with Cart System

The synced vouchers integrate seamlessly with GOK Cafe's existing cart discount system:

1. **Cart Entity** already supports:
   - `AppliedCouponId` - Reference to the coupon
   - `AppliedCouponCode` - Quick reference
   - `DiscountAmount` - Calculated discount
   - Computed `Total` including discount

2. **Coupon Application Flow:**
   ```
   User enters voucher code → Validate via CouponService → Apply to cart → Checkout
   ```

3. **Coupon Usage Tracking:**
   - Every coupon usage is recorded in `CouponUsage` table
   - Usage counts are incremented
   - Gradual coupon balances are decremented

## Error Handling

The service handles errors gracefully:

### Common Errors

1. **Connection Failure**
   - Error: "Failed to fetch vouchers from Loyalty Platform: [status code]"
   - Solution: Check `LoyaltyPlatform:Url` configuration and network connectivity

2. **Authentication Failure**
   - Error: "Failed to fetch vouchers from Loyalty Platform: 401"
   - Solution: Verify `LoyaltyPlatform:ApiKey` configuration

3. **Invalid Response Format**
   - Error: "Failed to parse response from Loyalty Platform"
   - Solution: Ensure loyalty platform returns expected JSON format

4. **Partial Sync Failures**
   - Individual voucher errors are logged in `LoyaltySyncResultDto.Errors`
   - Sync continues for remaining vouchers
   - Check logs for detailed error messages

## Logging

The service logs important events:

- **Information Level:**
  - Fetch/sync start
  - Successful fetches with voucher count
  - Sync completion with statistics

- **Error Level:**
  - API call failures
  - Voucher mapping errors
  - Database save errors

- **Debug Level:**
  - Raw API responses
  - Individual voucher creation/update

**Example Log Output:**
```
[Information] Starting Loyalty Platform voucher synchronization...
[Information] Successfully fetched 50 vouchers from Loyalty Platform
[Information] Found 15 existing coupons in system
[Debug] Created new coupon: WELCOME10
[Debug] Updated coupon: SUMMER20
[Information] Loyalty Platform sync completed. Created: 35, Updated: 15, Skipped: 0
```

## Testing the Integration

### 1. Configure the Loyalty Platform URL
Update `appsettings.Development.json`:
```json
{
  "LoyaltyPlatform": {
    "Url": "http://localhost:8079",
    "ApiKey": ""
  }
}
```

### 2. Test Fetch Endpoint
```bash
curl -X GET "https://localhost:7001/api/v1/loyaltyplatform/vouchers/fetch" \
  -H "Authorization: Bearer {admin-jwt-token}"
```

### 3. Test Sync Endpoint
```bash
curl -X POST "https://localhost:7001/api/v1/loyaltyplatform/vouchers/sync" \
  -H "Authorization: Bearer {admin-jwt-token}"
```

### 4. Verify Synced Coupons
```bash
curl -X GET "https://localhost:7001/api/v1/coupons" \
  -H "Authorization: Bearer {admin-jwt-token}"
```

### 5. Test Coupon Application
```bash
curl -X POST "https://localhost:7001/api/v1/coupons/apply" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {jwt-token}" \
  -d '{
    "couponCode": "WELCOME10",
    "orderAmount": 200,
    "userId": "user-guid-here"
  }'
```

## Security Considerations

1. **Admin-Only Access**
   - All loyalty platform endpoints require Admin role
   - Prevents unauthorized voucher manipulation

2. **API Key Protection**
   - Store API key in environment variables or Azure Key Vault in production
   - Never commit API keys to source control

3. **HTTPS Required**
   - Use HTTPS for production loyalty platform endpoints
   - Protect data in transit

4. **Rate Limiting** (Recommended)
   - Implement rate limiting on sync endpoints
   - Prevent abuse and excessive API calls

## Troubleshooting

### Issue: "Loyalty Platform URL not configured"
**Solution:** Add `LoyaltyPlatform:Url` to appsettings.json

### Issue: "Failed to fetch vouchers: Connection refused"
**Solution:**
- Verify loyalty platform is running
- Check if port 8079 is accessible
- Verify firewall settings

### Issue: Vouchers not appearing after sync
**Solution:**
- Check sync result for errors
- Verify voucher codes are unique
- Check coupon `IsActive` status
- Verify date ranges (StartDate/EndDate)

### Issue: Coupons not applying to cart
**Solution:**
- Verify coupon is active via `/api/v1/coupons/code/{code}`
- Check minimum order amount requirement
- Verify user eligibility (system vs personal coupons)
- Check expiry dates

## Future Enhancements

Potential improvements to the integration:

1. **Webhook Support**
   - Receive real-time voucher updates from loyalty platform
   - Eliminate need for polling/scheduled syncs

2. **Bi-directional Sync**
   - Send coupon usage data back to loyalty platform
   - Keep usage counts in sync

3. **Background Job Scheduling**
   - Integrate with Hangfire for automated syncs
   - Schedule daily/hourly voucher updates

4. **Conflict Resolution**
   - Handle voucher code conflicts
   - Merge strategies for duplicate vouchers

5. **Analytics Dashboard**
   - Track sync performance
   - Monitor voucher usage statistics
   - Alert on sync failures

## Support

For issues or questions about the integration:

1. Check application logs for error details
2. Verify loyalty platform API is accessible
3. Review this documentation
4. Contact the development team

---

**Last Updated:** December 11, 2025
**Version:** 1.0
**Author:** GOK Cafe Development Team
