// Admin Product Form JavaScript (Create & Edit)

document.addEventListener('DOMContentLoaded', function() {
    setupFormHandlers();
    setupImagePreview();
});

// Setup form submission handlers
function setupFormHandlers() {
    const createForm = document.getElementById('createProductForm');
    const editForm = document.getElementById('editProductForm');

    if (createForm) {
        createForm.addEventListener('submit', handleCreateProduct);
    }

    if (editForm) {
        editForm.addEventListener('submit', handleUpdateProduct);
    }
}

// Setup image preview
function setupImagePreview() {
    const imageUrlInput = document.getElementById('imageUrl');
    if (imageUrlInput) {
        imageUrlInput.addEventListener('input', function() {
            const url = this.value;
            const preview = document.getElementById('imagePreview');
            const previewImg = document.getElementById('previewImg');

            if (url && isValidUrl(url)) {
                previewImg.src = url;
                preview.classList.remove('hidden');
            } else {
                preview.classList.add('hidden');
            }
        });
    }
}

// Handle create product form submission
async function handleCreateProduct(e) {
    e.preventDefault();

    const formData = getFormData();

    if (!validateFormData(formData)) {
        return;
    }

    try {
        showSubmitting('Creating product...');

        const response = await fetch('https://localhost:7045/api/v1/products', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const result = await response.json();

        if (result.success) {
            showSuccess('Product created successfully!');
            setTimeout(() => {
                window.location.href = '/admin/products';
            }, 1500);
        } else {
            hideSubmitting();
            showError(result.message || 'Failed to create product');
        }
    } catch (error) {
        hideSubmitting();
        console.error('Error creating product:', error);
        showError('Error creating product. Please try again.');
    }
}

// Handle update product form submission
async function handleUpdateProduct(e) {
    e.preventDefault();

    const productId = document.getElementById('productId').value;
    const formData = getFormData();

    if (!validateFormData(formData)) {
        return;
    }

    try {
        showSubmitting('Updating product...');

        const response = await fetch(`https://localhost:7045/api/v1/products/${productId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const result = await response.json();

        if (result.success) {
            showSuccess('Product updated successfully!');
            setTimeout(() => {
                window.location.href = '/admin/products';
            }, 1500);
        } else {
            hideSubmitting();
            showError(result.message || 'Failed to update product');
        }
    } catch (error) {
        hideSubmitting();
        console.error('Error updating product:', error);
        showError('Error updating product. Please try again.');
    }
}

// Get form data
function getFormData() {
    // Get basic fields
    const formData = {
        name: document.getElementById('name').value.trim(),
        description: document.getElementById('description').value.trim(),
        shortDescription: document.getElementById('shortDescription')?.value.trim() || null,
        price: parseFloat(document.getElementById('price').value),
        discountPrice: parseFloat(document.getElementById('discountPrice')?.value) || null,
        stockQuantity: parseInt(document.getElementById('stockQuantity').value),
        sku: document.getElementById('sku')?.value.trim() || null,
        imageUrl: document.getElementById('imageUrl').value.trim(),
        isFeatured: document.getElementById('isFeatured')?.checked || false
    };

    // Get category ID from dropdown
    const categorySelect = document.getElementById('categoryId');
    const categoryId = categorySelect?.value;
    if (categoryId) {
        formData.categoryIds = [categoryId];
    } else {
        formData.categoryIds = [];
    }

    // Get available sizes (comma-separated to array)
    const sizesInput = document.getElementById('availableSizes')?.value.trim();
    if (sizesInput) {
        formData.availableSizes = sizesInput.split(',').map(s => s.trim()).filter(s => s);
    }

    // Get available grinds (comma-separated to array)
    const grindsInput = document.getElementById('availableGrinds')?.value.trim();
    if (grindsInput) {
        formData.availableGrinds = grindsInput.split(',').map(g => g.trim()).filter(g => g);
    }

    return formData;
}

// Validate form data
function validateFormData(data) {
    if (!data.name) {
        showError('Product name is required');
        return false;
    }

    if (!data.description) {
        showError('Description is required');
        return false;
    }

    if (!data.price || data.price <= 0) {
        showError('Valid price is required');
        return false;
    }

    if (!data.stockQuantity || data.stockQuantity < 0) {
        showError('Valid stock quantity is required');
        return false;
    }

    if (!data.imageUrl) {
        showError('Image URL is required');
        return false;
    }

    if (!data.categoryIds || data.categoryIds.length === 0) {
        showError('At least one category must be selected');
        return false;
    }

    return true;
}

// Utility functions
function isValidUrl(string) {
    try {
        new URL(string);
        return true;
    } catch (_) {
        return false;
    }
}

function showSubmitting(message) {
    const submitBtn = document.querySelector('button[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = `<i class="fas fa-spinner fa-spin mr-2"></i>${message}`;
    }
}

function hideSubmitting() {
    const submitBtn = document.querySelector('button[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = false;
        const isEdit = document.getElementById('editProductForm') !== null;
        submitBtn.innerHTML = isEdit
            ? '<i class="fas fa-save mr-2"></i>Update Product'
            : '<i class="fas fa-save mr-2"></i>Create Product';
    }
}

function showError(message) {
    alert(message); // Replace with better notification system
}

function showSuccess(message) {
    alert(message); // Replace with better notification system
}
