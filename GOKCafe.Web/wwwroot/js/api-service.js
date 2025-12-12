// API Service - Handles all API communications
// This service bridges the frontend with the backend API

class ApiService {
    constructor() {
        // Get API base URL from environment or default to localhost API
        // Check if we're on localhost and use the API server URL
        const isLocalhost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';

        if (isLocalhost) {
            // Use the API server running on port 7045
            this.baseUrl = 'https://localhost:7045/api/v1';
        } else {
            // In production, API might be on same host or different domain
            this.baseUrl = '/api/v1';
        }

        this.sessionId = this.getOrCreateSessionId();
    }

    // ========================================
    // Session Management
    // ========================================
    getOrCreateSessionId() {
        let sessionId = localStorage.getItem('gok_session_id');
        if (!sessionId) {
            sessionId = this.generateUUID();
            localStorage.setItem('gok_session_id', sessionId);
        }
        return sessionId;
    }

    generateUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    // ========================================
    // HTTP Helper Methods
    // ========================================
    async request(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            }
        };

        const config = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, config);
            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.message || 'API request failed');
            }

            return data;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    async get(endpoint, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = `${this.baseUrl}${endpoint}${queryString ? '?' + queryString : ''}`;
        return this.request(url, { method: 'GET' });
    }

    async post(endpoint, body, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = `${this.baseUrl}${endpoint}${queryString ? '?' + queryString : ''}`;
        return this.request(url, {
            method: 'POST',
            body: JSON.stringify(body)
        });
    }

    async put(endpoint, body, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = `${this.baseUrl}${endpoint}${queryString ? '?' + queryString : ''}`;
        return this.request(url, {
            method: 'PUT',
            body: JSON.stringify(body)
        });
    }

    async delete(endpoint, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const url = `${this.baseUrl}${endpoint}${queryString ? '?' + queryString : ''}`;
        return this.request(url, { method: 'DELETE' });
    }

    // ========================================
    // Cart API Methods
    // ========================================

    /**
     * Get cart for current user/session
     * @returns {Promise<Object>} Cart data
     */
    async getCart() {
        return this.get('/cart', { sessionId: this.sessionId });
    }

    /**
     * Add product to cart
     * @param {Object} item - { productId, quantity, flavourProfileIds, equipmentIds }
     * @returns {Promise<Object>} Updated cart
     */
    async addToCart(item) {
        return this.post('/cart/items', item, { sessionId: this.sessionId });
    }

    /**
     * Update cart item quantity
     * @param {string} cartItemId - Cart item ID
     * @param {number} quantity - New quantity
     * @returns {Promise<Object>} Updated cart
     */
    async updateCartItem(cartItemId, quantity) {
        return this.put(`/cart/items/${cartItemId}`, { quantity }, { sessionId: this.sessionId });
    }

    /**
     * Remove item from cart
     * @param {string} cartItemId - Cart item ID to remove
     * @returns {Promise<Object>} Result
     */
    async removeCartItem(cartItemId) {
        return this.delete(`/cart/items/${cartItemId}`, { sessionId: this.sessionId });
    }

    /**
     * Clear entire cart
     * @returns {Promise<Object>} Result
     */
    async clearCart() {
        return this.delete('/cart', { sessionId: this.sessionId });
    }

    /**
     * Get cart item count
     * @returns {Promise<number>} Item count
     */
    async getCartItemCount() {
        return this.get('/cart/count', { sessionId: this.sessionId });
    }

    /**
     * Checkout from cart
     * @param {Object} checkoutData - Checkout information
     * @returns {Promise<Object>} Order data
     */
    async checkout(checkoutData) {
        return this.post('/cart/checkout', checkoutData, { sessionId: this.sessionId });
    }

    /**
     * Apply coupon code to cart
     * @param {string} couponCode - Coupon code to apply
     * @returns {Promise<Object>} Updated cart with discount
     */
    async applyCouponToCart(couponCode) {
        return this.post('/cart/apply-coupon', null, {
            couponCode,
            sessionId: this.sessionId
        });
    }

    /**
     * Remove coupon from cart
     * @returns {Promise<Object>} Updated cart without discount
     */
    async removeCouponFromCart() {
        return this.delete('/cart/remove-coupon', {
            sessionId: this.sessionId
        });
    }

    // ========================================
    // Coupon API Methods
    // ========================================

    /**
     * Get available system coupons
     * @param {number} pageNumber - Page number
     * @param {number} pageSize - Page size
     * @returns {Promise<Object>} Paginated coupons
     */
    async getAvailableCoupons(pageNumber = 1, pageSize = 10) {
        return this.get('/coupons/system', { pageNumber, pageSize });
    }

    /**
     * Validate coupon before applying
     * @param {string} couponCode - Coupon code
     * @param {number} orderAmount - Order amount to validate against
     * @returns {Promise<Object>} Validation result
     */
    async validateCoupon(couponCode, orderAmount) {
        return this.post('/coupons/validate', {
            couponCode,
            orderAmount
        });
    }

    // ========================================
    // Product API Methods
    // ========================================

    /**
     * Get all products with filters
     * @param {Object} filters - Filter parameters
     * @returns {Promise<Object>} Paginated products
     */
    async getProducts(filters = {}) {
        return this.get('/products', filters);
    }

    /**
     * Get single product by ID
     * @param {string} productId - Product ID
     * @returns {Promise<Object>} Product details
     */
    async getProduct(productId) {
        return this.get(`/products/${productId}`);
    }

    // ========================================
    // Category API Methods
    // ========================================

    /**
     * Get all categories
     * @returns {Promise<Array>} List of categories
     */
    async getCategories() {
        return this.get('/categories');
    }

    /**
     * Get category by ID
     * @param {string} categoryId - Category ID
     * @returns {Promise<Object>} Category details
     */
    async getCategory(categoryId) {
        return this.get(`/categories/${categoryId}`);
    }

    // ========================================
    // Payment API Methods
    // ========================================

    /**
     * Create a payment for an order
     * @param {Object} paymentData - { orderId, paymentMethod, bankCode }
     * @returns {Promise<Object>} Payment details with QR code info
     */
    async createPayment(paymentData) {
        return this.post('/payments', paymentData);
    }

    /**
     * Get payment by ID
     * @param {string} paymentId - Payment ID
     * @returns {Promise<Object>} Payment details
     */
    async getPayment(paymentId) {
        return this.get(`/payments/${paymentId}`);
    }

    /**
     * Get payment by order ID
     * @param {string} orderId - Order ID
     * @returns {Promise<Object>} Payment details
     */
    async getPaymentByOrderId(orderId) {
        return this.get(`/payments/order/${orderId}`);
    }

    /**
     * Verify payment status
     * @param {Object} verifyData - { paymentId, transactionReference }
     * @returns {Promise<Object>} Verification result
     */
    async verifyPayment(verifyData) {
        return this.post('/payments/verify', verifyData);
    }

    /**
     * Cancel payment
     * @param {string} paymentId - Payment ID
     * @returns {Promise<Object>} Cancellation result
     */
    async cancelPayment(paymentId) {
        return this.post(`/payments/${paymentId}/cancel`, {});
    }

    /**
     * Get active bank transfer configurations
     * @returns {Promise<Array>} List of active bank configs
     */
    async getActiveBankConfigs() {
        return this.get('/payments/bank-configs');
    }

    /**
     * Get bank config by bank code
     * @param {string} bankCode - Bank code
     * @returns {Promise<Object>} Bank configuration
     */
    async getBankConfig(bankCode) {
        return this.get(`/payments/bank-configs/${bankCode}`);
    }

    // ========================================
    // Product Feed Methods (Umbraco Commerce)
    // ========================================

    /**
     * Get product feed XML/JSON
     * @param {string} feedPath - Feed path configured in Umbraco
     * @returns {Promise<string>} Feed content (XML/JSON)
     */
    async getProductFeed(feedPath) {
        const url = `/umbraco/commerce/productfeed/${feedPath}`;
        const response = await fetch(url);
        return response.text();
    }

    /**
     * Parse product feed XML to JSON
     * @param {string} xmlString - XML string from feed
     * @returns {Array} Parsed products
     */
    parseProductFeedXML(xmlString) {
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(xmlString, 'text/xml');
        const items = xmlDoc.querySelectorAll('item');

        const products = [];
        items.forEach(item => {
            const product = {
                id: this.getXMLValue(item, 'g\\:id'),
                title: this.getXMLValue(item, 'g\\:title') || this.getXMLValue(item, 'title'),
                description: this.getXMLValue(item, 'g\\:description') || this.getXMLValue(item, 'description'),
                link: this.getXMLValue(item, 'g\\:link') || this.getXMLValue(item, 'link'),
                imageLink: this.getXMLValue(item, 'g\\:image_link'),
                price: this.getXMLValue(item, 'g\\:price'),
                availability: this.getXMLValue(item, 'g\\:availability'),
                itemGroupId: this.getXMLValue(item, 'g\\:item_group_id')
            };
            products.push(product);
        });

        return products;
    }

    getXMLValue(parent, tagName) {
        const element = parent.querySelector(tagName);
        return element ? element.textContent : '';
    }
}

// Create global instance
window.apiService = new ApiService();

console.log('API Service initialized successfully');
