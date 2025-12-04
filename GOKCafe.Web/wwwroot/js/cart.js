// Shopping Cart Page JavaScript

// ========================================
// Cart Page Management
// ========================================
class CartPage {
    constructor() {
        this.cartData = null;
        this.init();
    }

    async loadCart() {
        try {
            const response = await window.apiService.getCart();
            if (response.success) {
                this.cartData = response.data;
                return this.cartData;
            }
        } catch (error) {
            console.error('Failed to load cart:', error);
            this.cartData = { items: [], subtotal: 0, total: 0 };
        }
        return this.cartData;
    }

    async init() {
        await this.loadCart();
        this.renderCart();
        this.attachEventListeners();
    }

    attachEventListeners() {
        // Quantity updates
        document.querySelectorAll('.cart-quantity-input').forEach(input => {
            input.addEventListener('change', (e) => {
                const cartItemId = e.target.dataset.cartItemId;
                const quantity = parseInt(e.target.value);
                this.updateQuantity(cartItemId, quantity);
            });
        });

        // Remove buttons
        document.querySelectorAll('.btn-remove-item').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const cartItemId = e.target.closest('.btn-remove-item').dataset.cartItemId;
                this.removeItem(cartItemId);
            });
        });

        // Clear cart button
        const clearCartBtn = document.querySelector('#clear-cart');
        if (clearCartBtn) {
            clearCartBtn.addEventListener('click', () => {
                if (confirm('Are you sure you want to clear the cart?')) {
                    this.clearCart();
                }
            });
        }

        // Proceed to checkout
        const checkoutBtn = document.querySelector('#proceed-checkout');
        if (checkoutBtn) {
            checkoutBtn.addEventListener('click', () => {
                this.proceedToCheckout();
            });
        }
    }

    async renderCart() {
        const cartContainer = document.querySelector('#cart-items-container');
        if (!cartContainer) return;

        if (!this.cartData || this.cartData.items.length === 0) {
            cartContainer.innerHTML = `
                <div class="alert alert-info text-center">
                    <i class="fas fa-shopping-cart fa-3x mb-3"></i>
                    <h4>Your cart is empty</h4>
                    <p>Add some products to get started!</p>
                    <a href="/products" class="btn btn-primary">Browse Products</a>
                </div>
            `;
            this.updateCartSummary(0, 0, 0, 0);
            return;
        }

        const cartHTML = this.cartData.items.map(item => this.renderCartItem(item)).join('');
        cartContainer.innerHTML = cartHTML;

        this.updateCartSummary(
            this.cartData.subtotal || 0,
            this.cartData.tax || 0,
            this.cartData.shipping || 0,
            this.cartData.total || 0
        );
        this.attachEventListeners();
    }

    renderCartItem(item) {
        const productImage = item.productImageUrl || '/images/placeholder-product.jpg';
        const productName = item.productName || 'Product Name';
        const unitPrice = item.unitPrice || 0;
        const subtotal = item.subtotal || (unitPrice * item.quantity);

        return `
            <div class="cart-item mb-3" data-cart-item-id="${item.id}">
                <div class="row align-items-center">
                    <div class="col-md-2">
                        <img src="${productImage}" alt="${productName}" class="img-fluid rounded" />
                    </div>
                    <div class="col-md-4">
                        <h5>${productName}</h5>
                        <p class="text-muted">${item.productDescription || ''}</p>
                    </div>
                    <div class="col-md-2">
                        <p class="mb-0"><strong>$${unitPrice.toFixed(2)}</strong></p>
                    </div>
                    <div class="col-md-2">
                        <input type="number" class="form-control cart-quantity-input"
                               value="${item.quantity}" min="1" max="99"
                               data-cart-item-id="${item.id}" />
                    </div>
                    <div class="col-md-1">
                        <p class="mb-0"><strong>$${subtotal.toFixed(2)}</strong></p>
                    </div>
                    <div class="col-md-1">
                        <button class="btn btn-sm btn-danger btn-remove-item"
                                data-cart-item-id="${item.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    async updateQuantity(cartItemId, quantity) {
        try {
            const response = await window.apiService.updateCartItem(cartItemId, quantity);
            if (response.success) {
                this.cartData = response.data;
                this.renderCart();
                this.showNotification('Cart updated', 'success');
            }
        } catch (error) {
            console.error('Failed to update cart:', error);
            this.showNotification('Failed to update cart', 'danger');
        }
    }

    async removeItem(cartItemId) {
        try {
            const response = await window.apiService.removeCartItem(cartItemId);
            if (response.success) {
                await this.loadCart();
                this.renderCart();
                this.showNotification('Item removed from cart', 'info');
            }
        } catch (error) {
            console.error('Failed to remove item:', error);
            this.showNotification('Failed to remove item', 'danger');
        }
    }

    async clearCart() {
        try {
            const response = await window.apiService.clearCart();
            if (response.success) {
                await this.loadCart();
                this.renderCart();
                this.showNotification('Cart cleared', 'info');
            }
        } catch (error) {
            console.error('Failed to clear cart:', error);
            this.showNotification('Failed to clear cart', 'danger');
        }
    }

    updateCartSummary(subtotal, tax = 0, shipping = 0, total = 0) {
        const subtotalEl = document.querySelector('#cart-subtotal');
        const taxEl = document.querySelector('#cart-tax');
        const shippingEl = document.querySelector('#cart-shipping');
        const totalEl = document.querySelector('#cart-total');

        if (subtotalEl) subtotalEl.textContent = `$${subtotal.toFixed(2)}`;
        if (taxEl) taxEl.textContent = `$${tax.toFixed(2)}`;
        if (shippingEl) shippingEl.textContent = shipping === 0 ? 'FREE' : `$${shipping.toFixed(2)}`;
        if (totalEl) totalEl.textContent = `$${total.toFixed(2)}`;
    }

    proceedToCheckout() {
        if (this.cartItems.length === 0) {
            alert('Your cart is empty!');
            return;
        }

        // TODO: Implement checkout process
        console.log('Proceeding to checkout with items:', this.cartItems);
        window.location.href = '/checkout';
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} cart-notification`;
        notification.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.remove();
        }, 3000);
    }
}

// Initialize cart page if on cart page
if (document.querySelector('.cart-page')) {
    const cartPage = new CartPage();
}

// ========================================
// Coupon Code Application
// ========================================
class CouponManager {
    constructor() {
        this.appliedCoupon = null;
        this.init();
    }

    init() {
        const applyCouponBtn = document.querySelector('#apply-coupon');
        if (applyCouponBtn) {
            applyCouponBtn.addEventListener('click', () => {
                this.applyCoupon();
            });
        }

        const removeCouponBtn = document.querySelector('#remove-coupon');
        if (removeCouponBtn) {
            removeCouponBtn.addEventListener('click', () => {
                this.removeCoupon();
            });
        }
    }

    async applyCoupon() {
        const couponInput = document.querySelector('#coupon-code');
        const couponCode = couponInput ? couponInput.value.trim() : '';

        if (!couponCode) {
            alert('Please enter a coupon code');
            return;
        }

        // TODO: Validate coupon with API
        console.log('Applying coupon:', couponCode);

        // Placeholder success
        this.appliedCoupon = {
            code: couponCode,
            discount: 10, // 10% discount
            type: 'percentage'
        };

        this.updateCouponUI();
        alert('Coupon applied successfully!');
    }

    removeCoupon() {
        this.appliedCoupon = null;
        this.updateCouponUI();
        alert('Coupon removed');
    }

    updateCouponUI() {
        const couponDisplay = document.querySelector('#applied-coupon');
        const discountAmount = document.querySelector('#discount-amount');

        if (this.appliedCoupon) {
            if (couponDisplay) {
                couponDisplay.textContent = this.appliedCoupon.code;
                couponDisplay.parentElement.style.display = 'block';
            }
            if (discountAmount) {
                discountAmount.textContent = `-$${this.appliedCoupon.discount.toFixed(2)}`;
            }
        } else {
            if (couponDisplay) {
                couponDisplay.parentElement.style.display = 'none';
            }
            if (discountAmount) {
                discountAmount.textContent = '$0.00';
            }
        }
    }
}

// Initialize coupon manager
const couponManager = new CouponManager();

console.log('Cart scripts loaded successfully');
