#!/bin/bash
# Bash script to seed product prices
# Usage: ./seed-prices.sh

echo "=== GOK Cafe Product Price Seeder ==="
echo ""
echo "This script will update all products with a price of 0 to have reasonable prices."
echo "Prices will be set based on product category:"
echo "  - Coffee: \$3.00 - \$6.50"
echo "  - Tea: \$3.00 - \$5.50"
echo "  - Specialty Drinks: \$4.50 - \$7.50"
echo "  - Food: \$2.50 - \$8.00"
echo "  - Merchandise: \$10.00 - \$50.00"
echo ""

read -p "Do you want to proceed? (Y/N): " confirmation

if [ "$confirmation" != "Y" ] && [ "$confirmation" != "y" ]; then
    echo "Operation cancelled."
    exit 1
fi

echo ""
echo "Starting price seeding process..."
echo ""

# Run the API with the seed-prices argument
dotnet run --project GOKCafe.API --seed-prices --exit-after-seed

echo ""
echo "=== Price seeding completed! ==="
