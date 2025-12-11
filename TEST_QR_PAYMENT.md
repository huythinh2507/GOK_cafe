# Quick Test Guide - QR Code Payment

## Prerequisites Checklist

- [ ] SQL Server is running
- [ ] Bank configurations are set up (run `setup_bank_configs.sql`)
- [ ] API project is running on port 7045
- [ ] Web project is running on port 44317

## Step-by-Step Test

### Step 1: Set Up Database
```bash
# Open SQL Server Management Studio or Azure Data Studio
# Connect to: tcp:banhanh-dev.database.windows.net,1433
# Run the script: setup_bank_configs.sql
```

### Step 2: Start API Server

Open Terminal 1:
```bash
cd d:\GOK_Cafe_BE\GOK_cafe
dotnet run --project GOKCafe.API
```

Wait for:
```
Now listening on: https://localhost:7045
```

### Step 3: Start Web Server

Open Terminal 2:
```bash
cd d:\GOK_Cafe_BE\GOK_cafe
dotnet run --project GOKCafe.Web
```

Wait for:
```
Now listening on: https://localhost:44317
```

### Step 4: Test the Flow

1. **Open Browser**: Go to `https://localhost:44317`

2. **Add Products to Cart**:
   - Browse products
   - Click on a product to open modal
   - Select options (size, grind type)
   - Click "Add to Cart"
   - Repeat for a few products

3. **View Cart**:
   - Click cart icon in header
   - Verify items are showing with correct prices
   - Click "CHECK OUT" button

4. **Fill Checkout Form**:
   ```
   Full Name: Test Customer
   Email: test@example.com
   Phone: 0901234567
   Address: 533 D12, HCM City
   ```

5. **Select Payment Method**:
   - Click "Bank Transfer (QR Code)" radio button
   - Bank selection dropdown should appear
   - Select a bank (e.g., "MB Bank - 0123456789012")

6. **Place Order**:
   - Click "Place Order" button
   - Wait for processing...

7. **Expected Result**:
   A modal should appear showing:
   - ✅ QR Code image (scannable)
   - ✅ Bank details (name, account number, account name)
   - ✅ Payment amount
   - ✅ Payment description
   - ✅ Countdown timer (15:00)
   - ✅ "Waiting for payment confirmation..." message

### Step 5: Verify QR Code

The QR code URL format should be:
```
https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.jpg?amount={amount}&addInfo={description}
```

Example:
```
https://img.vietqr.io/image/970422-0123456789012-compact2.jpg?amount=310000&addInfo=GOKCafe Order ORD123456
```

### Step 6: Test Payment Simulation

Since you don't have actual bank integration, you can:

**Option A: Mark as Paid via API**
```bash
# Get the payment ID from the modal or database
# Then call the API to mark it as paid
curl -X POST https://localhost:7045/api/v1/payments/{paymentId}/mark-paid \
  -H "Authorization: Bearer {admin-token}"
```

**Option B: Update Database Directly**
```sql
-- Find the payment
SELECT TOP 1 * FROM Payments
ORDER BY CreatedAt DESC;

-- Mark as paid
UPDATE Payments
SET Status = 1, -- 1 = Paid
    PaidAt = GETUTCDATE()
WHERE Id = '{payment-id}';
```

The modal should detect the payment and redirect to confirmation page.

## Troubleshooting

### Issue: Bank dropdown shows "Loading banks..."

**Check Console:**
```javascript
// Open browser console (F12)
// You should see:
API Service initialized successfully
```

**Test API Directly:**
```bash
curl https://localhost:7045/api/v1/payments/bank-configs
```

Expected response:
```json
{
  "success": true,
  "data": [
    {
      "id": "...",
      "bankCode": "970422",
      "bankName": "MB Bank",
      "accountNumber": "0123456789012"
    }
  ]
}
```

### Issue: QR Modal doesn't appear

**Check Console Errors:**
```javascript
// Open browser console
// Look for red errors
```

**Verify payment.js is loaded:**
```javascript
// In console, type:
window.paymentManager
// Should return: PaymentManager {currentPayment: null, ...}
```

### Issue: CORS Error

If you see CORS errors, add this to `GOKCafe.API/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:44317")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ... later ...

app.UseCors();
```

### Issue: API Returns 404

- Verify API is running: `curl https://localhost:7045/api/v1/payments/bank-configs`
- Check route in controller: `/api/v1/[controller]` = `/api/v1/payments`
- Check method: `[HttpGet("bank-configs")]`

## Expected Console Output

### Successful Flow:

```
Cart data from localStorage: [{"productId":"...","name":"...","price":32000,...}]
Parsed cart items: Array(3) [...]
Container: <div id="checkoutCartItems" class="...">
Cart items to render: Array(3)
Rendering item 0: {productId: "...", name: "...", price: 32000, ...}
Item 0 - name: Bananas 1KG, price: 32000, quantity: 1
...
API Service initialized successfully
Bank configs loaded: Array(3)
Payment initiated for order: abc-123-def
QR Code modal displayed
Payment status check started
```

## Demo Video Flow

1. Homepage → Product Grid
2. Click Product → Modal Opens
3. Select Options → Add to Cart
4. Cart Sidebar Opens → Shows Items
5. Click Checkout → Redirects to Checkout Page
6. Fill Form → Select Bank Transfer
7. Choose Bank → Click Place Order
8. QR Modal Appears → Shows QR Code + Details
9. (Simulate Payment) → Redirect to Confirmation
10. Order Confirmation Page → Success Message

## Production Checklist

Before going live:

- [ ] Replace test bank account numbers with real ones
- [ ] Implement real payment verification (bank webhooks)
- [ ] Add payment timeout handling
- [ ] Set up email notifications
- [ ] Configure proper CORS for production domain
- [ ] Add logging for payment transactions
- [ ] Set up payment reconciliation
- [ ] Add admin panel for payment management
- [ ] Test with real QR code scanning
- [ ] Verify security (HTTPS, secure headers)
