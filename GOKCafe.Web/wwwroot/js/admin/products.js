// Admin Products List Page JavaScript

let currentPage = 1;
let currentPageSize = 20;
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

// Apply filters
function applyFilters() {
    const searchInput = document.getElementById('searchInput');
    const categoryFilter = document.getElementById('categoryFilter');
    const stockFilter = document.getElementById('stockFilter');

    currentFilters.search = searchInput?.value || '';
    currentFilters.categoryId = categoryFilter?.value || null;
    currentFilters.inStock = stockFilter?.value ? (stockFilter.value === 'true') : null;

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
            renderProducts(result.data.items);
            updatePagination(result.data);
        } else {
            showError('Failed to load products');
        }
    } catch (error) {
        console.error('Error loading products:', error);
        showError('Error loading products. Please try again.');
    }
}

// Render products table
function renderProducts(products) {
    const tbody = document.getElementById('productsTableBody');

    if (!products || products.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="px-6 py-8 text-center text-gray-500">
                    <i class="fas fa-inbox text-4xl mb-3"></i>
                    <p>No products found</p>
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = products.map(product => `
        <tr class="hover:bg-gray-50 transition">
            <td class="px-6 py-4">
                <img src="${product.imageUrl || '/images/placeholder.jpg'}"
                     alt="${product.name}"
                     class="w-16 h-16 object-cover rounded-lg"
                     onerror="this.src='/images/placeholder.jpg'">
            </td>
            <td class="px-6 py-4">
                <div class="font-semibold text-gray-900">${product.name}</div>
                <div class="text-sm text-gray-500">${product.sku || 'N/A'}</div>
            </td>
            <td class="px-6 py-4">
                <div class="flex flex-wrap gap-1">
                    ${product.categories?.map(cat => `
                        <span class="inline-block px-2 py-1 text-xs bg-blue-100 text-blue-800 rounded">${cat.name}</span>
                    `).join('') || '<span class="text-gray-400">N/A</span>'}
                </div>
            </td>
            <td class="px-6 py-4">
                <div class="font-semibold text-gray-900">${formatCurrency(product.price)}</div>
                ${product.discountPrice ? `<div class="text-sm text-green-600">${formatCurrency(product.discountPrice)}</div>` : ''}
            </td>
            <td class="px-6 py-4">
                <span class="font-semibold ${product.stockQuantity > 0 ? 'text-gray-900' : 'text-red-600'}">
                    ${product.stockQuantity}
                </span>
            </td>
            <td class="px-6 py-4">
                ${product.isFeatured ? '<span class="inline-block px-2 py-1 text-xs bg-yellow-100 text-yellow-800 rounded">Featured</span>' : ''}
                ${product.stockQuantity > 0
                    ? '<span class="inline-block px-2 py-1 text-xs bg-green-100 text-green-800 rounded">In Stock</span>'
                    : '<span class="inline-block px-2 py-1 text-xs bg-red-100 text-red-800 rounded">Out of Stock</span>'}
            </td>
            <td class="px-6 py-4">
                <div class="flex space-x-2">
                    <a href="/admin/products/edit/${product.id}"
                       class="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 transition text-sm"
                       title="Edit">
                        <i class="fas fa-edit"></i>
                    </a>
                    <button onclick="deleteProduct('${product.id}', '${product.name}')"
                            class="px-3 py-1 bg-red-600 text-white rounded hover:bg-red-700 transition text-sm"
                            title="Delete">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// Update pagination
function updatePagination(data) {
    document.getElementById('showingFrom').textContent = ((data.pageNumber - 1) * data.pageSize) + 1;
    document.getElementById('showingTo').textContent = Math.min(data.pageNumber * data.pageSize, data.totalCount);
    document.getElementById('totalProducts').textContent = data.totalCount;

    const controls = document.getElementById('paginationControls');
    const totalPages = data.totalPages;

    let html = '';

    // Previous button
    html += `
        <button onclick="changePage(${data.pageNumber - 1})"
                ${data.pageNumber === 1 ? 'disabled' : ''}
                class="px-3 py-1 border border-gray-300 rounded ${data.pageNumber === 1 ? 'bg-gray-100 text-gray-400' : 'bg-white hover:bg-gray-50'}">
            <i class="fas fa-chevron-left"></i>
        </button>
    `;

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= data.pageNumber - 2 && i <= data.pageNumber + 2)) {
            html += `
                <button onclick="changePage(${i})"
                        class="px-3 py-1 border rounded ${i === data.pageNumber ? 'bg-primary text-white border-primary' : 'bg-white border-gray-300 hover:bg-gray-50'}">
                    ${i}
                </button>
            `;
        } else if (i === data.pageNumber - 3 || i === data.pageNumber + 3) {
            html += '<span class="px-2">...</span>';
        }
    }

    // Next button
    html += `
        <button onclick="changePage(${data.pageNumber + 1})"
                ${data.pageNumber === totalPages ? 'disabled' : ''}
                class="px-3 py-1 border border-gray-300 rounded ${data.pageNumber === totalPages ? 'bg-gray-100 text-gray-400' : 'bg-white hover:bg-gray-50'}">
            <i class="fas fa-chevron-right"></i>
        </button>
    `;

    controls.innerHTML = html;
}

// Change page
function changePage(page) {
    currentPage = page;
    loadProducts();
}

// Delete product
function deleteProduct(productId, productName) {
    productToDelete = { id: productId, name: productName };
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

function showLoading() {
    const tbody = document.getElementById('productsTableBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="7" class="px-6 py-8 text-center">
                <i class="fas fa-spinner fa-spin text-3xl text-gray-400 mr-3"></i>
                <span class="text-gray-500">Loading products...</span>
            </td>
        </tr>
    `;
}

function showError(message) {
    alert(message); // You can replace with a better notification system
}

function showSuccess(message) {
    alert(message); // You can replace with a better notification system
}
