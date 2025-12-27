// Main JavaScript for GOK Cafe

// ========================================
// API Configuration
// ========================================
const API_BASE_URL = 'https://localhost:7045/api/v1';

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

    addItem(productIdOrItem, quantity = 1) {
        // Support both old format (productId, quantity) and new format (item object)
        let productId, itemData;

        if (typeof productIdOrItem === 'object') {
            // New format: full item object passed
            itemData = productIdOrItem;
            productId = itemData.productId;
            quantity = itemData.quantity || 1;
        } else {
            // Old format: just productId and quantity
            productId = productIdOrItem;
            itemData = {
                productId: productId,
                quantity: quantity,
                addedAt: new Date().toISOString()
            };
        }

        const existingItem = this.items.find(item => item.productId === productId);

        if (existingItem) {
            existingItem.quantity += quantity;
            // Merge any additional properties from itemData
            Object.assign(existingItem, itemData, {
                quantity: existingItem.quantity // Preserve the updated quantity
            });
        } else {
            this.items.push(itemData);
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
            top: 20px;
            right: 20px;
            transform: translateX(400px);
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
            max-width: 400px;
            transition: transform 0.3s ease;
        `;
        notification.innerHTML = `
            <i class="fas fa-check-circle" style="font-size: 20px;"></i>
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

    if (!sidebar || !overlay) {
        console.error('Cart sidebar or overlay not found in DOM');
        return;
    }

    if (sidebar.classList.contains('open')) {
        closeCartSidebar();
    } else {
        openCartSidebar();
    }
};

window.openCartSidebar = function() {
    const sidebar = document.getElementById('cartSidebar');
    const overlay = document.getElementById('cartOverlay');

    if (!sidebar || !overlay) {
        console.error('Cart sidebar or overlay not found in DOM');
        return;
    }

    // Show overlay
    overlay.classList.remove('hidden');
    overlay.classList.add('show');

    // Open sidebar with animation
    setTimeout(() => {
        sidebar.classList.add('open');
    }, 10);

    // Prevent body scroll when sidebar is open
    document.body.style.overflow = 'hidden';

    // Clear voucher selection when opening cart (don't auto-select)
    currentDiscount.voucherId = null;
    currentDiscount.voucherDiscount = 0;
    currentDiscount.voucherType = null;
    saveDiscount();

    // Clear voucher input field
    const voucherInput = document.getElementById('voucherCodeInput');
    if (voucherInput) {
        voucherInput.value = '';
    }

    // Render cart items
    renderCartSidebar();

    // Load available vouchers
    if (typeof loadVouchers === 'function') {
        loadVouchers();
    }

    // Load and render recommended products
    renderRecommendedProducts();
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

        // Calculate subtotal - use discount price if available
        const effectivePrice = (item.discountPrice && item.discountPrice > 0 && item.discountPrice < item.price)
            ? item.discountPrice
            : item.price;
        const priceNum = Number.parseFloat(effectivePrice) || 0;
        subtotal += priceNum * item.quantity;
    });

    // Update subtotal
    subtotalElement.textContent = formatPriceVND(subtotal);

    // Update total with shipping and discount
    updateCartTotal();

    // Update voucher eligibility based on new subtotal
    updateVoucherEligibility();
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

    // Set price - Display with discount if available (similar to ProductsGrid.cshtml)
    const priceElement = clone.querySelector('.cart-item-price');

    // Check if item has discount price
    if (item.discountPrice && item.discountPrice > 0 && item.discountPrice < item.price) {
        // Calculate discount percentage
        const discountPercent = Math.round(((item.price - item.discountPrice) / item.price) * 100);

        // Calculate total prices for this item
        const originalItemPrice = (item.price * item.quantity).toFixed(2);
        const discountItemPrice = (item.discountPrice * item.quantity).toFixed(2);

        // Build HTML with original price (strikethrough) and discount price
        priceElement.innerHTML = `
            <div class="flex flex-col items-end gap-1">
            <span class="font-semibold text-sm text-gray-900">${discountItemPrice}</span>
                <div class="flex items-center gap-2">
                    <span class="text-gray-500 line-through text-xs">${originalItemPrice}</span>
                </div>
            </div>
        `;
    } else {
        // No discount - display regular price with 2 decimal places
        const priceNum = Number.parseFloat(item.price) || 0;
        const itemPrice = (priceNum * item.quantity).toFixed(2);
        priceElement.textContent = itemPrice;
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

    // Handle manual input changes
    quantityInput.addEventListener('change', (e) => {
        const newQuantity = parseInt(e.target.value);
        if (newQuantity && newQuantity > 0) {
            cart.updateQuantity(item.productId, newQuantity);
            renderCartSidebar();
        } else {
            // Reset to current quantity if invalid
            e.target.value = item.quantity;
        }
    });

    // Prevent non-numeric input
    quantityInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            e.target.blur(); // Trigger change event
        }
    });

    removeBtn.addEventListener('click', () => {
        openConfirmDeleteModal(item.productId);
    });

    return clone;
}

function formatPrice(price) {
    return Number.parseFloat(price).toFixed(2);
}

// ========================================
// Cart Item Selection (Checkbox) Functions
// ========================================
window.toggleSelectAll = function() {
    const selectAllCheckbox = document.getElementById('selectAllCheckbox');
    const itemCheckboxes = document.querySelectorAll('.cart-item-checkbox');

    itemCheckboxes.forEach(checkbox => {
        checkbox.checked = selectAllCheckbox.checked;
    });

    updateCartSelection();
};

window.updateCartSelection = function() {
    const selectAllCheckbox = document.getElementById('selectAllCheckbox');
    const deleteBtn = document.getElementById('deleteSelectedBtn');
    const itemCheckboxes = document.querySelectorAll('.cart-item-checkbox');
    const checkedCheckboxes = document.querySelectorAll('.cart-item-checkbox:checked');

    // Update "Select All" checkbox state - only checked when ALL items are selected
    if (itemCheckboxes.length === 0) {
        selectAllCheckbox.checked = false;
    } else if (checkedCheckboxes.length === itemCheckboxes.length) {
        selectAllCheckbox.checked = true;
    } else {
        selectAllCheckbox.checked = false;
    }

    // Show/hide delete button
    if (checkedCheckboxes.length > 0) {
        deleteBtn.classList.remove('hidden');
    } else {
        deleteBtn.classList.add('hidden');
    }

    // Update total to only include selected items
    updateCartTotal();
};

window.deleteSelectedItems = function() {
    const checkedCheckboxes = document.querySelectorAll('.cart-item-checkbox:checked');

    if (checkedCheckboxes.length === 0) {
        return;
    }

    // Get product IDs of selected items
    const selectedProductIds = [];
    checkedCheckboxes.forEach(checkbox => {
        const cartItem = checkbox.closest('.cart-item');
        const productId = cartItem.getAttribute('data-item-id');
        selectedProductIds.push(productId);
    });

    // Confirm deletion
    if (confirm(`Are you sure you want to delete ${selectedProductIds.length} item(s)?`)) {
        // Remove selected items from cart
        selectedProductIds.forEach(productId => {
            cart.removeItem(productId);
        });

        // Re-render cart
        renderCartSidebar();
        cart.showNotification(`${selectedProductIds.length} item(s) removed from cart`, 'info');
    }
};

function getSelectedCartTotal() {
    const checkedCheckboxes = document.querySelectorAll('.cart-item-checkbox:checked');

    // If no items are selected, calculate total for all items (default behavior)
    if (checkedCheckboxes.length === 0) {
        return getCartSubtotal();
    }

    // Calculate total only for selected items
    let selectedTotal = 0;
    checkedCheckboxes.forEach(checkbox => {
        const cartItem = checkbox.closest('.cart-item');
        const productId = cartItem.getAttribute('data-item-id');
        const item = cart.items.find(i => i.productId === productId);

        if (item) {
            // Use discount price if available
            const effectivePrice = (item.discountPrice && item.discountPrice > 0 && item.discountPrice < item.price)
                ? item.discountPrice
                : item.price;
            const priceNum = Number.parseFloat(effectivePrice) || 0;
            selectedTotal += priceNum * item.quantity;
        }
    });

    return selectedTotal;
}

// ========================================
// Recommended Products Management
// ========================================

/**
 * Fetch recommended products from API
 * @param {number} limit - Number of products to fetch (default: 2)
 * @returns {Promise<Array>} Array of product objects
 */
async function fetchRecommendedProducts(limit = 2) {
    try {
        const response = await fetch(`${API_BASE_URL}/Cart/recommended-products?limit=${limit}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (!result.success || !result.data) {
            console.warn('Invalid response format:', result);
            return [];
        }

        return result.data;

    } catch (error) {
        console.error('Error fetching recommended products:', error);
        return [];
    }
}

/**
 * Render recommended products in the cart sidebar
 * Fetches products from API and displays them in 2-column grid
 */
async function renderRecommendedProducts() {
    const container = document.getElementById('recommendedProductsContainer');
    const section = document.getElementById('recommendedProductsSection');

    if (!container || !section) {
        console.error('Recommended products container not found in DOM');
        return;
    }

    container.innerHTML = '';

    const products = await fetchRecommendedProducts(2);

    if (!products || products.length === 0) {
        section.classList.add('hidden');
        return;
    }

    section.classList.remove('hidden');

    products.forEach(product => {
        const productElement = createRecommendedProductElement(product);
        container.appendChild(productElement);
    });
}

/**
 * Create a recommended product card element from template
 * @param {Object} product - Product data from API
 * @returns {DocumentFragment} Cloned template with populated data
 */
function createRecommendedProductElement(product) {
    const template = document.getElementById('recommendedProductTemplate');
    if (!template) {
        console.error('Recommended product template not found');
        return document.createDocumentFragment();
    }

    const clone = template.content.cloneNode(true);
    const card = clone.querySelector('.recommended-product-item');

    card.setAttribute('data-product-id', product.id);

    const img = clone.querySelector('.recommended-product-image');
    img.src = product.imageUrl || '/images/placeholder-product.jpg';
    img.alt = product.name || 'Product';

    img.onerror = function() {
        this.onerror = null;
        this.src = '/images/placeholder-product.jpg';
    };

    const name = clone.querySelector('.recommended-product-name');
    name.textContent = product.name || 'Product';

    // Handle pricing based on discount availability
    const hasDiscount = product.discountPrice &&
                        product.discountPrice > 0 &&
                        product.discountPrice < product.price;

    const basePriceSpan = clone.querySelector('.recommended-base-price');
    const salePriceSpan = clone.querySelector('.recommended-sale-price');

    if (hasDiscount) {
        // Show both prices: base price (crossed out) and sale price
        basePriceSpan.textContent = formatPriceVND(product.price);
        salePriceSpan.textContent = formatPriceVND(product.discountPrice);
    } else {
        // Hide base price, show only regular price
        basePriceSpan.style.display = 'none';
        salePriceSpan.textContent = formatPriceVND(product.price);
    }

    // Handle out of stock
    const addBtn = clone.querySelector('.recommended-add-btn');
    if (product.stockQuantity <= 0) {
        card.classList.add('opacity-50', 'pointer-events-none');
        addBtn.textContent = 'OUT OF STOCK';
        addBtn.classList.add('text-gray-400');
    } else {
        // Click on card (but not button) to navigate to product detail page
        card.addEventListener('click', function(e) {
            // Don't navigate if clicking the button
            if (e.target.closest('.recommended-add-btn')) {
                return;
            }

            // Close cart sidebar
            closeCartSidebar();

            // Navigate to product detail page
            window.location.href = `/product-details?id=${product.id}`;
        });

        // Click on "Add to Cart" button to add product directly
        addBtn.addEventListener('click', async function(e) {
            e.stopPropagation();

            try {
                // Create cart item object
                const cartItem = {
                    productId: product.id,
                    name: product.name,
                    imageUrl: product.imageUrl,
                    price: product.price,
                    discountPrice: product.discountPrice,
                    quantity: 1,
                    packaging: null,
                    grind: null,
                    addedAt: new Date().toISOString()
                };

                // Add to localStorage cart first
                cart.addItem(cartItem);

                // Also sync with API if available
                if (window.apiService) {
                    try {
                        await window.apiService.addToCart({
                            productId: product.id,
                            quantity: 1,
                            flavourProfileIds: [],
                            equipmentIds: []
                        });
                    } catch (apiError) {
                        console.warn('Failed to sync with API, but item added to local cart:', apiError);
                    }
                }

                // Re-render cart sidebar to show new item
                renderCartSidebar();

            } catch (error) {
                console.error('Error adding to cart:', error);
                cart.showNotification('Failed to add to cart. Please try again.', 'error');
            }
        });
    }

    return clone;
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
            price: productData.price,
            discountPrice: productData.discountPrice,
            imageUrl: productData.imageUrl,
            description: productData.description,
            shortDescription: productData.shortDescription,
            categoryName: productData.categoryName,
            tastingNote: productData.tastingNote,
            region: productData.region,
            process: productData.process,
            stockQuantity: productData.stockQuantity,
            isActive: productData.isActive,
            availableSizes: productData.availableSizes || [],
            availableGrinds: productData.availableGrinds || []
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
        if (currentProduct.discountPrice && currentProduct.discountPrice > 0 && currentProduct.discountPrice < currentProduct.price) {
            priceElement.innerHTML = `
                <span class="text-gray-400 line-through text-xl mr-2">${Number.parseFloat(currentProduct.price).toFixed(2)}</span>
                <span class="text-primary">${Number.parseFloat(currentProduct.discountPrice).toFixed(2)}</span>
            `;
        } else {
            priceElement.textContent = Number.parseFloat(currentProduct.price).toFixed(2);
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

        // Set images with default fallback
        const modalImage = document.getElementById('modalMainImage');
        if (currentProduct.imageUrl && currentProduct.imageUrl.trim() !== '') {
            modalImage.src = currentProduct.imageUrl;
            modalImage.alt = currentProduct.name || '';
            modalImage.style.display = 'block';
        } else {
            // Set default placeholder for missing images
            modalImage.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="340" height="500" viewBox="0 0 340 500"%3E%3Crect fill="%23E8DCC4" width="340" height="500"/%3E%3Ctext fill="%239CA3AF" font-family="Arial" font-size="80" x="50%25" y="50%25" text-anchor="middle" dominant-baseline="middle"%3E%E2%98%95%3C/text%3E%3C/svg%3E';
            modalImage.alt = 'No image available';
            modalImage.style.display = 'block';
        }

        // Render size options from backend data (or use defaults if not available)
        const sizeOptions = currentProduct.availableSizes && currentProduct.availableSizes.length > 0
            ? currentProduct.availableSizes
            : ['250g', '500g', '1kg'];
        renderSizeOptions(sizeOptions);

        // Render grind options from backend data (or use defaults if not available)
        const grindOptions = currentProduct.availableGrinds && currentProduct.availableGrinds.length > 0
            ? currentProduct.availableGrinds
            : ['Whole Bean', 'French Press', 'Filter', 'Espresso'];
        renderGrindOptions(grindOptions);

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
        discountPrice: currentProduct.discountPrice,
        quantity: quantity,
        packaging: selectedSize,
        grind: selectedGrind,
        addedAt: new Date().toISOString()
    };

    // Add to cart with full item data
    cart.addItem(cartItem);

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

// Load discount from localStorage or initialize
function loadDiscount() {
    const saved = localStorage.getItem('gok_cart_discount');
    if (saved) {
        try {
            return JSON.parse(saved);
        } catch (e) {
            console.error('Error parsing saved discount:', e);
        }
    }
    return {
        couponCode: null,
        couponDiscount: 0,
        voucherId: null,
        voucherDiscount: 0,
        voucherType: null // 'percentage' or 'fixed'
    };
}

// Save discount to localStorage
function saveDiscount() {
    localStorage.setItem('gok_cart_discount', JSON.stringify(currentDiscount));
}

// Clear discount from localStorage
function clearDiscount() {
    localStorage.removeItem('gok_cart_discount');
    currentDiscount = {
        couponCode: null,
        couponDiscount: 0,
        voucherId: null,
        voucherDiscount: 0,
        voucherType: null
    };
}

let currentDiscount = loadDiscount();

const SHIPPING_FEE = 150000; // Fixed shipping fee 30,000 VND

// Format price in VND with 2 decimal places
function formatPriceVND(price) {
    return Number.parseFloat(price).toFixed(2);
}

// Get cart subtotal
function getCartSubtotal() {
    let subtotal = 0;
    cart.items.forEach(item => {
        // Use discount price if available
        const effectivePrice = (item.discountPrice && item.discountPrice > 0 && item.discountPrice < item.price)
            ? item.discountPrice
            : item.price;
        const priceNum = Number.parseFloat(effectivePrice) || 0;
        subtotal += priceNum * item.quantity;
    });
    return subtotal;
}

// Update cart total with discounts (no shipping fee)
function updateCartTotal() {
    // Use selected items total if checkboxes are present and items are selected
    const checkedCheckboxes = document.querySelectorAll('.cart-item-checkbox:checked');
    const subtotal = checkedCheckboxes.length > 0 ? getSelectedCartTotal() : getCartSubtotal();

    // Calculate total discount
    let totalDiscount = currentDiscount.couponDiscount + currentDiscount.voucherDiscount;

    // Calculate final total (no shipping fee)
    let total = subtotal - totalDiscount;

    // Ensure total is not negative
    if (total < 0) total = 0;

    // Update UI
    document.getElementById('cartSubtotal').textContent = formatPriceVND(subtotal);
    document.getElementById('cartTotal').textContent = formatPriceVND(total);

    // Show/hide coupon discount
    const couponContainer = document.getElementById('couponDiscountContainer');
    if (currentDiscount.couponDiscount > 0) {
        couponContainer.classList.remove('hidden');
        document.getElementById('couponDiscount').textContent = '-' + formatPriceVND(currentDiscount.couponDiscount);
        const appliedCodeElement = document.getElementById('appliedCouponCode');
        if (appliedCodeElement && currentDiscount.couponCode) {
            appliedCodeElement.textContent = `(${currentDiscount.couponCode})`;
        }
    } else {
        couponContainer.classList.add('hidden');
    }

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
        errorElement.textContent = 'Please enter discount code';
        errorElement.classList.remove('hidden');
        return;
    }

    try {
        // Call backend API to apply coupon to cart
        const response = await window.apiService.applyCouponToCart(couponCode);

        if (response.success && response.data) {
            const cartData = response.data;

            // Update discount from API response
            currentDiscount.couponCode = couponCode;
            currentDiscount.couponDiscount = cartData.discountAmount || 0;

            // Save discount to localStorage
            saveDiscount();

            // Show success message
            successElement.textContent = response.message || `Coupon applied successfully! Save ${formatPriceVND(currentDiscount.couponDiscount)}`;
            successElement.classList.remove('hidden');

            // Update cart totals
            updateCartTotal();

            // Optionally refresh cart items if backend modified them
            if (cartData.items && cartData.items.length > 0) {
                // Update cart items from backend response
                cart.items = cartData.items.map(item => ({
                    productId: item.productId,
                    name: item.productName,
                    imageUrl: item.productImageUrl,
                    price: item.unitPrice,
                    quantity: item.quantity,
                    packaging: item.selectedOptions?.packaging,
                    grind: item.selectedOptions?.grind
                }));
                cart.saveCart();
                renderCartSidebar();
            }
        } else {
            // Show error message from API
            errorElement.textContent = response.message || 'Invalid discount code';
            errorElement.classList.remove('hidden');
        }
    } catch (error) {
        console.error('Error applying coupon:', error);
        errorElement.textContent = error.message || 'An error occurred. Please try again';
        errorElement.classList.remove('hidden');
    }
};

// Remove coupon code
window.removeCoupon = async function() {
    const errorElement = document.getElementById('couponError');
    const successElement = document.getElementById('couponSuccess');
    const couponInput = document.getElementById('couponCode');

    // Hide previous messages
    errorElement.classList.add('hidden');
    successElement.classList.add('hidden');

    try {
        // Call backend API to remove coupon from cart
        const response = await window.apiService.removeCouponFromCart();

        if (response.success) {
            // Reset coupon discount
            currentDiscount.couponCode = null;
            currentDiscount.couponDiscount = 0;

            // Save updated discount to localStorage
            saveDiscount();

            // Clear input field
            couponInput.value = '';

            // Show success message
            cart.showNotification('Coupon removed successfully', 'info');

            // Update cart totals
            updateCartTotal();
        } else {
            errorElement.textContent = response.message || 'Failed to remove coupon';
            errorElement.classList.remove('hidden');
        }
    } catch (error) {
        console.error('Error removing coupon:', error);
        errorElement.textContent = error.message || 'An error occurred. Please try again';
        errorElement.classList.remove('hidden');
    }
};

// Toggle voucher selection (for both card click and radio button)
window.toggleVoucherSelection = async function(cardElement) {
    const voucherId = cardElement.getAttribute('data-voucher-id');
    const voucherCode = cardElement.getAttribute('data-voucher-code');
    const radio = cardElement.querySelector('.voucher-radio');

    // If clicking the same voucher, deselect it
    if (currentDiscount.voucherId === voucherId) {
        // Deselect
        cardElement.classList.remove('selected');
        radio.checked = false;

        // Clear input field
        const voucherInput = document.getElementById('voucherCodeInput');
        if (voucherInput) {
            voucherInput.value = '';
        }

        currentDiscount.voucherId = null;
        currentDiscount.voucherDiscount = 0;
        currentDiscount.voucherType = null;
        saveDiscount();
        updateCartTotal();
        return;
    }

    const subtotal = getCartSubtotal();

    try {
        // Validate voucher via API
        const isValid = await validateVoucherWithAPI(voucherCode, subtotal);

        if (!isValid.success) {
            cart.showNotification(isValid.message || 'Voucher không hợp lệ', 'error');
            return;
        }

        // Remove selection from all cards
        document.querySelectorAll('.voucher-card').forEach(card => {
            card.classList.remove('selected');
            card.querySelector('.voucher-radio').checked = false;
        });

        // Use estimated discount from API response
        const estimatedDiscount = isValid.estimatedDiscount || 0;

        // Update current discount
        currentDiscount.voucherId = voucherId;
        currentDiscount.voucherDiscount = estimatedDiscount;
        currentDiscount.voucherType = isValid.coupon?.discountType === 0 ? 'percentage' : 'fixed';

        // Save discount to localStorage
        saveDiscount();

        // Mark card as selected
        cardElement.classList.add('selected');
        radio.checked = true;

        // Update input field with voucher code
        const voucherInput = document.getElementById('voucherCodeInput');
        if (voucherInput) {
            voucherInput.value = voucherCode;
        }

        // Show success notification
        cart.showNotification(`Áp dụng voucher thành công! Giảm ${formatPriceVND(currentDiscount.voucherDiscount)}`, 'success');

        // Update total
        updateCartTotal();

    } catch (error) {
        console.error('Error validating voucher:', error);
        cart.showNotification('Không thể xác thực voucher. Vui lòng thử lại', 'error');
    }
};

// Apply voucher from input field
window.applyVoucherFromInput = async function() {
    const voucherInput = document.getElementById('voucherCodeInput');
    const voucherCode = voucherInput.value.trim().toUpperCase();

    if (!voucherCode) {
        cart.showNotification('Vui lòng nhập mã voucher', 'error');
        return;
    }

    const subtotal = getCartSubtotal();

    try {
        // Validate voucher via API first
        const isValid = await validateVoucherWithAPI(voucherCode, subtotal);

        if (!isValid.success) {
            cart.showNotification(isValid.message || 'Mã voucher không hợp lệ', 'error');
            return;
        }

        // Find corresponding card by code
        const voucherCard = document.querySelector(`.voucher-card[data-voucher-code="${voucherCode}"]`);

        if (voucherCard) {
            // Card exists in the list, select it directly
            // Remove selection from all cards first
            document.querySelectorAll('.voucher-card').forEach(card => {
                card.classList.remove('selected');
                card.querySelector('.voucher-radio').checked = false;
            });

            // Use validated data from API
            const estimatedDiscount = isValid.estimatedDiscount || 0;
            const voucherId = voucherCard.dataset.voucherId;

            // Update current discount
            currentDiscount.voucherId = voucherId;
            currentDiscount.voucherDiscount = estimatedDiscount;
            currentDiscount.voucherType = isValid.coupon?.discountType === 0 ? 'percentage' : 'fixed';

            // Save discount to localStorage
            saveDiscount();

            // Mark card as selected
            voucherCard.classList.add('selected');
            voucherCard.querySelector('.voucher-radio').checked = true;

            // Show success notification
            cart.showNotification(`Áp dụng voucher thành công! Giảm ${formatPriceVND(currentDiscount.voucherDiscount)}`, 'success');

            // Update total
            updateCartTotal();
        } else {
            // Card not in the visible list, but voucher is valid
            // Apply it anyway using API data
            const estimatedDiscount = isValid.estimatedDiscount || 0;
            const couponId = isValid.coupon?.id;

            // Remove selection from all cards
            document.querySelectorAll('.voucher-card').forEach(card => {
                card.classList.remove('selected');
                card.querySelector('.voucher-radio').checked = false;
            });

            // Update current discount
            currentDiscount.voucherId = couponId;
            currentDiscount.voucherDiscount = estimatedDiscount;
            currentDiscount.voucherType = isValid.coupon?.discountType === 0 ? 'percentage' : 'fixed';

            // Save discount to localStorage
            saveDiscount();

            // Show success notification
            cart.showNotification(`Áp dụng voucher thành công! Giảm ${formatPriceVND(currentDiscount.voucherDiscount)}`, 'success');

            // Update total
            updateCartTotal();
        }

    } catch (error) {
        console.error('Error applying voucher:', error);
        cart.showNotification('Không thể xác thực voucher. Vui lòng thử lại', 'error');
    }
};

// Scroll vouchers horizontally with center alignment
window.scrollVouchers = function(direction) {
    const container = document.getElementById('voucherContainer');
    const voucherCards = container.querySelectorAll('.voucher-card');

    if (voucherCards.length === 0) return;

    const containerWidth = container.clientWidth;
    const cardWidth = voucherCards[0].offsetWidth;
    const gap = 12; // 3 * 4px (gap-3 in Tailwind)
    const cardWithGap = cardWidth + gap;

    // Get current scroll position
    const currentScroll = container.scrollLeft;

    // Calculate which card is currently visible
    const currentCardIndex = Math.round(currentScroll / cardWithGap);

    let targetCardIndex;
    if (direction === 'left') {
        targetCardIndex = Math.max(0, currentCardIndex - 1);
    } else {
        targetCardIndex = Math.min(voucherCards.length - 1, currentCardIndex + 1);
    }

    // Calculate scroll position to center the target card
    const targetCard = voucherCards[targetCardIndex];
    const cardCenter = targetCard.offsetLeft + (cardWidth / 2);
    const containerCenter = containerWidth / 2;
    const scrollPosition = cardCenter - containerCenter;

    container.scrollTo({
        left: Math.max(0, scrollPosition),
        behavior: 'smooth'
    });

    // Update button visibility after scrolling
    setTimeout(updateScrollButtonsVisibility, 300);
};

// Show all vouchers (could open a modal or expand view)
window.showAllVouchers = function() {
    // TODO: Implement modal or expanded view for all vouchers
    cart.showNotification('Showing all vouchers', 'info');
};

// ========================================
// Voucher API Validation
// ========================================
async function validateVoucherWithAPI(couponCode, orderAmount) {
    try {
        const response = await fetch(`${API_BASE_URL}/Coupons/validate`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'accept': 'text/plain'
            },
            body: JSON.stringify({
                couponCode: couponCode,
                orderAmount: orderAmount
            })
        });

        if (!response.ok) {
            throw new Error('Failed to validate voucher');
        }

        const result = await response.json();

        if (!result.success || !result.data) {
            return {
                success: false,
                message: result.message || 'Voucher không hợp lệ'
            };
        }

        const validationData = result.data;

        if (!validationData.isValid) {
            return {
                success: false,
                message: validationData.message || 'Voucher không thể sử dụng'
            };
        }

        // Return validation result with estimated discount
        return {
            success: true,
            message: validationData.message,
            coupon: validationData.coupon,
            estimatedDiscount: validationData.estimatedDiscount || 0
        };

    } catch (error) {
        console.error('Error calling validate API:', error);
        throw error;
    }
}

// ========================================
// Voucher Eligibility Update
// ========================================
/**
 * Update voucher eligibility based on current cart subtotal
 * Called when cart items or quantities change
 */
function updateVoucherEligibility() {
    const voucherCards = document.querySelectorAll('.voucher-card');
    if (voucherCards.length === 0) return;

    const currentSubtotal = getCartSubtotal();
    voucherCards.forEach(card => updateSingleVoucherEligibility(card, currentSubtotal));
}

function updateSingleVoucherEligibility(card, currentSubtotal) {
    const minOrderAmount = Number.parseFloat(card.dataset.minOrderAmount) || 0;
    const isActive = card.dataset.isActive !== 'false';
    const isExpired = card.dataset.isExpired === 'true';
    const canBeUsed = card.dataset.canBeUsed !== 'false';

    const meetsConditions = canBeUsed && isActive && !isExpired && currentSubtotal >= minOrderAmount;

    if (meetsConditions) {
        enableVoucherCard(card);
    } else {
        disableVoucherCard(card, { isActive, isExpired, minOrderAmount, currentSubtotal });
    }
}

function enableVoucherCard(card) {
    card.classList.remove('opacity-50');
    card.removeAttribute('title');

    const voucherTicket = card.querySelector('.voucher-ticket');
    const radioBtn = card.querySelector('.voucher-radio');

    if (voucherTicket) voucherTicket.style.pointerEvents = '';
    if (radioBtn) {
        radioBtn.disabled = false;
        radioBtn.style.cursor = '';
    }
}

function disableVoucherCard(card, { isActive, isExpired, minOrderAmount, currentSubtotal }) {
    card.classList.add('opacity-50');

    const voucherTicket = card.querySelector('.voucher-ticket');
    const conditionBtn = card.querySelector('.voucher-condition-btn');
    const radioBtn = card.querySelector('.voucher-radio');

    if (voucherTicket) voucherTicket.style.pointerEvents = 'none';
    if (conditionBtn) {
        conditionBtn.style.pointerEvents = 'auto';
        conditionBtn.style.cursor = 'pointer';
    }
    if (radioBtn) {
        radioBtn.disabled = true;
        radioBtn.style.cursor = 'not-allowed';
    }

    const tooltipText = getVoucherDisabledReason(isActive, isExpired, minOrderAmount, currentSubtotal);
    card.setAttribute('title', tooltipText);
}

function getVoucherDisabledReason(isActive, isExpired, minOrderAmount, currentSubtotal) {
    if (!isActive) return 'Voucher không khả dụng';
    if (isExpired) return 'Voucher đã hết hạn';
    if (currentSubtotal < minOrderAmount) return `Đơn hàng cần tối thiểu ${formatPriceVND(minOrderAmount)}`;
    return 'Voucher không thể sử dụng';
}

// ========================================
// Voucher Loading & Management
// ========================================
async function loadVouchers() {
    const voucherContainer = document.getElementById('voucherContainer');
    if (!voucherContainer) return;

    // Show loading state
    voucherContainer.innerHTML = '<div class="text-center text-gray-400 text-sm py-4 w-full">Loading vouchers...</div>';

    try {
        // Call API to get system coupons
        const response = await fetch(`${API_BASE_URL}/Coupons/system?pageNumber=1&pageSize=10`);

        if (!response.ok) {
            throw new Error('Failed to fetch vouchers');
        }

        const result = await response.json();

        if (!result.success || !result.data || !result.data.items) {
            throw new Error('Invalid response format');
        }

        const vouchers = result.data.items;

        // Clear loading state
        voucherContainer.innerHTML = '';

        if (vouchers.length === 0) {
            voucherContainer.innerHTML = '<div class="text-center text-gray-400 text-sm py-4 w-full">No vouchers available</div>';
            return;
        }

        const template = document.getElementById('voucherCardTemplate');
        if (!template) return;

        const currentSubtotal = getCartSubtotal();

        // Render each voucher
        vouchers.forEach(voucher => {
            const clone = template.content.cloneNode(true);
            const card = clone.querySelector('.voucher-card');

            // Set voucher data
            card.setAttribute('data-voucher-id', voucher.id);
            card.setAttribute('data-voucher-code', voucher.code);

            // Set code
            card.querySelector('.voucher-code').textContent = voucher.code;

            // Set badge with remaining usage count
            const remainingCount = voucher.maxUsageCount - voucher.usageCount;

            // Set description
            card.querySelector('.voucher-description').textContent = voucher.description || 'No description';

            // Format and set expiry date
            const expiryDate = formatExpiryDate(voucher.endDate);
            card.querySelector('.voucher-expiry').textContent = `HSD: ${expiryDate}`;

            // Store voucher data for later use
            card.dataset.discountType = voucher.discountType; // 0 = percentage, 1 = fixed
            card.dataset.discountValue = voucher.discountValue;
            card.dataset.maxDiscountAmount = voucher.maxDiscountAmount || '';
            card.dataset.minOrderAmount = voucher.minOrderAmount || 0;

            // Store additional data for modal
            card.dataset.name = voucher.name || voucher.code;
            card.dataset.typeDisplay = voucher.typeDisplay || 'N/A';
            card.dataset.startDate = voucher.startDate || '';
            card.dataset.endDate = voucher.endDate || '';

            // Store eligibility flags for dynamic updates
            card.dataset.canBeUsed = voucher.canBeUsed;
            card.dataset.isActive = voucher.isActive;
            card.dataset.isExpired = voucher.isExpired;

            // Don't auto-select vouchers on load - user must manually select
            // This section is intentionally commented out to prevent auto-selection

            // Check if voucher can be used
            const canBeUsed = voucher.canBeUsed &&
                             voucher.isActive &&
                             !voucher.isExpired &&
                             currentSubtotal >= voucher.minOrderAmount;

            // Disable if not eligible - but still allow clicking "Condition" button
            if (!canBeUsed) {
                card.classList.add('opacity-50');

                // Disable click on the card itself (not the condition button)
                const voucherTicket = card.querySelector('.voucher-ticket');
                if (voucherTicket) {
                    voucherTicket.style.pointerEvents = 'none';
                }

                // Re-enable pointer events for the Condition button specifically
                const conditionBtn = card.querySelector('.voucher-condition-btn');
                if (conditionBtn) {
                    conditionBtn.style.pointerEvents = 'auto';
                    conditionBtn.style.cursor = 'pointer';
                }

                // Ensure radio button is also disabled
                const radioBtn = card.querySelector('.voucher-radio');
                if (radioBtn) {
                    radioBtn.disabled = true;
                    radioBtn.style.cursor = 'not-allowed';
                }

                let tooltipText = '';
                if (!voucher.isActive) {
                    tooltipText = 'Voucher không khả dụng';
                } else if (voucher.isExpired) {
                    tooltipText = 'Voucher đã hết hạn';
                } else if (currentSubtotal < voucher.minOrderAmount) {
                    tooltipText = `Đơn hàng cần tối thiểu ${formatPriceVND(voucher.minOrderAmount)}`;
                } else {
                    tooltipText = 'Voucher không thể sử dụng';
                }

                card.setAttribute('title', tooltipText);
            }

            voucherContainer.appendChild(clone);
        });

        // Update scroll button visibility
        updateScrollButtonsVisibility();

    } catch (error) {
        console.error('Error loading vouchers:', error);
        voucherContainer.innerHTML = '<div class="text-center text-red-400 text-sm py-4 w-full">Failed to load vouchers</div>';
    }
}

// Format expiry date to DD/MM/YYYY
function formatExpiryDate(dateString) {
    if (!dateString) return 'N/A';

    try {
        const date = new Date(dateString);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();

        return `${day}/${month}/${year}`;
    } catch (error) {
        console.error('Error formatting date:', error);
        return 'N/A';
    }
}

function updateScrollButtonsVisibility() {
    const container = document.getElementById('voucherContainer');
    const leftBtn = document.getElementById('scrollLeftBtn');
    const rightBtn = document.getElementById('scrollRightBtn');

    if (!container || !leftBtn || !rightBtn) return;

    const isScrollable = container.scrollWidth > container.clientWidth;

    if (isScrollable) {
        // Show/hide left button
        if (container.scrollLeft > 10) {
            leftBtn.classList.remove('hidden');
            leftBtn.classList.add('flex');
        } else {
            leftBtn.classList.add('hidden');
            leftBtn.classList.remove('flex');
        }

        // Show/hide right button
        if (container.scrollLeft < container.scrollWidth - container.clientWidth - 10) {
            rightBtn.classList.remove('hidden');
            rightBtn.classList.add('flex');
        } else {
            rightBtn.classList.add('hidden');
            rightBtn.classList.remove('flex');
        }
    } else {
        leftBtn.classList.add('hidden');
        leftBtn.classList.remove('flex');
        rightBtn.classList.add('hidden');
        rightBtn.classList.remove('flex');
    }
}

// Listen to scroll events to update buttons
document.addEventListener('DOMContentLoaded', () => {
    const voucherContainer = document.getElementById('voucherContainer');
    if (voucherContainer) {
        voucherContainer.addEventListener('scroll', updateScrollButtonsVisibility);
    }

    // Add event listeners to voucher condition buttons
    attachVoucherConditionListeners();
});

// ========================================
// Voucher Condition Modal
// ========================================

/**
 * Attach click event listeners to all voucher condition buttons
 */
let voucherConditionListenerAttached = false;

function attachVoucherConditionListeners() {
    // Only attach once to avoid duplicate listeners
    if (voucherConditionListenerAttached) return;

    document.addEventListener('click', (e) => {
        // Check if clicked element or its parent is the condition button
        const conditionBtn = e.target.closest('.voucher-condition-btn');
        if (conditionBtn) {
            e.preventDefault();
            e.stopPropagation();

            const card = conditionBtn.closest('.voucher-card');
            if (card) {
                openVoucherConditionModal(card);
            }
        }
    });

    voucherConditionListenerAttached = true;
}

/**
 * Open voucher condition modal with voucher details
 * @param {HTMLElement} voucherCard - The voucher card element
 */
function openVoucherConditionModal(voucherCard) {
    const modal = document.getElementById('voucherConditionModal');
    if (!modal) {
        console.error('Voucher condition modal not found in DOM');
        return;
    }

    // Close cart sidebar first
    const cartSidebar = document.getElementById('cartSidebar');
    const cartOverlay = document.getElementById('cartOverlay');
    if (cartSidebar && cartSidebar.classList.contains('open')) {
        // Close sidebar with proper transition
        cartSidebar.classList.remove('open');

        // Force hide overlay immediately
        if (cartOverlay) {
            cartOverlay.classList.add('hidden');
            cartOverlay.style.display = 'none';
        }

        // Store flag that we should reopen cart after modal closes
        modal.dataset.shouldReopenCart = 'true';
    } else {
        modal.dataset.shouldReopenCart = 'false';
    }

    // Get voucher data from card attributes
    const voucherCode = voucherCard.getAttribute('data-voucher-code');
    const discountType = parseInt(voucherCard.dataset.discountType);
    const discountValue = parseFloat(voucherCard.dataset.discountValue);
    const maxDiscountAmount = voucherCard.dataset.maxDiscountAmount ? parseFloat(voucherCard.dataset.maxDiscountAmount) : null;
    const minOrderAmount = parseFloat(voucherCard.dataset.minOrderAmount) || 0;

    // Get stored data
    const name = voucherCard.dataset.name || voucherCode;
    const description = voucherCard.querySelector('.voucher-description')?.textContent || 'No description available';
    const typeDisplay = voucherCard.dataset.typeDisplay || 'N/A';
    const startDate = voucherCard.dataset.startDate ? formatModalDate(voucherCard.dataset.startDate) : 'N/A';
    const endDate = voucherCard.dataset.endDate ? formatModalDate(voucherCard.dataset.endDate) : 'N/A';

    // Populate modal content
    document.getElementById('modalVoucherCode').textContent = voucherCode;
    document.getElementById('modalVoucherName').textContent = name;
    document.getElementById('modalVoucherDescription').textContent = description;
    document.getElementById('modalVoucherType').textContent = typeDisplay;

    // Format discount value
    let discountText = '';
    if (discountType === 0) {
        // Percentage discount
        discountText = `${discountValue}% off`;
    } else if (discountType === 1) {
        // Percentage discount (same as 0)
        discountText = `${discountValue}% off`;
    } else {
        // Fixed amount discount
        discountText = formatPriceVND(discountValue);
    }
    document.getElementById('modalDiscountValue').textContent = discountText;

    // Max discount amount
    const maxDiscountContainer = document.getElementById('modalMaxDiscountContainer');
    if (maxDiscountAmount) {
        maxDiscountContainer.classList.remove('hidden');
        document.getElementById('modalMaxDiscountAmount').textContent = formatPriceVND(maxDiscountAmount);
    } else {
        maxDiscountContainer.classList.add('hidden');
    }

    // Min order amount
    document.getElementById('modalMinOrderAmount').textContent = minOrderAmount > 0 ? formatPriceVND(minOrderAmount) : 'No minimum';

    // Dates
    document.getElementById('modalStartDate').textContent = startDate;
    document.getElementById('modalEndDate').textContent = endDate;

    // Show modal
    modal.classList.remove('hidden');
    document.body.style.overflow = 'hidden'; // Prevent background scrolling
}

/**
 * Close voucher condition modal
 * @param {Event} event - Optional event object
 */
window.closeVoucherConditionModal = function(event) {
    // If event is provided and it's a click on the backdrop
    if (event && event.target.id !== 'voucherConditionModal') {
        return;
    }

    const modal = document.getElementById('voucherConditionModal');
    if (modal) {
        // Check if we should reopen cart
        const shouldReopenCart = modal.dataset.shouldReopenCart === 'true';

        // Hide modal
        modal.classList.add('hidden');
        document.body.style.overflow = ''; // Restore scrolling

        // Reopen cart if it was open before
        if (shouldReopenCart) {
            const cartSidebar = document.getElementById('cartSidebar');
            const cartOverlay = document.getElementById('cartOverlay');
            if (cartSidebar) {
                cartSidebar.classList.add('open');
            }
            if (cartOverlay) {
                cartOverlay.classList.remove('hidden');
                cartOverlay.style.display = '';
            }
        }

        // Clear the flag
        modal.dataset.shouldReopenCart = 'false';
    }
};

/**
 * Format date for modal display
 * @param {string} dateString - ISO date string
 * @returns {string} Formatted date
 */
function formatModalDate(dateString) {
    if (!dateString) return 'N/A';

    try {
        const date = new Date(dateString);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();

        return `${day}/${month}/${year}`;
    } catch (error) {
        console.error('Error formatting modal date:', error);
        return dateString;
    }
}
