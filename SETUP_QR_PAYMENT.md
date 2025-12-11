# QR Code Payment Integration Setup

Your checkout already has QR code payment integrated! Here's how to set it up:

## Current Status ✅

The following are already implemented:
- ✅ QR Payment Modal UI (in `Checkout.cshtml`)
- ✅ Payment Manager (`payment.js`)
- ✅ Payment API Controller (`PaymentsController.cs`)
- ✅ Bank Transfer Configuration endpoints
- ✅ Checkout flow with payment integration

## Setup Steps

### 1. Start Both Projects

You need both the API and Web projects running:

**Terminal 1 - API Server (Port 7045):**
```bash
cd d:\GOK_Cafe_BE\GOK_cafe
dotnet run --project GOKCafe.API
```

**Terminal 2 - Web Server (Port 44317):**
```bash
cd d:\GOK_Cafe_BE\GOK_cafe
dotnet run --project GOKCafe.Web
```

### 2. Add Bank Configuration

You need to add at least one bank configuration. Run this SQL script:

```sql
USE [GOKCafe];

-- Create BankTransferConfigs table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BankTransferConfigs')
BEGIN
    CREATE TABLE [dbo].[BankTransferConfigs] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [BankCode] NVARCHAR(20) NOT NULL,
        [BankName] NVARCHAR(200) NOT NULL,
        [AccountNumber] NVARCHAR(50) NOT NULL,
        [AccountName] NVARCHAR(200) NOT NULL,
        [BankBranch] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- Insert sample bank configuration (MB Bank - Military Bank)
INSERT INTO [dbo].[BankTransferConfigs]
    ([Id], [BankCode], [BankName], [AccountNumber], [AccountName], [BankBranch], [IsActive])
VALUES
    (NEWID(), '970422', 'MB Bank', '0123456789012', 'CONG TY GOK CAFE', 'HCM Branch', 1);

-- Insert sample bank configuration (VietcomBank)
INSERT INTO [dbo].[BankTransferConfigs]
    ([Id], [BankCode], [BankName], [AccountNumber], [AccountName], [BankBranch], [IsActive])
VALUES
    (NEWID(), '970436', 'Vietcombank', '9876543210987', 'CONG TY GOK CAFE', 'HCM Branch', 1);
```

### 3. Test the Payment Flow

1. Go to `https://localhost:44317`
2. Add products to cart
3. Go to checkout: `https://localhost:44317/checkout`
4. Fill in customer information
5. Select "Bank Transfer (QR Code)" payment method
6. Select a bank from the dropdown
7. Click "Place Order"
8. The QR code modal should appear with:
   - QR code image for scanning
   - Bank details
   - Payment amount
   - 15-minute countdown timer
   - Automatic payment verification

## How It Works

### Frontend Flow
1. User selects Bank Transfer payment method
2. User chooses a bank from dropdown (loaded from API)
3. On "Place Order":
   - Checkout API creates the order
   - Payment API generates QR code using VietQR format
   - QR modal displays with countdown
   - Frontend polls payment status every 10 seconds

### Backend Flow
1. `POST /api/v1/cart/checkout` - Creates order
2. `POST /api/v1/payments` - Creates payment with QR code
3. QR code generated using format: `https://img.vietqr.io/image/{bankCode}-{accountNumber}-{template}.jpg?amount={amount}&addInfo={description}`
4. Payment status can be verified via `POST /api/v1/payments/verify`

## Bank Codes Reference

Common Vietnamese bank codes:
- `970422` - MB Bank (Military Bank)
- `970436` - Vietcombank
- `970415` - Vietinbank
- `970418` - BIDV
- `970405` - Agribank
- `970432` - VPBank
- `970407` - Techcombank
- `970403` - Sacombank
- `970423` - TPBank
- `970416` - ACB

## Troubleshooting

### Bank dropdown shows "Loading banks..."
- Make sure API server is running on port 7045
- Check browser console for API errors
- Verify bank configs exist in database
- The system will fallback to a default bank for testing

### QR code doesn't appear
- Check browser console for errors
- Verify payment.js is loaded
- Check that the modal element exists in Checkout.cshtml

### Payment status not updating
- Backend needs to implement actual payment verification
- Currently checks database payment status
- For real integration, connect to bank API or payment gateway

## API Endpoints

All endpoints are prefixed with `/api/v1`:

### Bank Configuration
- `GET /payments/bank-configs` - Get all active banks
- `GET /payments/bank-configs/{bankCode}` - Get specific bank
- `POST /payments/bank-configs` - Create bank config (Admin only)
- `DELETE /payments/bank-configs/{id}` - Delete bank config (Admin only)

### Payment
- `POST /payments` - Create payment with QR code
- `GET /payments/{id}` - Get payment details
- `GET /payments/order/{orderId}` - Get payment by order
- `POST /payments/verify` - Verify payment status
- `POST /payments/{id}/mark-paid` - Mark as paid (Admin only)
- `POST /payments/{id}/cancel` - Cancel payment

## Next Steps

For production deployment:
1. Use real bank account numbers
2. Implement actual payment verification (bank webhooks or API)
3. Add email notifications for payment confirmation
4. Set up payment reconciliation process
5. Add admin panel for managing bank configs
6. Configure CORS for API if on different domain
