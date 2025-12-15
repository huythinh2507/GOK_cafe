// ========================================
// Search Modal Management
// ========================================

let currentSearchTerm = '';
let searchDebounceTimeout = null;

// Open search modal
window.openSearchModal = function() {
    console.log('[Search] Opening search modal');
    const modal = document.getElementById('searchModal');
    const overlay = document.getElementById('searchModalOverlay');
    const searchInput = document.getElementById('searchInput');

    if (!modal || !overlay || !searchInput) {
        console.error('[Search] Required elements not found');
        return;
    }

    overlay.classList.remove('hidden');
    overlay.classList.add('show');
    setTimeout(() => {
        modal.classList.remove('hidden');
        modal.classList.add('show');
        searchInput.focus();
    }, 10);
    document.body.style.overflow = 'hidden';
};

// Close search modal
window.closeSearchModal = function() {
    console.log('[Search] Closing search modal');
    const modal = document.getElementById('searchModal');
    const overlay = document.getElementById('searchModalOverlay');

    if (!modal || !overlay) return;

    modal.classList.remove('show');
    overlay.classList.remove('show');
    setTimeout(() => {
        modal.classList.add('hidden');
        overlay.classList.add('hidden');
    }, 300);
    document.body.style.overflow = '';
};

// Clear search
window.clearSearch = function() {
    console.log('[Search] Clearing search');
    const searchInput = document.getElementById('searchInput');
    const resultsGrid = document.getElementById('searchResultsGrid');
    const resultsTitle = document.getElementById('searchResultsTitle');

    if (searchInput) searchInput.value = '';
    if (resultsGrid) resultsGrid.innerHTML = '';
    if (resultsTitle) resultsTitle.classList.add('hidden');

    currentSearchTerm = '';
};

// View all search results - redirect to search page
window.viewAllSearchResults = function() {
    const searchInput = document.getElementById('searchInput');
    const searchTerm = searchInput?.value.trim() || '';

    console.log('[Search] Redirecting to search page with term:', searchTerm);
    if (searchTerm) {
        window.location.href = `/search?content=${encodeURIComponent(searchTerm)}`;
    } else {
        // Empty search - go to search page without query (shows all products)
        window.location.href = `/search`;
    }
};

// Perform modal search (live search in modal)
window.performModalSearch = async function(searchTerm) {
    console.log('[Search] Performing modal search for:', searchTerm);

    const resultsGrid = document.getElementById('searchResultsGrid');
    const resultsTitle = document.getElementById('searchResultsTitle');

    if (!resultsGrid || !resultsTitle) {
        console.error('[Search] Results elements not found');
        return;
    }

    if (!searchTerm || searchTerm.trim().length < 2) {
        resultsGrid.innerHTML = '';
        resultsTitle.classList.add('hidden');
        return;
    }

    currentSearchTerm = searchTerm.trim();

    // Show loading state
    resultsGrid.innerHTML = `
        <div class="col-span-full text-center py-8">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            <p class="mt-2 text-gray-600">Searching...</p>
        </div>
    `;
    resultsTitle.classList.remove('hidden');

    try {
        // Call API to search products
        const response = await fetch(`/api/v1/products?search=${encodeURIComponent(currentSearchTerm)}&pageNumber=1&pageSize=8`);

        if (!response.ok) {
            throw new Error('Search request failed');
        }

        const result = await response.json();
        console.log('[Search] Search results:', result);

        if (!result.success || !result.data || !result.data.items || result.data.items.length === 0) {
            // No results found
            resultsGrid.innerHTML = `
                <div class="col-span-full text-center py-8">
                    <i class="fas fa-search text-4xl text-gray-300 mb-2"></i>
                    <p class="text-gray-600">No products found</p>
                </div>
            `;
            return;
        }

        // Render results
        resultsGrid.innerHTML = '';
        result.data.items.forEach(product => {
            const productCard = createModalProductCard(product);
            resultsGrid.appendChild(productCard);
        });

        console.log(`[Search] Rendered ${result.data.items.length} products`);

    } catch (error) {
        console.error('[Search] Error searching products:', error);
        resultsGrid.innerHTML = `
            <div class="col-span-full text-center py-8">
                <i class="fas fa-exclamation-circle text-4xl text-red-500 mb-2"></i>
                <p class="text-gray-600">Error loading search results</p>
            </div>
        `;
    }
};

// Create modal product card
function createModalProductCard(product) {
    const card = document.createElement('div');
    card.className = 'bg-white rounded-lg overflow-hidden hover:shadow-lg transition-shadow cursor-pointer';
    card.onclick = () => {
        closeSearchModal();
        if (window.openProductModal) {
            window.openProductModal(product.id);
        }
    };

    const imageUrl = product.imageUrl || '/images/placeholder-product.jpg';
    const price = product.discountPrice || product.price || 0;
    const originalPrice = product.discountPrice ? product.price : null;

    card.innerHTML = `
        <div class="aspect-square bg-gray-100">
            <img src="${imageUrl}" alt="${product.name || 'Product'}" class="w-full h-full object-cover" />
        </div>
        <div class="p-3">
            <h4 class="font-semibold text-sm text-gray-900 mb-1 line-clamp-2">${product.name || 'Product'}</h4>
            <div class="flex items-baseline gap-2">
                <span class="text-primary font-bold">${formatPriceVND(price)}</span>
                ${originalPrice ? `<span class="text-gray-400 text-sm line-through">${formatPriceVND(originalPrice)}</span>` : ''}
            </div>
        </div>
    `;

    return card;
}

// Format price helper
function formatPriceVND(price) {
    if (typeof price !== 'number') price = parseInt(price) || 0;
    return price.toLocaleString('vi-VN') + '';
}

// ========================================
// Event Listeners
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    console.log('[Search] Initializing search functionality');

    // Search input event listener
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function(e) {
            const searchTerm = e.target.value.trim();

            // Clear previous timeout
            if (searchDebounceTimeout) {
                clearTimeout(searchDebounceTimeout);
            }

            // Debounce search - wait 300ms after user stops typing
            searchDebounceTimeout = setTimeout(() => {
                performModalSearch(searchTerm);
            }, 300);
        });

        // Enter key to view all results
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                viewAllSearchResults();
            }
        });
    }

    // Escape key to close modal
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const searchModal = document.getElementById('searchModal');
            if (searchModal && searchModal.classList.contains('show')) {
                closeSearchModal();
            }
        }
    });
});

console.log('[Search] Search module loaded successfully');
