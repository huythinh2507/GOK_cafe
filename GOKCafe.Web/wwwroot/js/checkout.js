// Checkout Page Management

// Use SHIPPING_FEE from cart.js (already declared globally)
// const SHIPPING_FEE is declared in cart.js

// Initialize checkout page
document.addEventListener('DOMContentLoaded', function() {
    loadCheckoutData();
    loadBankConfigs();
    initializePaymentMethodSelection();
});

/**
 * Load cart data and display in checkout
 */
function loadCheckoutData() {
    // Get cart from localStorage
    const cartData = localStorage.getItem('gok_cart');
    console.log('Cart data from localStorage:', cartData);

    if (!cartData) {
        showEmptyCartMessage();
        return;
    }

    const cart = JSON.parse(cartData);
    console.log('Parsed cart items:', cart);

    if (cart.length === 0) {
        showEmptyCartMessage();
        return;
    }

    renderCheckoutItems(cart);
    calculateCheckoutTotals(cart);
}

/**
 * Render cart items in checkout summary
 */
function renderCheckoutItems(cartItems) {
    const container = document.getElementById('checkoutCartItems');
    console.log('Container:', container);
    console.log('Cart items to render:', cartItems);

    if (!container) {
        console.error('checkoutCartItems container not found!');
        return;
    }

    container.innerHTML = '';

    cartItems.forEach((item, index) => {
        console.log(`Rendering item ${index}:`, item);

        const itemElement = document.createElement('div');
        itemElement.className = 'flex items-start gap-3 pb-3 border-b border-gray-100';

        const price = parseInt(item.price) || 32000;
        const totalPrice = price * item.quantity;

        console.log(`Item ${index} - name: ${item.name}, price: ${price}, quantity: ${item.quantity}`);

        itemElement.innerHTML = `
            <div class="w-16 h-16 bg-gray-100 rounded overflow-hidden flex-shrink-0">
                <img src="${item.imageUrl || '/images/placeholder-product.jpg'}"
                     alt="${item.name || 'Product'}"
                     class="w-full h-full object-cover" />
            </div>
            <div class="flex-1 min-w-0">
                <h4 class="text-sm font-medium text-gray-900 truncate">${item.name || 'Product'}</h4>
                <p class="text-xs text-gray-500">Qty: ${item.quantity}</p>
                ${item.packaging ? `<p class="text-xs text-gray-500">${item.packaging}</p>` : ''}
            </div>
            <div class="text-sm font-medium text-gray-900">${formatCurrency(totalPrice)}</div>
        `;

        container.appendChild(itemElement);
    });

    console.log('Finished rendering items. Container HTML:', container.innerHTML);
}

/**
 * Calculate and display checkout totals
 */
function calculateCheckoutTotals(cartItems) {
    let subtotal = 0;

    cartItems.forEach(item => {
        const price = parseInt(item.price) || 32000;
        subtotal += price * item.quantity;
    });

    const shipping = subtotal > 0 ? SHIPPING_FEE : 0;

    // Load discount from localStorage
    const discountData = localStorage.getItem('gok_cart_discount');
    let totalDiscount = 0;

    if (discountData) {
        try {
            const discount = JSON.parse(discountData);
            totalDiscount = (discount.couponDiscount || 0) + (discount.voucherDiscount || 0);

            // Show discount row if there's a discount
            if (totalDiscount > 0) {
                const discountRow = document.getElementById('checkoutDiscountRow');
                if (discountRow) {
                    discountRow.classList.remove('hidden');
                    document.getElementById('checkoutDiscount').textContent = '-' + formatCurrency(totalDiscount);
                }
            }
        } catch (e) {
            console.error('Error parsing discount data:', e);
        }
    }

    const total = Math.max(0, subtotal + shipping - totalDiscount);

    document.getElementById('checkoutSubtotal').textContent = formatCurrency(subtotal);
    document.getElementById('checkoutShipping').textContent = formatCurrency(shipping);
    document.getElementById('checkoutTotal').textContent = formatCurrency(total);
}

/**
 * Show empty cart message and redirect
 */
function showEmptyCartMessage() {
    alert('Your cart is empty. Redirecting to shop...');
    window.location.href = '/';
}

/**
 * Load available bank configurations
 */
async function loadBankConfigs() {
    try {
        const response = await window.apiService.getActiveBankConfigs();

        if (response.success && response.data && response.data.length > 0) {
            const banks = response.data;
            populateBankSelect(banks);
        } else {
            // No banks configured, use default for testing
            console.warn('No bank configs found, using default');
            populateBankSelect([
                {
                    bankCode: '970422',
                    bankName: 'MB Bank',
                    accountNumber: '0123456789'
                }
            ]);
        }
    } catch (error) {
        console.error('Error loading bank configs:', error);
        // Fallback to default bank for development
        populateBankSelect([
            {
                bankCode: '970422',
                bankName: 'MB Bank (Default)',
                accountNumber: '0123456789'
            }
        ]);
    }
}

/**
 * Populate bank select dropdown
 */
function populateBankSelect(banks) {
    const select = document.getElementById('bankSelect');
    if (!select) return;

    select.innerHTML = '<option value="">Select a bank</option>';

    banks.forEach(bank => {
        const option = document.createElement('option');
        option.value = bank.bankCode;
        option.textContent = `${bank.bankName} - ${bank.accountNumber}`;
        select.appendChild(option);
    });
}

/**
 * Initialize payment method selection handlers
 */
function initializePaymentMethodSelection() {
    const paymentMethodOptions = document.querySelectorAll('input[name="paymentMethod"]');
    const bankContainer = document.getElementById('bankSelectionContainer');

    paymentMethodOptions.forEach(option => {
        option.addEventListener('change', function() {
            // Update visual selection
            document.querySelectorAll('.payment-method-option').forEach(el => {
                el.classList.remove('border-primary', 'bg-blue-50');
            });
            this.closest('.payment-method-option').classList.add('border-primary', 'bg-blue-50');

            // Show/hide bank selection
            if (this.value === '4') { // Bank Transfer
                bankContainer.classList.remove('hidden');
            } else {
                bankContainer.classList.add('hidden');
            }
        });
    });
}

/**
 * Place order and process payment
 */
async function placeOrder() {
    // Validate form
    if (!validateCheckoutForm()) {
        return;
    }

    // Get form data
    const customerName = document.getElementById('customerName').value.trim();
    const customerEmail = document.getElementById('customerEmail').value.trim();
    const customerPhone = document.getElementById('customerPhone').value.trim();
    const shippingAddress = document.getElementById('shippingAddress').value.trim();
    const orderNotes = document.getElementById('orderNotes').value.trim();

    // Get selected payment method
    const paymentMethodValue = document.querySelector('input[name="paymentMethod"]:checked').value;
    const paymentMethod = paymentMethodValue === '0' ? 'Cash' : 'BankTransfer';

    // Get bank code if bank transfer is selected
    let bankCode = null;
    if (paymentMethodValue === '4') {
        bankCode = document.getElementById('bankSelect').value;
        if (!bankCode) {
            alert('Please select a bank for bank transfer');
            return;
        }
    }

    // Get cart items
    const cartData = localStorage.getItem('gok_cart');
    if (!cartData) {
        alert('Your cart is empty');
        return;
    }

    const cartItems = JSON.parse(cartData);

    // Calculate totals
    let subtotal = 0;
    cartItems.forEach(item => {
        const price = parseInt(item.price) || 32000;
        subtotal += price * item.quantity;
    });
    const shipping = SHIPPING_FEE;

    // Get discount from localStorage
    const discountData = localStorage.getItem('gok_cart_discount');
    let totalDiscount = 0;
    let couponCode = null;
    let voucherId = null;

    if (discountData) {
        try {
            const discount = JSON.parse(discountData);
            totalDiscount = (discount.couponDiscount || 0) + (discount.voucherDiscount || 0);
            couponCode = discount.couponCode;
            voucherId = discount.voucherId;
        } catch (e) {
            console.error('Error parsing discount data:', e);
        }
    }

    const total = Math.max(0, subtotal + shipping - totalDiscount);

    // Prepare checkout data
    const checkoutData = {
        customerName: customerName,
        customerEmail: customerEmail,
        customerPhone: customerPhone,
        shippingAddress: shippingAddress,
        notes: orderNotes,
        paymentMethod: paymentMethod,
        shippingFee: shipping
    };

    // Show loading state
    const placeOrderBtn = event.target;
    placeOrderBtn.disabled = true;
    placeOrderBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Processing...';

    try {
        // First, sync cart items to backend
        const sessionId = window.apiService.sessionId;

        // Add each cart item to backend cart
        for (const item of cartItems) {
            try {
                await window.apiService.addToCart({
                    productId: item.productId,
                    quantity: item.quantity,
                    selectedSize: item.packaging,
                    selectedGrind: item.grind
                });
            } catch (error) {
                console.error('Error adding item to backend cart:', error);
            }
        }

        // Call checkout API
        const response = await window.apiService.checkout(checkoutData);

        if (response.success && response.data) {
            const order = response.data;

            // Clear cart and discount after successful order
            localStorage.removeItem('gok_cart');
            localStorage.removeItem('gok_cart_discount');

            // If payment method is bank transfer, initiate payment
            if (paymentMethod === 'BankTransfer') {
                await initiatePayment(order.id, paymentMethod, bankCode);
            } else {
                // For COD, redirect to order confirmation
                window.location.href = `/order-confirmation?orderId=${order.id}`;
            }
        } else {
            alert(response.message || 'Failed to create order. Please try again.');
            placeOrderBtn.disabled = false;
            placeOrderBtn.innerHTML = '<i class="fas fa-check-circle mr-2"></i>Place Order';
        }
    } catch (error) {
        console.error('Error placing order:', error);
        alert('An error occurred while placing your order. Please try again.');
        placeOrderBtn.disabled = false;
        placeOrderBtn.innerHTML = '<i class="fas fa-check-circle mr-2"></i>Place Order';
    }
}

/**
 * Initiate payment for order
 */
async function initiatePayment(orderId, paymentMethod, bankCode) {
    try {
        const result = await window.paymentManager.initializePayment(orderId, paymentMethod, bankCode);

        if (result.success && result.qrCodeInfo) {
            // Display QR code payment modal
            window.paymentManager.displayQRCodePayment(result.qrCodeInfo);
        } else {
            alert(result.message || 'Failed to initialize payment');
        }
    } catch (error) {
        console.error('Error initiating payment:', error);
        alert('An error occurred while initiating payment');
    }
}

/**
 * Validate checkout form
 */
function validateCheckoutForm() {
    const form = document.getElementById('checkoutForm');

    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }

    // Additional validation
    const email = document.getElementById('customerEmail').value;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        alert('Please enter a valid email address');
        return false;
    }

    const phone = document.getElementById('customerPhone').value;
    const phoneRegex = /^[0-9]{10,11}$/;
    if (!phoneRegex.test(phone)) {
        alert('Please enter a valid phone number (10-11 digits)');
        return false;
    }

    return true;
}

/**
 * Format currency (VND)
 */
function formatCurrency(amount) {
    return amount.toLocaleString('vi-VN') + 'đ';
}
