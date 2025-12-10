// Admin Products Grid View JavaScript

let currentPage = 1;
let currentPageSize = 12;
let currentView = 'grid';
let currentFilters = {
    search: '',
    categoryId: null,
    inStock: null
};
let productToDelete = null;

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    loadProducts();
    setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
    // Search input - debounce
    let searchTimeout;
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                currentFilters.search = this.value;
                currentPage = 1;
                loadProducts();
            }, 500);
        });
    }
}

// Switch between grid and list view
function switchView(view) {
    currentView = view;
    const gridView = document.getElementById('gridView');
    const listView = document.getElementById('listView');
    const gridBtn = document.getElementById('gridViewBtn');
    const listBtn = document.getElementById('listViewBtn');

    if (view === 'grid') {
        gridView.classList.remove('hidden');
        listView.classList.add('hidden');
        gridBtn.classList.add('bg-primary', 'text-white');
        gridBtn.classList.remove('hover:bg-gray-100');
        listBtn.classList.remove('bg-primary', 'text-white');
        listBtn.classList.add('hover:bg-gray-100');
    } else {
        gridView.classList.add('hidden');
        listView.classList.remove('hidden');
        listBtn.classList.add('bg-primary', 'text-white');
        listBtn.classList.remove('hover:bg-gray-100');
        gridBtn.classList.remove('bg-primary', 'text-white');
        gridBtn.classList.add('hover:bg-gray-100');
    }

    loadProducts();
}

// Apply filters
function applyFilters() {
    const categoryFilter = document.getElementById('categoryFilter');
    const stockFilter = document.getElementById('stockFilter');

    currentFilters.categoryId = categoryFilter?.value || null;
    currentFilters.inStock = stockFilter?.value ? (stockFilter.value === 'true') : null;

    currentPage = 1;
    loadProducts();
}

// Clear filters
function clearFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('stockFilter').value = '';

    currentFilters = {
        search: '',
        categoryId: null,
        inStock: null
    };

    currentPage = 1;
    loadProducts();
}

// Load products from API
async function loadProducts() {
    try {
        showLoading();

        const params = new URLSearchParams({
            pageNumber: currentPage,
            pageSize: currentPageSize
        });

        if (currentFilters.search) params.append('search', currentFilters.search);
        if (currentFilters.categoryId) params.append('categoryIds', currentFilters.categoryId);
        if (currentFilters.inStock !== null) params.append('inStock', currentFilters.inStock);

        const response = await fetch(`https://localhost:7045/api/v1/products?${params.toString()}`);
        const result = await response.json();

        if (result.success && result.data) {
            if (currentView === 'grid') {
                renderProductsGrid(result.data.items);
            } else {
                renderProductsList(result.data.items);
            }
            updatePagination(result.data);
        } else {
            showError('Failed to load products');
        }
    } catch (error) {
        console.error('Error loading products:', error);
        showError('Error loading products. Please try again.');
    }
}

// Render products in grid view
function renderProductsGrid(products) {
    const grid = document.getElementById('productsGrid');

    if (!products || products.length === 0) {
        grid.innerHTML = `
            <div class="col-span-full text-center py-12">
                <i class="fas fa-inbox text-6xl text-gray-300 mb-4"></i>
                <p class="text-gray-500 text-lg">No products found</p>
                <a href="/admin/products/create" class="inline-block mt-4 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark transition">
                    Create Your First Product
                </a>
            </div>
        `;
        return;
    }

    grid.innerHTML = products.map(product => `
        <div class="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition group">
            <!-- Product Image -->
            <div class="relative aspect-square bg-gray-100 overflow-hidden">
                ${product.imageUrl ? `
                    <img src="${product.imageUrl}"
                         alt="${product.name}"
                         class="w-full h-full object-cover group-hover:scale-105 transition duration-300"
                         onerror="this.style.display='none'; this.parentElement.innerHTML='<div class=\'w-full h-full flex items-center justify-center bg-gray-200\'><i class=\'fas fa-image text-4xl text-gray-400\'></i></div>'">
                ` : `
                    <div class="w-full h-full flex items-center justify-center bg-gray-200">
                        <i class="fas fa-image text-4xl text-gray-400"></i>
                    </div>
                `}

                <!-- Status Badges -->
                <div class="absolute top-2 right-2 flex flex-col gap-1">
                    ${product.isFeatured ? '<span class="px-2 py-1 bg-yellow-500 text-white text-xs rounded shadow">Featured</span>' : ''}
                    ${product.stockQuantity === 0 ? '<span class="px-2 py-1 bg-red-500 text-white text-xs rounded shadow">Out of Stock</span>' : ''}
                    ${product.discountPrice ? '<span class="px-2 py-1 bg-green-500 text-white text-xs rounded shadow">Sale</span>' : ''}
                </div>
            </div>

            <!-- Product Info -->
            <div class="p-4">
                <h3 class="font-semibold text-gray-900 mb-1 truncate" title="${product.name}">${product.name}</h3>

                <!-- Categories -->
                ${product.categories && product.categories.length > 0 ? `
                    <div class="flex flex-wrap gap-1 mb-2">
                        ${product.categories.slice(0, 2).map(cat => `
                            <span class="text-xs px-2 py-0.5 bg-blue-100 text-blue-700 rounded">${cat.name}</span>
                        `).join('')}
                        ${product.categories.length > 2 ? `<span class="text-xs text-gray-500">+${product.categories.length - 2}</span>` : ''}
                    </div>
                ` : '<div class="mb-2 text-xs text-gray-400">No category</div>'}

                <!-- Price -->
                <div class="mb-3">
                    ${product.discountPrice ? `
                        <div class="flex items-baseline gap-2">
                            <span class="text-lg font-bold text-green-600">${formatCurrency(product.discountPrice)}</span>
                            <span class="text-sm text-gray-400 line-through">${formatCurrency(product.price)}</span>
                        </div>
                    ` : `
                        <span class="text-lg font-bold text-gray-900">${formatCurrency(product.price)}</span>
                    `}
                </div>

                <!-- Stock and SKU -->
                <div class="flex justify-between items-center text-xs text-gray-500 mb-3">
                    <span>Stock: <strong class="${product.stockQuantity > 0 ? 'text-gray-900' : 'text-red-600'}">${product.stockQuantity}</strong></span>
                    <span>${product.sku || 'No SKU'}</span>
                </div>

                <!-- Status Info -->
                <div class="flex items-center gap-2 text-xs text-gray-500 border-t pt-3">
                    <span class="flex items-center gap-1">
                        <i class="fas fa-circle ${product.stockQuantity > 0 ? 'text-green-500' : 'text-red-500'}" style="font-size: 6px;"></i>
                        ${product.stockQuantity > 0 ? 'Published' : 'Out of Stock'}
                    </span>
                    <span>•</span>
                    <span>Last edited: ${formatDate(product.updatedAt || product.createdAt)}</span>
                </div>

                <!-- Actions -->
                <div class="flex gap-2 mt-3">
                    <a href="/admin/products/details/${product.id}"
                       class="flex-1 px-3 py-2 bg-gray-600 text-white text-sm rounded hover:bg-gray-700 transition text-center"
                       title="View Details">
                        <i class="fas fa-eye"></i>
                    </a>
                    <a href="/admin/products/edit/${product.id}"
                       class="flex-1 px-3 py-2 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 transition text-center">
                        <i class="fas fa-edit mr-1"></i>Edit
                    </a>
                    <button onclick="deleteProduct('${product.id}', '${escapeHtml(product.name)}')"
                            class="px-3 py-2 bg-red-600 text-white text-sm rounded hover:bg-red-700 transition">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

// Render products in list view
function renderProductsList(products) {
    const tbody = document.getElementById('productsListBody');

    if (!products || products.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="px-4 py-12 text-center text-gray-500">
                    <i class="fas fa-inbox text-4xl mb-3"></i>
                    <p>No products found</p>
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = products.map(product => `
        <tr class="hover:bg-gray-50">
            <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                    ${product.imageUrl ? `
                        <img src="${product.imageUrl}"
                             alt="${product.name}"
                             class="w-12 h-12 object-cover rounded"
                             onerror="this.style.display='none'; this.parentElement.innerHTML+='<div class=\'w-12 h-12 flex items-center justify-center bg-gray-200 rounded\'><i class=\'fas fa-image text-gray-400\'></i></div>'">
                    ` : `
                        <div class="w-12 h-12 flex items-center justify-center bg-gray-200 rounded">
                            <i class="fas fa-image text-gray-400"></i>
                        </div>
                    `}
                    <div>
                        <div class="font-medium text-gray-900">${product.name}</div>
                        <div class="text-sm text-gray-500">${product.sku || 'No SKU'}</div>
                    </div>
                </div>
            </td>
            <td class="px-4 py-3">
                ${product.categories?.map(cat => `<span class="text-xs px-2 py-1 bg-blue-100 text-blue-700 rounded mr-1">${cat.name}</span>`).join('') || '<span class="text-gray-400">-</span>'}
            </td>
            <td class="px-4 py-3">
                ${product.discountPrice ? `
                    <div><span class="font-semibold text-green-600">${formatCurrency(product.discountPrice)}</span></div>
                    <div class="text-sm text-gray-400 line-through">${formatCurrency(product.price)}</div>
                ` : `<span class="font-semibold">${formatCurrency(product.price)}</span>`}
            </td>
            <td class="px-4 py-3">
                <span class="${product.stockQuantity > 0 ? 'text-gray-900' : 'text-red-600'} font-medium">${product.stockQuantity}</span>
            </td>
            <td class="px-4 py-3">
                ${product.isFeatured ? '<span class="inline-block px-2 py-1 text-xs bg-yellow-100 text-yellow-800 rounded mr-1">Featured</span>' : ''}
                ${product.stockQuantity > 0
                    ? '<span class="inline-block px-2 py-1 text-xs bg-green-100 text-green-800 rounded">Published</span>'
                    : '<span class="inline-block px-2 py-1 text-xs bg-red-100 text-red-800 rounded">Out of Stock</span>'}
            </td>
            <td class="px-4 py-3">
                <div class="flex gap-2">
                    <a href="/admin/products/details/${product.id}" class="px-3 py-1 bg-gray-600 text-white text-sm rounded hover:bg-gray-700" title="View Details">
                        <i class="fas fa-eye"></i>
                    </a>
                    <a href="/admin/products/edit/${product.id}" class="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700">
                        <i class="fas fa-edit"></i>
                    </a>
                    <button onclick="deleteProduct('${product.id}', '${escapeHtml(product.name)}')" class="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// Update pagination
function updatePagination(data) {
    document.getElementById('showingFrom').textContent = data.items.length > 0 ? ((data.pageNumber - 1) * data.pageSize) + 1 : 0;
    document.getElementById('showingTo').textContent = Math.min(data.pageNumber * data.pageSize, data.totalCount);
    document.getElementById('totalProducts').textContent = data.totalCount;

    const controls = document.getElementById('paginationControls');
    const totalPages = data.totalPages;

    let html = '';

    // Previous button
    html += `
        <button onclick="changePage(${data.pageNumber - 1})"
                ${data.pageNumber === 1 ? 'disabled' : ''}
                class="px-3 py-2 border rounded ${data.pageNumber === 1 ? 'bg-gray-100 text-gray-400 cursor-not-allowed' : 'bg-white hover:bg-gray-50 border-gray-300'}">
            <i class="fas fa-chevron-left"></i>
        </button>
    `;

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= data.pageNumber - 1 && i <= data.pageNumber + 1)) {
            html += `
                <button onclick="changePage(${i})"
                        class="px-4 py-2 border rounded ${i === data.pageNumber ? 'bg-primary text-white border-primary' : 'bg-white border-gray-300 hover:bg-gray-50'}">
                    ${i}
                </button>
            `;
        } else if (i === data.pageNumber - 2 || i === data.pageNumber + 2) {
            html += '<span class="px-2 py-2">...</span>';
        }
    }

    // Next button
    html += `
        <button onclick="changePage(${data.pageNumber + 1})"
                ${data.pageNumber === totalPages ? 'disabled' : ''}
                class="px-3 py-2 border rounded ${data.pageNumber === totalPages ? 'bg-gray-100 text-gray-400 cursor-not-allowed' : 'bg-white hover:bg-gray-50 border-gray-300'}">
            <i class="fas fa-chevron-right"></i>
        </button>
    `;

    controls.innerHTML = html;
}

// Change page
function changePage(page) {
    currentPage = page;
    loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Delete product
function deleteProduct(productId, productName) {
    productToDelete = { id: productId, name: productName };
    document.getElementById('deleteProductName').textContent = productName;
    document.getElementById('deleteModal').classList.remove('hidden');
}

// Close delete modal
function closeDeleteModal() {
    document.getElementById('deleteModal').classList.add('hidden');
    productToDelete = null;
}

// Confirm delete
async function confirmDelete() {
    if (!productToDelete) return;

    try {
        const response = await fetch(`https://localhost:7045/api/v1/products/${productToDelete.id}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (result.success) {
            showSuccess(`Product "${productToDelete.name}" deleted successfully`);
            closeDeleteModal();
            loadProducts();
        } else {
            showError(result.message || 'Failed to delete product');
        }
    } catch (error) {
        console.error('Error deleting product:', error);
        showError('Error deleting product. Please try again.');
    }
}

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', { year: 'numeric', month: 'short', day: 'numeric' });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showLoading() {
    if (currentView === 'grid') {
        const grid = document.getElementById('productsGrid');
        grid.innerHTML = `
            <div class="col-span-full flex justify-center items-center py-12">
                <i class="fas fa-spinner fa-spin text-3xl text-gray-400 mr-3"></i>
                <span class="text-gray-500">Loading products...</span>
            </div>
        `;
    } else {
        const tbody = document.getElementById('productsListBody');
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="px-4 py-12 text-center">
                    <i class="fas fa-spinner fa-spin text-3xl text-gray-400 mr-3"></i>
                    <span class="text-gray-500">Loading products...</span>
                </td>
            </tr>
        `;
    }
}

function showError(message) {
    alert(message); // Replace with better notification system
}

function showSuccess(message) {
    alert(message); // Replace with better notification system
}
