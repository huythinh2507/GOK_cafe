// Main JavaScript for GOK Cafe

// ========================================
// Shopping Cart Management
// ========================================
class ShoppingCart {
    constructor() {
        this.items = this.loadCart();
        this.updateCartUI();
    }

    loadCart() {
        const cart = localStorage.getItem('gok_cart');
        return cart ? JSON.parse(cart) : [];
    }

    saveCart() {
        localStorage.setItem('gok_cart', JSON.stringify(this.items));
        this.updateCartUI();
    }

    addItem(productId, quantity = 1) {
        const existingItem = this.items.find(item => item.productId === productId);

        if (existingItem) {
            existingItem.quantity += quantity;
        } else {
            this.items.push({
                productId: productId,
                quantity: quantity,
                addedAt: new Date().toISOString()
            });
        }

        this.saveCart();
        this.showNotification('Product added to cart!', 'success');
    }

    removeItem(productId) {
        this.items = this.items.filter(item => item.productId !== productId);
        this.saveCart();
        this.showNotification('Product removed from cart', 'info');
    }

    updateQuantity(productId, quantity) {
        const item = this.items.find(item => item.productId === productId);
        if (item) {
            item.quantity = quantity;
            if (quantity <= 0) {
                this.removeItem(productId);
            } else {
                this.saveCart();
            }
        }
    }

    getItemCount() {
        return this.items.reduce((total, item) => total + item.quantity, 0);
    }

    clearCart() {
        this.items = [];
        this.saveCart();
        this.showNotification('Cart cleared', 'info');
    }

    updateCartUI() {
        const cartCountElements = document.querySelectorAll('.cart-count');
        const count = this.getItemCount();

        cartCountElements.forEach(element => {
            element.textContent = count;
            element.style.display = count > 0 ? 'flex' : 'none';
        });
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `cart-notification ${type}`;

        // Set background color based on type
        let bgColor = '#10B981'; // success - green
        let icon = 'fa-check-circle';
        if (type === 'error') {
            bgColor = '#EF4444'; // error - red
            icon = 'fa-exclamation-circle';
        } else if (type === 'info') {
            bgColor = '#3B82F6'; // info - blue
            icon = 'fa-info-circle';
        }

        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            transform: translateX(400px);
            z-index: 10000;
            background: ${bgColor};
            color: white;
            padding: 16px 24px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            display: flex;
            align-items: center;
            gap: 12px;
            font-weight: 500;
            min-width: 320px;
            max-width: 400px;
            transition: transform 0.3s ease;
        `;
        notification.innerHTML = `
            <i class="fas ${icon}" style="font-size: 20px;"></i>
            <span>${message}</span>
        `;

        document.body.appendChild(notification);

        // Slide in animation from right
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 10);

        // Remove after 3 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(400px)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
}

// Initialize cart
const cart = new ShoppingCart();

// ========================================
// Global Functions
// ========================================
window.addToCart = function(productId, quantity = 1) {
    cart.addItem(productId, quantity);
};

window.removeFromCart = function(productId) {
    cart.removeItem(productId);
};

window.updateCartQuantity = function(productId, quantity) {
    cart.updateQuantity(productId, quantity);
};

window.clearCart = function() {
    if (confirm('Are you sure you want to clear the cart?')) {
        cart.clearCart();
    }
};

// ========================================
// Smooth Scroll
// ========================================
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const href = this.getAttribute('href');
        if (href !== '#' && document.querySelector(href)) {
            e.preventDefault();
            document.querySelector(href).scrollIntoView({
                behavior: 'smooth'
            });
        }
    });
});

// ========================================
// Navbar Scroll Effect
// ========================================
let lastScroll = 0;
const navbar = document.querySelector('.site-header');

window.addEventListener('scroll', () => {
    if (!navbar) return; // Guard against null navbar

    const currentScroll = window.pageYOffset;

    if (currentScroll > 100) {
        navbar.classList.add('scrolled');
    } else {
        navbar.classList.remove('scrolled');
    }

    lastScroll = currentScroll;
});

// ========================================
// Image Lazy Loading
// ========================================
if ('IntersectionObserver' in window) {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                if (img.dataset.src) {
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            }
        });
    });

    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });
}

// ========================================
// Form Validation
// ========================================
const forms = document.querySelectorAll('.needs-validation');

forms.forEach(form => {
    form.addEventListener('submit', event => {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }

        form.classList.add('was-validated');
    }, false);
});

// ========================================
// Animations on Scroll
// ========================================
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('fade-in');
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

document.querySelectorAll('.product-card, .category-card').forEach(el => {
    observer.observe(el);
});

// ========================================
// Search Functionality
// ========================================
const searchInput = document.querySelector('input[name="search"]');
if (searchInput) {
    let searchTimeout;

    searchInput.addEventListener('input', function() {
        clearTimeout(searchTimeout);

        searchTimeout = setTimeout(() => {
            // Could implement live search here
            console.log('Searching for:', this.value);
        }, 500);
    });
}

// ========================================
// Console Welcome Message
// ========================================
console.log('%c Welcome to GOK Cafe! ☕', 'color: #6f42c1; font-size: 20px; font-weight: bold;');
console.log('%c Built with Umbraco CMS', 'color: #666; font-size: 14px;');

// ========================================
// CSS Animations
// ========================================
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }

    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }

    .site-header.scrolled {
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    }
`;
document.head.appendChild(style);

// ========================================
// Cart Sidebar Controls
// ========================================
window.toggleCartSidebar = function() {
    const sidebar = document.getElementById('cartSidebar');
    const overlay = document.getElementById('cartOverlay');

    if (sidebar.classList.contains('open')) {
        closeCartSidebar();
    } else {
        openCartSidebar();
    }
};

window.openCartSidebar = function() {
    const sidebar = document.getElementById('cartSidebar');
    const overlay = document.getElementById('cartOverlay');

    // Show overlay
    overlay.classList.remove('hidden');
    overlay.classList.add('show');

    // Open sidebar with animation
    setTimeout(() => {
        sidebar.classList.add('open');
    }, 10);

    // Prevent body scroll when sidebar is open
    document.body.style.overflow = 'hidden';

    // Render cart items
    renderCartSidebar();

    // Load available vouchers
    if (typeof loadVouchers === 'function') {
        loadVouchers();
    }
};

window.closeCartSidebar = function() {
    const sidebar = document.getElementById('cartSidebar');
    const overlay = document.getElementById('cartOverlay');

    // Close sidebar
    sidebar.classList.remove('open');

    // Hide overlay after animation
    setTimeout(() => {
        overlay.classList.remove('show');
        overlay.classList.add('hidden');
    }, 300);

    // Re-enable body scroll
    document.body.style.overflow = '';
};

window.proceedToCheckout = function() {
    if (cart.getItemCount() === 0) {
        alert('Your cart is empty!');
        return;
    }
    window.location.href = '/checkout';
};

window.buyNow = function() {
    if (cart.getItemCount() === 0) {
        alert('Your cart is empty!');
        return;
    }
    window.location.href = '/checkout?buyNow=true';
};

window.toggleOrderForSomeoneElse = function() {
    const form = document.getElementById('orderForSomeoneElseForm');
    form.classList.toggle('hidden');
};

// ========================================
// Cart Sidebar Rendering
// ========================================
function renderCartSidebar() {
    const container = document.getElementById('cartItemsContainer');
    const emptyMessage = document.getElementById('emptyCartMessage');
    const subtotalElement = document.getElementById('cartSubtotal');

    if (cart.items.length === 0) {
        emptyMessage.classList.remove('hidden');
        // Hide all cart items
        const existingItems = container.querySelectorAll('.cart-item');
        existingItems.forEach(item => item.remove());
        subtotalElement.textContent = '0đ';
        updateCartTotal();
        return;
    }

    emptyMessage.classList.add('hidden');

    // Clear existing items (except empty message and template)
    const existingItems = container.querySelectorAll('.cart-item');
    existingItems.forEach(item => item.remove());

    // Render each cart item
    let subtotal = 0;
    cart.items.forEach(item => {
        const itemElement = createCartItemElement(item);
        container.insertBefore(itemElement, emptyMessage);

        // Calculate subtotal
        const priceNum = parseInt(item.price) || 32000;
        subtotal += priceNum * item.quantity;
    });

    // Update subtotal
    subtotalElement.textContent = formatPriceVND(subtotal);

    // Update total with shipping and discount
    updateCartTotal();

    // Render special price products (mock data for now)
    renderSpecialPriceProducts();
}

function createCartItemElement(item) {
    const template = document.getElementById('cartItemTemplate');
    const clone = template.content.cloneNode(true);
    const div = clone.querySelector('.cart-item');

    // Set data attributes
    div.setAttribute('data-item-id', item.productId);

    // Set image
    const img = clone.querySelector('.cart-item-image');
    img.src = item.imageUrl || '/images/placeholder-product.jpg';
    img.alt = item.name || 'Product';

    // Set name
    const name = clone.querySelector('.cart-item-name');
    name.textContent = item.name || 'Product';

    // Set options (packaging, grind, etc.)
    const sizeSpan = clone.querySelector('.cart-item-size');
    const grindSpan = clone.querySelector('.cart-item-grind');
    if (item.packaging) {
        sizeSpan.textContent = `Size: ${item.packaging}`;
    } else {
        sizeSpan.style.display = 'none';
    }
    if (item.grind) {
        grindSpan.textContent = `Grind: ${item.grind}`;
    } else {
        grindSpan.style.display = 'none';
    }

    // Set quantity
    const quantityInput = clone.querySelector('.cart-item-quantity');
    quantityInput.value = item.quantity;

    // Set price
    const priceElement = clone.querySelector('.cart-item-price');
    const originalPriceElement = clone.querySelector('.cart-item-original-price');

    console.log('Cart item data:', {
        name: item.name,
        price: item.price,
        originalPrice: item.originalPrice,
        quantity: item.quantity
    });

    // Check if item has both original and discount price
    if (item.originalPrice && item.price && item.originalPrice > item.price) {
        // Show both prices
        const originalPriceNum = parseInt(item.originalPrice) || 0;
        const discountPriceNum = parseInt(item.price) || 0;
        const originalItemPrice = originalPriceNum * item.quantity;
        const discountItemPrice = discountPriceNum * item.quantity;

        console.log('Displaying discount prices:', {
            original: originalItemPrice,
            discount: discountItemPrice
        });

        originalPriceElement.textContent = formatPrice(originalItemPrice);
        originalPriceElement.classList.remove('hidden');
        priceElement.textContent = formatPrice(discountItemPrice);
        priceElement.classList.add('text-primary');
    } else {
        // Show only one price
        const priceNum = parseInt(item.price) || 32000;
        const itemPrice = priceNum * item.quantity;
        priceElement.textContent = formatPrice(itemPrice);
        originalPriceElement.classList.add('hidden');
        priceElement.classList.remove('text-primary');
    }

    // Attach event listeners
    const decreaseBtn = clone.querySelector('.cart-decrease-btn');
    const increaseBtn = clone.querySelector('.cart-increase-btn');
    const removeBtn = clone.querySelector('.cart-remove-btn');

    decreaseBtn.addEventListener('click', () => {
        if (item.quantity > 1) {
            cart.updateQuantity(item.productId, item.quantity - 1);
            renderCartSidebar();
        }
    });

    increaseBtn.addEventListener('click', () => {
        cart.updateQuantity(item.productId, item.quantity + 1);
        renderCartSidebar();
    });

    removeBtn.addEventListener('click', () => {
        openRemoveProductModal(item.productId);
    });

    return clone;
}

function formatPrice(price) {
    return price.toLocaleString('en-US').replace(/,/g, '.') + ' VND';
}

// ========================================
// Special Price Products Rendering
// ========================================
function renderSpecialPriceProducts() {
    const container = document.getElementById('specialPriceProducts');
    if (!container) return;

    // Clear existing items
    container.innerHTML = '';

    // Mock data for special price products (replace with real data later)
    const specialProducts = [
        {
            id: '101',
            name: '75g Amruthavanam',
            price: 149,
            imageUrl: '/media/special1.jpg'
        },
        {
            id: '102',
            name: '75g Kalledevara',
            price: 149,
            imageUrl: '/media/special2.jpg'
        },
        {
            id: '103',
            name: '75g St. Joseph',
            price: 149,
            imageUrl: '/media/special3.jpg'
        }
    ];

    specialProducts.forEach(product => {
        const productElement = createSpecialPriceElement(product);
        container.appendChild(productElement);
    });
}

function createSpecialPriceElement(product) {
    const template = document.getElementById('specialPriceTemplate');
    const clone = template.content.cloneNode(true);

    // Set image
    const img = clone.querySelector('.special-product-image');
    img.src = product.imageUrl;
    img.alt = product.name;

    // Set name
    const name = clone.querySelector('.special-product-name');
    name.textContent = product.name;

    // Set price
    const price = clone.querySelector('.special-product-price');
    price.textContent = `₹${product.price}`;

    // Add click handler for add button
    const addBtn = clone.querySelector('.special-add-btn');
    addBtn.addEventListener('click', () => {
        // Add to cart logic
        cart.addItem(product.id, 1);
        renderCartSidebar();
        showNotificationMessage(`${product.name} added to cart!`, 'success');
    });

    return clone;
}

function showNotificationMessage(message, type = 'info') {
    if (cart && cart.showNotification) {
        cart.showNotification(message, type);
    } else {
        console.log(message);
    }
}

// Close sidebar when pressing Escape key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        const sidebar = document.getElementById('cartSidebar');
        if (sidebar && sidebar.classList.contains('open')) {
            closeCartSidebar();
        }

        const modal = document.getElementById('productModal');
        if (modal && modal.classList.contains('show')) {
            closeProductModal();
        }
    }
});

// ========================================
// Product Detail Modal
// ========================================
let currentProduct = null;
let selectedSize = null;
let selectedGrind = null;

window.openProductModal = async function(productId) {
    try {
        // Show loading state
        const modal = document.getElementById('productModal');
        const overlay = document.getElementById('productModalOverlay');

        // Show modal with loading state
        overlay.classList.remove('hidden');
        overlay.classList.add('show');
        modal.classList.remove('hidden');
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';

        // Show loading indicator
        document.getElementById('modalProductName').textContent = 'Loading...';

        // Fetch product data from API
        const response = await fetch(`/umbraco/api/ProductModalApi/${productId}`);

        if (!response.ok) {
            throw new Error('Failed to fetch product data');
        }

        const result = await response.json();

        if (!result.success || !result.data) {
            throw new Error(result.message || 'Product not found');
        }

        const productData = result.data;
        currentProduct = {
            id: productData.id,
            name: productData.name,
            price: productData.discountPrice || productData.price,
            originalPrice: productData.discountPrice ? productData.price : null,
            imageUrl: productData.imageUrl,
            description: productData.description,
            shortDescription: productData.shortDescription,
            categoryName: productData.categoryName,
            tastingNote: productData.tastingNote,
            region: productData.region,
            process: productData.process,
            stockQuantity: productData.stockQuantity,
            isActive: productData.isActive
        };

        // Populate modal with product data
        document.getElementById('modalProductName').textContent = currentProduct.name || 'Product Name';

        // Set category
        const categoryElement = document.getElementById('modalProductCategory');
        if (currentProduct.categoryName) {
            categoryElement.textContent = currentProduct.categoryName;
            categoryElement.classList.remove('hidden');
        } else {
            categoryElement.classList.add('hidden');
        }

        // Display price with discount if applicable
        const priceElement = document.getElementById('modalProductPrice');
        if (currentProduct.originalPrice) {
            priceElement.innerHTML = `
                <span class="text-gray-400 line-through text-xl mr-2">${currentProduct.originalPrice.toLocaleString('en-US')}</span>
                <span class="text-red-600">${currentProduct.price.toLocaleString('en-US')}</span>
            `;
        } else {
            priceElement.textContent = currentProduct.price.toLocaleString('en-US');
        }

        // Set short description
        const shortDescContainer = document.getElementById('modalShortDescriptionContainer');
        const shortDescElement = document.getElementById('modalShortDescription');
        if (currentProduct.shortDescription) {
            shortDescElement.textContent = currentProduct.shortDescription;
            shortDescContainer.classList.remove('hidden');
        } else {
            shortDescContainer.classList.add('hidden');
        }

        // Set product details (tasting note, region, process)
        const detailsContainer = document.getElementById('modalProductDetailsContainer');
        let hasDetails = false;

        // Tasting Note
        const tastingNoteContainer = document.getElementById('modalTastingNoteContainer');
        const tastingNoteElement = document.getElementById('modalTastingNote');
        if (currentProduct.tastingNote) {
            tastingNoteElement.textContent = currentProduct.tastingNote;
            tastingNoteContainer.classList.remove('hidden');
            hasDetails = true;
        } else {
            tastingNoteContainer.classList.add('hidden');
        }

        // Region
        const regionContainer = document.getElementById('modalRegionContainer');
        const regionElement = document.getElementById('modalRegion');
        if (currentProduct.region) {
            regionElement.textContent = currentProduct.region;
            regionContainer.classList.remove('hidden');
            hasDetails = true;
        } else {
            regionContainer.classList.add('hidden');
        }

        // Process
        const processContainer = document.getElementById('modalProcessContainer');
        const processElement = document.getElementById('modalProcess');
        if (currentProduct.process) {
            processElement.textContent = currentProduct.process;
            processContainer.classList.remove('hidden');
            hasDetails = true;
        } else {
            processContainer.classList.add('hidden');
        }

        // Show or hide details container
        if (hasDetails) {
            detailsContainer.classList.remove('hidden');
        } else {
            detailsContainer.classList.add('hidden');
        }

        // Set images
        document.getElementById('modalMainImage').src = currentProduct.imageUrl || '/images/placeholder-product.jpg';
        document.getElementById('modalMainImage').alt = currentProduct.name || '';

        // Render size options (default options, can be customized later)
        renderSizeOptions(['250g', '500g', '1kg']);

        // Render grind options (default options, can be customized later)
        renderGrindOptions(['Whole Bean', 'French Press', 'Filter', 'Espresso']);

        // Reset quantity
        document.getElementById('modalQuantity').value = 1;

    } catch (error) {
        console.error('Error loading product:', error);

        // Show error message
        document.getElementById('modalProductName').textContent = 'Error loading product';
        cart.showNotification('Failed to load product details. Please try again.', 'error');

        // Close modal after delay
        setTimeout(() => {
            closeProductModal();
        }, 2000);
    }
};

window.closeProductModal = function() {
    const modal = document.getElementById('productModal');
    const overlay = document.getElementById('productModalOverlay');
    // Add 'hide' so the fade-out animation plays while the element still has display styles from 'show'
    modal.classList.add('hide');
    overlay.classList.add('hide');

    // After animation ends, remove both 'show' and 'hide', then add 'hidden' to fully hide elements
    setTimeout(() => {
        modal.classList.remove('show');
        overlay.classList.remove('show');

        modal.classList.remove('hide');
        overlay.classList.remove('hide');

        modal.classList.add('hidden');
        overlay.classList.add('hidden');

        // Re-enable body scroll
        document.body.style.overflow = '';

        // Reset selections
        currentProduct = null;
        selectedSize = null;
        selectedGrind = null;
    }, 300);
};

function renderSizeOptions(sizes) {
    const container = document.getElementById('modalSizeOptions');
    container.innerHTML = '';

    sizes.forEach((size, index) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'size-option px-6 py-2 border-2 border-gray-300 text-gray-700 hover:border-primary hover:text-primary transition-colors font-medium';
        button.textContent = size;
        button.dataset.size = size;

        // Set first as default
        if (index === 0) {
            button.classList.add('active');
            selectedSize = size;
        }

        button.addEventListener('click', () => {
            // Remove active from all
            container.querySelectorAll('.size-option').forEach(btn => {
                btn.classList.remove('active');
            });
            // Add active to clicked
            button.classList.add('active');
            selectedSize = size;
        });

        container.appendChild(button);
    });
}

function renderGrindOptions(grinds) {
    const container = document.getElementById('modalGrindOptions');
    container.innerHTML = '';

    grinds.forEach((grind, index) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'grind-option px-6 py-2 border-2 border-gray-300 text-gray-700 hover:border-primary hover:text-primary transition-colors font-medium';
        button.textContent = grind;
        button.dataset.grind = grind;

        // Set first as default
        if (index === 0) {
            button.classList.add('active');
            selectedGrind = grind;
        }

        button.addEventListener('click', () => {
            // Remove active from all
            container.querySelectorAll('.grind-option').forEach(btn => {
                btn.classList.remove('active');
            });
            // Add active to clicked
            button.classList.add('active');
            selectedGrind = grind;
        });

        container.appendChild(button);
    });
}

window.decreaseModalQuantity = function() {
    const input = document.getElementById('modalQuantity');
    const currentValue = parseInt(input.value);
    if (currentValue > 1) {
        input.value = currentValue - 1;
    }
};

window.increaseModalQuantity = function() {
    const input = document.getElementById('modalQuantity');
    const currentValue = parseInt(input.value);
    input.value = currentValue + 1;
};

window.addToCartFromModal = function() {
    if (!currentProduct) return;

    const quantity = parseInt(document.getElementById('modalQuantity').value);

    // Create cart item with selected options
    const cartItem = {
        productId: currentProduct.id,
        name: currentProduct.name,
        imageUrl: currentProduct.imageUrl,
        price: currentProduct.price,
        originalPrice: currentProduct.originalPrice,
        quantity: quantity,
        packaging: selectedSize,
        grind: selectedGrind
    };

    // Add to cart
    cart.addItem(cartItem.productId, quantity);

    // Update cart item with full data
    const existingItem = cart.items.find(item => item.productId === cartItem.productId);
    if (existingItem) {
        const currentQuantity = existingItem.quantity; // Preserve the updated quantity
        Object.assign(existingItem, cartItem);
        existingItem.quantity = currentQuantity; // Restore the quantity after merge
        cart.saveCart();
    }

    // Close modal
    closeProductModal();

    // Open cart sidebar
    setTimeout(() => {
        openCartSidebar();
    }, 300);
};

window.buyNowFromModal = function() {
    addToCartFromModal();
    // After adding to cart and opening sidebar, redirect to checkout
    setTimeout(() => {
        window.location.href = '/checkout?buyNow=true';
    }, 500);
};

// ========================================
// Product Card Integration
// ========================================
window.openProductModalFromCard = function(button) {
    // Find the product card container
    const productCard = button.closest('.group');
    if (!productCard) return;

    // Get product data from JSON script tag
    const dataScript = productCard.querySelector('.product-data');
    if (!dataScript) return;

    try {
        const productData = JSON.parse(dataScript.textContent);
        openProductModal(productData);
    } catch (error) {
        console.error('Failed to parse product data:', error);
    }
};

// ========================================
// Cart Calculation & Discount Management
// ========================================
let currentDiscount = {
    couponCode: null,
    couponDiscount: 0,
    voucherId: null,
    voucherDiscount: 0,
    voucherType: null // 'percentage' or 'fixed'
};

const SHIPPING_FEE = 150000; // Fixed shipping fee 30,000 VND

// Format price in VND
function formatPriceVND(price) {
    return price.toLocaleString('vi-VN') + 'đ';
}

// Get cart subtotal
function getCartSubtotal() {
    let subtotal = 0;
    cart.items.forEach(item => {
        const priceNum = parseInt(item.price) || 32000;
        subtotal += priceNum * item.quantity;
    });
    return subtotal;
}

// Update cart total with shipping and discounts
function updateCartTotal() {
    const subtotal = getCartSubtotal();
    const shippingFee = subtotal > 0 ? SHIPPING_FEE : 0;

    // Calculate total discount
    let totalDiscount = currentDiscount.couponDiscount + currentDiscount.voucherDiscount;

    // Calculate final total
    let total = subtotal + shippingFee - totalDiscount;

    // Ensure total is not negative
    if (total < 0) total = 0;

    // Update UI
    document.getElementById('shippingFee').textContent = formatPriceVND(shippingFee);
    document.getElementById('cartTotal').textContent = formatPriceVND(total);

    // Show/hide voucher discount
    const voucherContainer = document.getElementById('voucherDiscountContainer');
    if (currentDiscount.voucherDiscount > 0) {
        voucherContainer.classList.remove('hidden');
        document.getElementById('voucherDiscount').textContent = '-' + formatPriceVND(currentDiscount.voucherDiscount);
    } else {
        voucherContainer.classList.add('hidden');
    }
}

// Apply coupon code
window.applyCoupon = async function() {
    const couponInput = document.getElementById('couponCode');
    const couponCode = couponInput.value.trim().toUpperCase();
    const errorElement = document.getElementById('couponError');
    const successElement = document.getElementById('couponSuccess');

    // Hide previous messages
    errorElement.classList.add('hidden');
    successElement.classList.add('hidden');

    if (!couponCode) {
        errorElement.textContent = 'Please enter discout code';
        errorElement.classList.remove('hidden');
        return;
    }

    try {
        // TODO: Call API to validate coupon
        // For now, use mock validation
        const mockCoupons = {
            'WELCOME10': { discount: 10000, type: 'fixed' },
            'SAVE20': { discount: 20000, type: 'fixed' },
            'DISCOUNT15': { discount: 15, type: 'percentage' }
        };

        if (mockCoupons[couponCode]) {
            const coupon = mockCoupons[couponCode];
            const subtotal = getCartSubtotal();

            if (coupon.type === 'percentage') {
                currentDiscount.couponDiscount = Math.floor(subtotal * coupon.discount / 100);
            } else {
                currentDiscount.couponDiscount = coupon.discount;
            }

            currentDiscount.couponCode = couponCode;

            successElement.textContent = `Coupon applied successfully! Save ${formatPriceVND(currentDiscount.couponDiscount)}`;
            successElement.classList.remove('hidden');

            updateCartTotal();
        } else {
            errorElement.textContent = 'Invalid discount code';
            errorElement.classList.remove('hidden');
        }
    } catch (error) {
        console.error('Error applying coupon:', error);
        errorElement.textContent = 'An error occurred. Please try again';
        errorElement.classList.remove('hidden');
    }
};

// Select voucher from card
window.selectVoucher = function(cardElement) {
    const voucherId = cardElement.getAttribute('data-voucher-id');

    // If clicking the same voucher, deselect it
    if (currentDiscount.voucherId === voucherId) {
        // Deselect
        cardElement.classList.remove('border-primary', 'bg-blue-50');
        cardElement.querySelector('.voucher-radio').checked = false;
        currentDiscount.voucherId = null;
        currentDiscount.voucherDiscount = 0;
        currentDiscount.voucherType = null;
        updateCartTotal();
        return;
    }

    // Remove selection from all cards
    document.querySelectorAll('.voucher-card').forEach(card => {
        card.classList.remove('border-primary', 'bg-blue-50');
        card.querySelector('.voucher-radio').checked = false;
    });

    // Get voucher details
    const mockVouchers = {
        'voucher1': { code: 'GOTIK12', discount: 50000, type: 'fixed', minOrder: 1, description: 'Save 50K for orders from 499K' },
        'voucher2': { code: 'GOTIK123', discount: 50000, type: 'fixed', minOrder: 1, description: '[New Customer] Enter  first order from 299k' },
        'voucher3': { code: 'GOTIK1234', discount: 100000, type: 'fixed', minOrder: 1, description: 'Save 100K for orders from 500K' }
    };

    const voucher = mockVouchers[voucherId];
    if (voucher) {
        const subtotal = getCartSubtotal();

        // Check minimum order requirement
        if (subtotal < voucher.minOrder) {
            cart.showNotification(`Minimum order ${formatPriceVND(voucher.minOrder)} required to use this voucher`, 'error');
            return;
        }

        // Calculate discount
        if (voucher.type === 'percentage') {
            currentDiscount.voucherDiscount = Math.floor(subtotal * voucher.discount / 100);
        } else {
            currentDiscount.voucherDiscount = voucher.discount;
        }

        // Update current discount
        currentDiscount.voucherId = voucherId;
        currentDiscount.voucherType = voucher.type;

        // Mark card as selected
        cardElement.classList.add('border-primary', 'bg-blue-50');
        cardElement.querySelector('.voucher-radio').checked = true;

        // Show success notification
        cart.showNotification(`Voucher applied successfully! Save ${formatPriceVND(currentDiscount.voucherDiscount)}`, 'success');

        // Update total
        updateCartTotal();
    }
};

// Scroll vouchers horizontally
window.scrollVouchers = function(direction) {
    const container = document.getElementById('voucherContainer');
    const scrollAmount = 200;

    if (direction === 'left') {
        container.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
        container.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
};

// Show all vouchers (could open a modal or expand view)
window.showAllVouchers = function() {
    // TODO: Implement modal or expanded view for all vouchers
    cart.showNotification('Showing all vouchers', 'info');
};

// Load available vouchers
function loadVouchers() {
    const voucherContainer = document.getElementById('voucherContainer');
    if (!voucherContainer) return;

    // TODO: Load vouchers from API
    // For now, use mock data matching the image
    const mockVouchers = [
        {
            id: 'voucher1',
            code: 'GTIK12',
            discount: 50000,
            type: 'fixed',
            minOrder: 1,
            description: 'Save 50K orders from 499K',
            expiry: 'EXP: 31/12/2025',
            progress: { current: 0, target: 499000, show: true }
        },
        {
            id: 'voucher2',
            code: 'GTIK123',
            discount: 50000,
            type: 'fixed',
            minOrder: 1,
            description: 'Save 50K order from 299k',
            expiry: 'EXP: 31/12/2025',
            progress: null
        },
        {
            id: 'voucher3',
            code: 'GTIK124',
            discount: 100000,
            type: 'fixed',
            minOrder: 1,
            description: 'Save 100K orders from 500K',
            expiry: 'EXP: 31/12/2025',
            progress: null
        }
    ];

    // Clear container
    voucherContainer.innerHTML = '';

    const template = document.getElementById('voucherCardTemplate');

    // Render voucher cards
    mockVouchers.forEach(voucher => {
        const clone = template.content.cloneNode(true);
        const card = clone.querySelector('.voucher-card');

        // Set data
        card.setAttribute('data-voucher-id', voucher.id);
        card.querySelector('.voucher-code').textContent = voucher.code;

        // Set discount text
        let discountText = '';
        if (voucher.type === 'percentage') {
            discountText = `Save ${voucher.discount}%`;
        } else {
            discountText = `Save ${formatPriceVND(voucher.discount)}`;
        }
        card.querySelector('.voucher-discount').textContent = discountText;

        // Set description
        card.querySelector('.voucher-description').textContent = voucher.description;

        // Set expiry
        card.querySelector('.voucher-expiry').textContent = voucher.expiry;

        // Handle progress bar
        const progressContainer = card.querySelector('.voucher-progress');
        if (voucher.progress && voucher.progress.show) {
            const subtotal = getCartSubtotal();
            const progressPercent = Math.min((subtotal / voucher.progress.target) * 100, 100);

            progressContainer.classList.remove('hidden');
            progressContainer.querySelector('.voucher-progress-bar').style.width = `${progressPercent}%`;

            const remaining = Math.max(voucher.progress.target - subtotal, 0);
            if (remaining > 0) {
                progressContainer.querySelector('.voucher-progress-text').textContent = '';
                progressContainer.classList.add('hidden');
            } else {
                progressContainer.querySelector('.voucher-progress-text').textContent = '✓ Eligible to use';
                progressContainer.querySelector('.voucher-progress-text').classList.remove('text-orange-600');
                progressContainer.querySelector('.voucher-progress-text').classList.add('text-green-600');
            }
        }

        // Check if this voucher is currently selected
        if (currentDiscount.voucherId === voucher.id) {
            card.classList.add('border-primary', 'bg-blue-50');
            card.querySelector('.voucher-radio').checked = true;
        }

        voucherContainer.appendChild(clone);
    });

    // Update scroll buttons visibility
    updateScrollButtons();
}

// Update scroll button visibility
function updateScrollButtons() {
    const container = document.getElementById('voucherContainer');
    const leftBtn = document.getElementById('scrollLeftBtn');
    const rightBtn = document.getElementById('scrollRightBtn');

    if (!container || !leftBtn || !rightBtn) return;

    // Check if scrollable
    const isScrollable = container.scrollWidth > container.clientWidth;

    if (isScrollable) {
        // Show buttons if at scroll positions
        if (container.scrollLeft > 0) {
            leftBtn.classList.remove('hidden');
            leftBtn.classList.add('flex');
        } else {
            leftBtn.classList.add('hidden');
            leftBtn.classList.remove('flex');
        }

        if (container.scrollLeft < container.scrollWidth - container.clientWidth - 10) {
            rightBtn.classList.remove('hidden');
            rightBtn.classList.add('flex');
        } else {
            rightBtn.classList.add('hidden');
            rightBtn.classList.remove('flex');
        }
    } else {
        leftBtn.classList.add('hidden');
        rightBtn.classList.add('hidden');
    }
}

// Listen to scroll events to update buttons
document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('voucherContainer');
    if (container) {
        container.addEventListener('scroll', updateScrollButtons);
    }
});
