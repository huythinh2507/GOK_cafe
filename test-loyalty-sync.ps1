# Loyalty Platform Integration Test Script
# This script tests the complete integration between Loyalty Platform and GOK Cafe

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Loyalty Platform Integration Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$loyaltyUrl = "http://localhost:3000"
$gokCafeUrl = "http://localhost:5000"

# Step 1: Test Loyalty Platform Vouchers Endpoint
Write-Host "[1/4] Testing Loyalty Platform Vouchers Endpoint..." -ForegroundColor Yellow
try {
    $vouchersResponse = Invoke-RestMethod -Uri "$loyaltyUrl/api/vouchers" -Method Get -ErrorAction Stop
    $voucherCount = $vouchersResponse.Count
    Write-Host "   ✓ Success! Found $voucherCount vouchers in Loyalty Platform" -ForegroundColor Green

    # Show first 3 vouchers
    Write-Host "   Sample vouchers:" -ForegroundColor Gray
    $vouchersResponse | Select-Object -First 3 | ForEach-Object {
        $voucherInfo = "     - $($_.code): $($_.name) (Type: $($_.type), Value: $($_.value))"
        Write-Host $voucherInfo -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Failed to fetch vouchers from Loyalty Platform" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    Write-Host "   Make sure Loyalty Platform is running on $loyaltyUrl" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Step 2: Get Admin Token
Write-Host "[2/4] Logging in to GOK Cafe to get Admin token..." -ForegroundColor Yellow

# You need to replace these with actual admin credentials
$loginBody = @{
    email = "admin@gokcafe.com"
    password = "Admin123@"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$gokCafeUrl/api/v1/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json" `
        -ErrorAction Stop

    $token = $loginResponse.data.token

    if ($token) {
        Write-Host "   ✓ Successfully logged in! Token obtained" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Login successful but no token received" -ForegroundColor Red
        Write-Host "   Response: $($loginResponse | ConvertTo-Json)" -ForegroundColor Gray
        exit 1
    }
} catch {
    Write-Host "   ✗ Failed to login to GOK Cafe" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    Write-Host "   Please check admin credentials in the script" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Step 3: Fetch Vouchers (Preview)
Write-Host "[3/4] Fetching vouchers from Loyalty Platform via GOK Cafe API (preview)..." -ForegroundColor Yellow

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $fetchResponse = Invoke-RestMethod -Uri "$gokCafeUrl/api/v1/loyaltyplatform/vouchers/fetch" `
        -Method Get `
        -Headers $headers `
        -ErrorAction Stop

    if ($fetchResponse.success) {
        $fetchedCount = $fetchResponse.data.Count
        Write-Host "   ✓ Successfully fetched $fetchedCount vouchers via GOK Cafe API" -ForegroundColor Green

        # Show first 3
        Write-Host "   Sample vouchers:" -ForegroundColor Gray
        $fetchResponse.data | Select-Object -First 3 | ForEach-Object {
            Write-Host "     - $($_.code): $($_.name)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ✗ Fetch failed" -ForegroundColor Red
        Write-Host "   Message: $($fetchResponse.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Failed to fetch vouchers via GOK Cafe API" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: Sync Vouchers
Write-Host "[4/4] Syncing vouchers from Loyalty Platform to GOK Cafe..." -ForegroundColor Yellow

try {
    $syncResponse = Invoke-RestMethod -Uri "$gokCafeUrl/api/v1/loyaltyplatform/vouchers/sync" `
        -Method Post `
        -Headers $headers `
        -ErrorAction Stop

    if ($syncResponse.success) {
        $syncData = $syncResponse.data
        Write-Host "   ✓ Sync completed successfully!" -ForegroundColor Green
        Write-Host "   Results:" -ForegroundColor Cyan
        Write-Host "     - Total Fetched: $($syncData.totalFetched)" -ForegroundColor White
        Write-Host "     - Created: $($syncData.created)" -ForegroundColor Green
        Write-Host "     - Updated: $($syncData.updated)" -ForegroundColor Yellow
        Write-Host "     - Skipped: $($syncData.skipped)" -ForegroundColor Gray

        if ($syncData.errors -and $syncData.errors.Count -gt 0) {
            Write-Host "     - Errors: $($syncData.errors.Count)" -ForegroundColor Red
            $syncData.errors | ForEach-Object {
                Write-Host "       • $_" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "   ✗ Sync failed" -ForegroundColor Red
        Write-Host "   Message: $($syncResponse.message)" -ForegroundColor Red
        if ($syncResponse.errors) {
            $syncResponse.errors | ForEach-Object {
                Write-Host "     - $_" -ForegroundColor Red
            }
        }
        exit 1
    }
} catch {
    Write-Host "   ✗ Failed to sync vouchers" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ Integration Test Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test a coupon by code: GET $gokCafeUrl/api/v1/coupons/code/WELCOME10" -ForegroundColor White
Write-Host "2. Apply a coupon: POST $gokCafeUrl/api/v1/coupons/apply" -ForegroundColor White
Write-Host "3. Test checkout flow with discount" -ForegroundColor White
Write-Host ""
