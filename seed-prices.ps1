#!/usr/bin/env pwsh
# PowerShell script to seed product prices
# Usage: .\seed-prices.ps1

Write-Host "=== GOK Cafe Product Price Seeder ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will update all products with a price of 0 to have reasonable prices." -ForegroundColor Yellow
Write-Host "Prices will be set based on product category:" -ForegroundColor Yellow
Write-Host "  - Coffee: `$3.00 - `$6.50" -ForegroundColor Gray
Write-Host "  - Tea: `$3.00 - `$5.50" -ForegroundColor Gray
Write-Host "  - Specialty Drinks: `$4.50 - `$7.50" -ForegroundColor Gray
Write-Host "  - Food: `$2.50 - `$8.00" -ForegroundColor Gray
Write-Host "  - Merchandise: `$10.00 - `$50.00" -ForegroundColor Gray
Write-Host ""

$confirmation = Read-Host "Do you want to proceed? (Y/N)"

if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "Operation cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Starting price seeding process..." -ForegroundColor Green
Write-Host ""

# Run the API with the seed-prices argument
dotnet run --project GOKCafe.API --seed-prices --exit-after-seed

Write-Host ""
Write-Host "=== Price seeding completed! ===" -ForegroundColor Cyan
