# Simple Loyalty Platform Integration Test

Write-Host "Testing Loyalty Platform Integration" -ForegroundColor Cyan
Write-Host ""

# Test 1: Loyalty Platform
Write-Host "[1/3] Testing Loyalty Platform..." -ForegroundColor Yellow
$vouchers = Invoke-RestMethod -Uri "http://localhost:3000/api/vouchers" -Method Get
Write-Host "   Found $($vouchers.Count) vouchers" -ForegroundColor Green
Write-Host "   First voucher: $($vouchers[0].code) - $($vouchers[0].name)" -ForegroundColor Gray
Write-Host ""

# Test 2: Login to GOK Cafe
Write-Host "[2/3] Logging in to GOK Cafe..." -ForegroundColor Yellow
$loginBody = @{ email = "admin@example.com"; password = "Huythinh1" } | ConvertTo-Json
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $login.data.token
Write-Host "   Login successful, token obtained" -ForegroundColor Green
Write-Host ""

# Test 3: Sync Vouchers
Write-Host "[3/3] Syncing vouchers..." -ForegroundColor Yellow
$headers = @{ Authorization = "Bearer $token" }
$syncResult = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/loyaltyplatform/vouchers/sync" -Method Post -Headers $headers
Write-Host "   Sync completed!" -ForegroundColor Green
Write-Host "   Created: $($syncResult.data.created)" -ForegroundColor Green
Write-Host "   Updated: $($syncResult.data.updated)" -ForegroundColor Yellow
Write-Host "   Total: $($syncResult.data.totalFetched)" -ForegroundColor Cyan
Write-Host ""

Write-Host "Integration test complete!" -ForegroundColor Green
