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
        notification.style.cssText = `
            position: fixed;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%) translateY(100px);
            z-index: 10000;
            background: #10B981;
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
        subtotalElement.textContent = '0 VND';
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
    subtotalElement.textContent = formatPrice(subtotal);

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
    const priceNum = parseInt(item.price) || 32000;
    const itemPrice = priceNum * item.quantity;
    priceElement.textContent = formatPrice(itemPrice);

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
        if (confirm('Remove this item from cart?')) {
            cart.removeItem(item.productId);
            renderCartSidebar();
        }
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

window.openProductModal = function(productData) {
    currentProduct = productData;
    const modal = document.getElementById('productModal');
    const overlay = document.getElementById('productModalOverlay');

    // Populate modal with product data
    document.getElementById('modalProductName').textContent = productData.name || 'Product Name';
    document.getElementById('modalProductPrice').textContent = productData.price || '700';
    document.getElementById('modalMainImage').src = productData.imageUrl || '';
    document.getElementById('modalMainImage').alt = productData.name || '';

    // Render size options
    renderSizeOptions(productData.sizes || ['250g', '500g', '1kg']);

    // Render grind options
    renderGrindOptions(productData.grinds || ['Whole Bean', 'French Press', 'Filter', 'Espresso']);

    // Reset quantity
    document.getElementById('modalQuantity').value = 1;

    // Show modal
    overlay.classList.remove('hidden');
    overlay.classList.add('show');
    modal.classList.remove('hidden');
    modal.classList.add('show');

    // Prevent body scroll
    document.body.style.overflow = 'hidden';
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
        quantity: quantity,
        packaging: selectedSize,
        grind: selectedGrind
    };

    // Add to cart
    cart.addItem(cartItem.productId, quantity);

    // Update cart item with full data
    const existingItem = cart.items.find(item => item.productId === cartItem.productId);
    if (existingItem) {
        Object.assign(existingItem, cartItem);
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
