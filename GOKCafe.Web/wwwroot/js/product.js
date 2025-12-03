// Product-specific JavaScript

// ========================================
// Product Image Gallery
// ========================================
class ProductGallery {
    constructor() {
        this.mainImage = document.querySelector('.main-product-image img');
        this.thumbnails = document.querySelectorAll('.thumbnail-image');
        this.init();
    }

    init() {
        if (!this.mainImage || this.thumbnails.length === 0) return;

        this.thumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', (e) => {
                this.changeImage(e.target.src);
                this.setActiveThumbnail(e.target);
            });
        });
    }

    changeImage(src) {
        if (this.mainImage) {
            this.mainImage.style.opacity = '0';
            setTimeout(() => {
                this.mainImage.src = src;
                this.mainImage.style.opacity = '1';
            }, 200);
        }
    }

    setActiveThumbnail(activeThumbnail) {
        this.thumbnails.forEach(thumb => {
            thumb.style.borderColor = 'transparent';
        });
        activeThumbnail.style.borderColor = '#6f42c1';
    }
}

// Initialize gallery on product detail pages
if (document.querySelector('.product-detail-page')) {
    new ProductGallery();
}

// ========================================
// Product Quantity Selector
// ========================================
class QuantitySelector {
    constructor(element) {
        this.container = element;
        this.input = element.querySelector('.quantity-input');
        this.decreaseBtn = element.querySelector('.quantity-decrease');
        this.increaseBtn = element.querySelector('.quantity-increase');
        this.min = parseInt(this.input.min) || 1;
        this.max = parseInt(this.input.max) || 999;

        this.init();
    }

    init() {
        this.decreaseBtn.addEventListener('click', () => this.decrease());
        this.increaseBtn.addEventListener('click', () => this.increase());
        this.input.addEventListener('change', () => this.validate());
    }

    getValue() {
        return parseInt(this.input.value) || this.min;
    }

    setValue(value) {
        value = Math.max(this.min, Math.min(this.max, value));
        this.input.value = value;
        this.updateButtons();
    }

    decrease() {
        this.setValue(this.getValue() - 1);
    }

    increase() {
        this.setValue(this.getValue() + 1);
    }

    validate() {
        this.setValue(this.getValue());
    }

    updateButtons() {
        const value = this.getValue();
        this.decreaseBtn.disabled = value <= this.min;
        this.increaseBtn.disabled = value >= this.max;
    }
}

// Initialize quantity selectors
document.querySelectorAll('.quantity-selector').forEach(selector => {
    new QuantitySelector(selector);
});

// ========================================
// Product Filter & Sort
// ========================================
class ProductFilter {
    constructor() {
        this.sortSelect = document.querySelector('#product-sort');
        this.priceRange = document.querySelector('#price-range');
        this.categoryCheckboxes = document.querySelectorAll('.category-filter');

        this.init();
    }

    init() {
        if (this.sortSelect) {
            this.sortSelect.addEventListener('change', () => this.applyFilters());
        }

        if (this.priceRange) {
            this.priceRange.addEventListener('change', () => this.applyFilters());
        }

        this.categoryCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', () => this.applyFilters());
        });
    }

    applyFilters() {
        const params = new URLSearchParams(window.location.search);

        // Sort
        if (this.sortSelect) {
            const sortValue = this.sortSelect.value;
            if (sortValue) {
                params.set('sort', sortValue);
            } else {
                params.delete('sort');
            }
        }

        // Price range
        if (this.priceRange) {
            const priceValue = this.priceRange.value;
            if (priceValue) {
                params.set('maxPrice', priceValue);
            } else {
                params.delete('maxPrice');
            }
        }

        // Categories
        const selectedCategories = Array.from(this.categoryCheckboxes)
            .filter(cb => cb.checked)
            .map(cb => cb.value);

        if (selectedCategories.length > 0) {
            params.set('categories', selectedCategories.join(','));
        } else {
            params.delete('categories');
        }

        // Redirect with new filters
        window.location.search = params.toString();
    }
}

// Initialize filters on product list page
if (document.querySelector('.product-list-page')) {
    // Filter will be initialized if elements exist
    const filter = new ProductFilter();
}

// ========================================
// Product Comparison
// ========================================
class ProductComparison {
    constructor() {
        this.compareList = this.loadCompareList();
        this.maxItems = 4;
        this.updateUI();
    }

    loadCompareList() {
        const list = localStorage.getItem('gok_compare');
        return list ? JSON.parse(list) : [];
    }

    saveCompareList() {
        localStorage.setItem('gok_compare', JSON.stringify(this.compareList));
        this.updateUI();
    }

    addProduct(productId) {
        if (this.compareList.includes(productId)) {
            this.showMessage('Product already in comparison list', 'warning');
            return;
        }

        if (this.compareList.length >= this.maxItems) {
            this.showMessage(`You can only compare up to ${this.maxItems} products`, 'warning');
            return;
        }

        this.compareList.push(productId);
        this.saveCompareList();
        this.showMessage('Product added to comparison', 'success');
    }

    removeProduct(productId) {
        this.compareList = this.compareList.filter(id => id !== productId);
        this.saveCompareList();
        this.showMessage('Product removed from comparison', 'info');
    }

    clearList() {
        this.compareList = [];
        this.saveCompareList();
        this.showMessage('Comparison list cleared', 'info');
    }

    updateUI() {
        const compareButtons = document.querySelectorAll('.btn-compare');
        compareButtons.forEach(btn => {
            const productId = btn.dataset.productId;
            if (this.compareList.includes(productId)) {
                btn.classList.add('active');
                btn.innerHTML = '<i class="fas fa-check"></i> In Comparison';
            } else {
                btn.classList.remove('active');
                btn.innerHTML = '<i class="fas fa-balance-scale"></i> Compare';
            }
        });

        // Update compare count badge
        const compareCount = document.querySelector('.compare-count');
        if (compareCount) {
            compareCount.textContent = this.compareList.length;
            compareCount.style.display = this.compareList.length > 0 ? 'inline-block' : 'none';
        }
    }

    showMessage(message, type) {
        // Reuse notification from main.js
        if (typeof cart !== 'undefined' && cart.showNotification) {
            cart.showNotification(message, type);
        } else {
            console.log(message);
        }
    }
}

// Initialize product comparison
const productComparison = new ProductComparison();

window.toggleCompare = function(productId) {
    if (productComparison.compareList.includes(productId)) {
        productComparison.removeProduct(productId);
    } else {
        productComparison.addProduct(productId);
    }
};

// ========================================
// Product Reviews (Placeholder)
// ========================================
class ProductReviews {
    constructor() {
        this.reviewForm = document.querySelector('.review-form');
        this.init();
    }

    init() {
        if (!this.reviewForm) return;

        this.reviewForm.addEventListener('submit', (e) => {
            e.preventDefault();
            this.submitReview();
        });

        // Star rating
        const stars = document.querySelectorAll('.star-rating input');
        stars.forEach(star => {
            star.addEventListener('change', () => {
                this.updateStarDisplay();
            });
        });
    }

    submitReview() {
        // TODO: Implement review submission
        console.log('Review submitted');
    }

    updateStarDisplay() {
        // TODO: Update star display
    }
}

// Initialize reviews
new ProductReviews();

// ========================================
// Product Wishlist
// ========================================
window.toggleWishlist = function(productId) {
    const wishlist = JSON.parse(localStorage.getItem('gok_wishlist') || '[]');

    if (wishlist.includes(productId)) {
        const index = wishlist.indexOf(productId);
        wishlist.splice(index, 1);
        if (typeof cart !== 'undefined' && cart.showNotification) {
            cart.showNotification('Removed from wishlist', 'info');
        }
    } else {
        wishlist.push(productId);
        if (typeof cart !== 'undefined' && cart.showNotification) {
            cart.showNotification('Added to wishlist', 'success');
        }
    }

    localStorage.setItem('gok_wishlist', JSON.stringify(wishlist));
    updateWishlistUI();
};

function updateWishlistUI() {
    const wishlist = JSON.parse(localStorage.getItem('gok_wishlist') || '[]');
    const wishlistButtons = document.querySelectorAll('.btn-wishlist');

    wishlistButtons.forEach(btn => {
        const productId = btn.dataset.productId;
        if (wishlist.includes(productId)) {
            btn.innerHTML = '<i class="fas fa-heart"></i>';
            btn.classList.add('active');
        } else {
            btn.innerHTML = '<i class="far fa-heart"></i>';
            btn.classList.remove('active');
        }
    });

    // Update wishlist count
    const wishlistCount = document.querySelector('.wishlist-count');
    if (wishlistCount) {
        wishlistCount.textContent = wishlist.length;
    }
}

// Initialize wishlist UI
updateWishlistUI();

console.log('Product scripts loaded successfully');
