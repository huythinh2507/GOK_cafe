// API-based Shopping Cart Management
// This file provides API integration for the cart system

class ApiShoppingCart {
    constructor() {
        this.cartData = null;
        this.init();
    }

    async init() {
        // Wait for apiService to be available
        if (!window.apiService) {
            setTimeout(() => this.init(), 100);
            return;
        }
        await this.loadCart();
        this.updateCartUI();
    }

    async loadCart() {
        try {
            const response = await window.apiService.getCart();
            if (response.success) {
                this.cartData = response.data;
            } else {
                this.cartData = { items: [], subtotal: 0, total: 0 };
            }
        } catch (error) {
            console.error('Failed to load cart:', error);
            this.cartData = { items: [], subtotal: 0, total: 0 };
        }
        return this.cartData;
    }

    async addItem(productId, quantity = 1, size = null, grind = null) {
        try {
            const payload = {
                productId: productId,
                quantity: quantity
            };

            // Add size and grind if provided
            if (size) payload.selectedSize = size;
            if (grind) payload.selectedGrind = grind;

            const response = await window.apiService.addToCart(payload);

            if (response.success) {
                this.cartData = response.data;
                this.updateCartUI();
                this.renderCartSidebar();
                this.showNotification('Product added to cart!', 'success');

                // Auto-open cart sidebar
                if (window.openCartSidebar) {
                    window.openCartSidebar();
                }
            } else {
                this.showNotification(response.message || 'Failed to add to cart', 'error');
            }
        } catch (error) {
            console.error('Failed to add item to cart:', error);
            this.showNotification('Failed to add to cart', 'error');
        }
    }

    async removeItem(cartItemId) {
        try {
            const response = await window.apiService.removeCartItem(cartItemId);

            if (response.success) {
                await this.loadCart();
                this.updateCartUI();
                this.renderCartSidebar();
                this.showNotification('Item removed from cart', 'info');
            } else {
                this.showNotification(response.message || 'Failed to remove item', 'error');
            }
        } catch (error) {
            console.error('Failed to remove item:', error);
            this.showNotification('Failed to remove item', 'error');
        }
    }

    async updateQuantity(cartItemId, quantity) {
        try {
            const response = await window.apiService.updateCartItem(cartItemId, quantity);

            if (response.success) {
                this.cartData = response.data;
                this.updateCartUI();
                this.renderCartSidebar();
            } else {
                this.showNotification(response.message || 'Failed to update quantity', 'error');
            }
        } catch (error) {
            console.error('Failed to update quantity:', error);
            this.showNotification('Failed to update quantity', 'error');
        }
    }

    getItemCount() {
        if (!this.cartData || !this.cartData.items) return 0;
        return this.cartData.items.reduce((total, item) => total + item.quantity, 0);
    }

    async clearCart() {
        if (!confirm('Are you sure you want to clear the cart?')) return;

        try {
            const response = await window.apiService.clearCart();

            if (response.success) {
                await this.loadCart();
                this.updateCartUI();
                this.renderCartSidebar();
                this.showNotification('Cart cleared', 'info');
            } else {
                this.showNotification(response.message || 'Failed to clear cart', 'error');
            }
        } catch (error) {
            console.error('Failed to clear cart:', error);
            this.showNotification('Failed to clear cart', 'error');
        }
    }

    updateCartUI() {
        const cartCountElements = document.querySelectorAll('.cart-count');
        const count = this.getItemCount();

        cartCountElements.forEach(element => {
            element.textContent = count;
            element.style.display = count > 0 ? 'flex' : 'none';
        });
    }

    renderCartSidebar() {
        const cartItemsContainer = document.getElementById('cartItemsContainer');
        const emptyCartMessage = document.getElementById('emptyCartMessage');
        const cartTemplate = document.getElementById('cartItemTemplate');

        if (!cartItemsContainer || !cartTemplate) return;

        // Clear existing items (except template and empty message)
        const existingItems = cartItemsContainer.querySelectorAll('.cart-item');
        existingItems.forEach(item => item.remove());

        // Show/hide empty message
        if (!this.cartData || !this.cartData.items || this.cartData.items.length === 0) {
            if (emptyCartMessage) emptyCartMessage.style.display = 'flex';
            this.updateCartTotals(0, 0, 0);
            return;
        }

        if (emptyCartMessage) emptyCartMessage.style.display = 'none';

        // Render cart items
        this.cartData.items.forEach(item => {
            const cartItemElement = cartTemplate.content.cloneNode(true);
            const itemDiv = cartItemElement.querySelector('.cart-item');

            // Set item ID
            itemDiv.setAttribute('data-item-id', item.id);

            // Set product image
            const img = cartItemElement.querySelector('.cart-item-image');
            if (img) {
                img.src = item.productImageUrl || '/images/placeholder.jpg';
                img.alt = item.productName || 'Product';
            }

            // Set product name
            const nameEl = cartItemElement.querySelector('.cart-item-name');
            if (nameEl) nameEl.textContent = item.productName || 'Product';

            // Set size and grind
            const sizeEl = cartItemElement.querySelector('.cart-item-size');
            if (sizeEl) {
                sizeEl.textContent = item.selectedSize ? `Size: ${item.selectedSize}` : '';
                sizeEl.style.display = item.selectedSize ? 'block' : 'none';
            }

            const grindEl = cartItemElement.querySelector('.cart-item-grind');
            if (grindEl) {
                grindEl.textContent = item.selectedGrind ? `Grind: ${item.selectedGrind}` : '';
                grindEl.style.display = item.selectedGrind ? 'block' : 'none';
            }

            // Set quantity
            const quantityInput = cartItemElement.querySelector('.cart-item-quantity');
            if (quantityInput) quantityInput.value = item.quantity;

            // Set price
            const priceEl = cartItemElement.querySelector('.cart-item-price');
            if (priceEl) {
                const price = item.subtotal || (item.unitPrice * item.quantity);
                priceEl.textContent = this.formatPrice(price);
            }

            // Attach event listeners
            const decreaseBtn = cartItemElement.querySelector('.cart-decrease-btn');
            const increaseBtn = cartItemElement.querySelector('.cart-increase-btn');
            const removeBtn = cartItemElement.querySelector('.cart-remove-btn');

            if (decreaseBtn) {
                decreaseBtn.addEventListener('click', () => {
                    if (item.quantity > 1) {
                        this.updateQuantity(item.id, item.quantity - 1);
                    }
                });
            }

            if (increaseBtn) {
                increaseBtn.addEventListener('click', () => {
                    this.updateQuantity(item.id, item.quantity + 1);
                });
            }

            if (removeBtn) {
                removeBtn.addEventListener('click', () => {
                    this.removeItem(item.id);
                });
            }

            cartItemsContainer.appendChild(cartItemElement);
        });

        // Update totals
        this.updateCartTotals(
            this.cartData.subtotal || 0,
            this.cartData.shippingFee || 0,
            this.cartData.total || 0
        );
    }

    updateCartTotals(subtotal, shippingFee, total) {
        const subtotalEl = document.getElementById('cartSubtotal');
        const shippingEl = document.getElementById('shippingFee');
        const totalEl = document.getElementById('cartTotal');

        if (subtotalEl) subtotalEl.textContent = this.formatPrice(subtotal);
        if (shippingEl) shippingEl.textContent = this.formatPrice(shippingFee);
        if (totalEl) totalEl.textContent = this.formatPrice(total);
    }

    formatPrice(amount) {
        if (typeof amount !== 'number') amount = 0;
        return new Intl.NumberFormat('vi-VN', {
            style: 'decimal',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount) + 'đ';
    }

    showNotification(message, type = 'info') {
        // Reuse existing notification logic if available
        if (window.cart && window.cart.showNotification) {
            window.cart.showNotification(message, type);
            return;
        }

        // Create notification element
        const notification = document.createElement('div');
        notification.className = `cart-notification ${type}`;

        let bgColor = '#10B981'; // success
        if (type === 'error') bgColor = '#EF4444';
        else if (type === 'info') bgColor = '#3B82F6';

        notification.style.cssText = `
            position: fixed;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%) translateY(100px);
            z-index: 10000;
            background: ${bgColor};
            color: white;
            padding: 16px 24px;
            border-radius: 8px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
            display: flex;
            align-items: center;
            gap: 12px;
            font-weight: 500;
            min-width: 320px;
            transition: transform 0.3s ease;
        `;

        notification.innerHTML = `
            <i class="fas fa-check-circle" style="font-size: 20px;"></i>
            <span>${message}</span>
        `;

        document.body.appendChild(notification);

        // Slide up animation
        setTimeout(() => {
            notification.style.transform = 'translateX(-50%) translateY(0)';
        }, 10);

        // Remove after 3 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(-50%) translateY(100px)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
}

// Initialize API-based cart
const apiCart = new ApiShoppingCart();

// Override global cart functions to use API
window.addToCart = function(productId, quantity = 1, size = null, grind = null) {
    apiCart.addItem(productId, quantity, size, grind);
};

window.removeFromCart = function(cartItemId) {
    apiCart.removeItem(cartItemId);
};

window.updateCartQuantity = function(cartItemId, quantity) {
    apiCart.updateQuantity(cartItemId, quantity);
};

window.clearCart = function() {
    apiCart.clearCart();
};

// Update cart sidebar rendering function
const originalOpenCartSidebar = window.openCartSidebar;
window.openCartSidebar = function() {
    if (originalOpenCartSidebar) {
        originalOpenCartSidebar.call(this);
    }
    // Render cart from API data
    apiCart.renderCartSidebar();
};

// Coupon/Voucher management functions
window.applyCoupon = async function(couponCode) {
    if (!couponCode || !couponCode.trim()) {
        alert('Please enter a coupon code');
        return;
    }

    try {
        const response = await window.apiService.applyCouponToCart(couponCode.trim());

        if (response.success) {
            // Reload cart to show discount
            await apiCart.loadCart();
            apiCart.updateCartUI();
            apiCart.renderCartSidebar();
            apiCart.showNotification(response.message || 'Coupon applied successfully!', 'success');
        } else {
            apiCart.showNotification(response.message || 'Invalid coupon code', 'error');
        }
    } catch (error) {
        console.error('Error applying coupon:', error);
        apiCart.showNotification('Failed to apply coupon', 'error');
    }
};

window.removeCoupon = async function() {
    try {
        const response = await window.apiService.removeCouponFromCart();

        if (response.success) {
            // Reload cart to remove discount
            await apiCart.loadCart();
            apiCart.updateCartUI();
            apiCart.renderCartSidebar();
            apiCart.showNotification('Coupon removed', 'info');
        } else {
            apiCart.showNotification(response.message || 'Failed to remove coupon', 'error');
        }
    } catch (error) {
        console.error('Error removing coupon:', error);
        apiCart.showNotification('Failed to remove coupon', 'error');
    }
};

window.loadAvailableVouchers = async function() {
    try {
        const response = await window.apiService.getAvailableCoupons(1, 20);

        if (response.success && response.data && response.data.items) {
            return response.data.items;
        }
        return [];
    } catch (error) {
        console.error('Error loading vouchers:', error);
        return [];
    }
};

console.log('API-based cart loaded successfully');
