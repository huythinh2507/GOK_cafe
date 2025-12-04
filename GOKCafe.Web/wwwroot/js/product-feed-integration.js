// Product Feed Integration with Umbraco Commerce
// This file demonstrates how to integrate product feeds from Umbraco Commerce into your frontend

class ProductFeedIntegration {
    constructor() {
        this.feedPath = 'products'; // Change this to match your feed path in Umbraco backoffice
        this.products = [];
    }

    /**
     * Load products from Umbraco Commerce Product Feed
     * @returns {Promise<Array>} Array of products from feed
     */
    async loadProductsFromFeed() {
        try {
            // Get XML feed from Umbraco Commerce
            const feedXML = await window.apiService.getProductFeed(this.feedPath);

            // Parse XML to JSON
            this.products = window.apiService.parseProductFeedXML(feedXML);

            console.log('Products loaded from feed:', this.products);
            return this.products;
        } catch (error) {
            console.error('Failed to load product feed:', error);
            return [];
        }
    }

    /**
     * Render products from feed into product grid
     * @param {string} containerId - Container element ID
     */
    async renderProductGrid(containerId = 'product-grid') {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container #${containerId} not found`);
            return;
        }

        // Show loading state
        container.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin fa-3x"></i><p>Loading products...</p></div>';

        // Load products
        const products = await this.loadProductsFromFeed();

        if (products.length === 0) {
            container.innerHTML = '<div class="alert alert-info">No products available</div>';
            return;
        }

        // Render product cards
        const html = products.map(product => this.renderProductCard(product)).join('');
        container.innerHTML = `<div class="row">${html}</div>`;
    }

    /**
     * Render single product card
     * @param {Object} product - Product data from feed
     * @returns {string} HTML string
     */
    renderProductCard(product) {
        const availability = product.availability === 'in_stock' ?
            '<span class="badge badge-success">In Stock</span>' :
            '<span class="badge badge-danger">Out of Stock</span>';

        return `
            <div class="col-md-4 mb-4">
                <div class="card product-card h-100">
                    <img src="${product.imageLink}" class="card-img-top" alt="${product.title}" />
                    <div class="card-body">
                        <h5 class="card-title">${product.title}</h5>
                        <p class="card-text text-muted">${this.truncateText(product.description, 100)}</p>
                        <div class="d-flex justify-content-between align-items-center">
                            <span class="h4 mb-0 text-primary">${product.price}</span>
                            ${availability}
                        </div>
                    </div>
                    <div class="card-footer">
                        <a href="${product.link}" class="btn btn-primary btn-block">View Product</a>
                        <button class="btn btn-outline-secondary btn-block mt-2"
                                onclick="addToCartFromFeed('${product.id}')">
                            <i class="fas fa-cart-plus"></i> Add to Cart
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Truncate text to specified length
     * @param {string} text - Text to truncate
     * @param {number} maxLength - Maximum length
     * @returns {string} Truncated text
     */
    truncateText(text, maxLength) {
        if (!text) return '';
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    }

    /**
     * Search products by keyword
     * @param {string} keyword - Search keyword
     * @returns {Array} Filtered products
     */
    searchProducts(keyword) {
        if (!keyword) return this.products;

        keyword = keyword.toLowerCase();
        return this.products.filter(product =>
            product.title.toLowerCase().includes(keyword) ||
            product.description.toLowerCase().includes(keyword)
        );
    }

    /**
     * Filter products by price range
     * @param {number} minPrice - Minimum price
     * @param {number} maxPrice - Maximum price
     * @returns {Array} Filtered products
     */
    filterByPrice(minPrice, maxPrice) {
        return this.products.filter(product => {
            const price = parseFloat(product.price.replace(/[^0-9.]/g, ''));
            return price >= minPrice && price <= maxPrice;
        });
    }

    /**
     * Get product by ID from feed
     * @param {string} productId - Product ID
     * @returns {Object|null} Product or null
     */
    getProductById(productId) {
        return this.products.find(p => p.id === productId) || null;
    }
}

// Global instance
window.productFeedIntegration = new ProductFeedIntegration();

/**
 * Add product to cart from feed
 * This function can be called from product cards
 * @param {string} productId - Product ID from feed
 */
window.addToCartFromFeed = async function(productId) {
    try {
        const product = window.productFeedIntegration.getProductById(productId);
        if (!product) {
            alert('Product not found');
            return;
        }

        // Add to cart using API service
        const result = await window.apiService.addToCart({
            productId: productId,
            quantity: 1,
            flavourProfileIds: [],
            equipmentIds: []
        });

        if (result.success) {
            alert(`${product.title} added to cart!`);
            // Update cart count in header if exists
            updateCartCount();
        } else {
            alert('Failed to add to cart');
        }
    } catch (error) {
        console.error('Error adding to cart:', error);
        alert('Failed to add to cart');
    }
};

/**
 * Update cart count in header
 */
async function updateCartCount() {
    try {
        const result = await window.apiService.getCartItemCount();
        if (result.success) {
            const cartCountElements = document.querySelectorAll('.cart-count, .cart-badge');
            cartCountElements.forEach(el => {
                el.textContent = result.data;
                el.style.display = result.data > 0 ? 'inline-block' : 'none';
            });
        }
    } catch (error) {
        console.error('Failed to update cart count:', error);
    }
}

/**
 * Example: Initialize product grid on page load
 * Usage in your HTML:
 *
 * <div id="product-grid"></div>
 * <script>
 *   document.addEventListener('DOMContentLoaded', () => {
 *     productFeedIntegration.renderProductGrid('product-grid');
 *   });
 * </script>
 */

console.log('Product Feed Integration loaded successfully');
