// Admin Product Form JavaScript (Create & Edit)

document.addEventListener('DOMContentLoaded', function() {
    setupFormHandlers();
    setupImagePreview();
    setupProductTypeSelector();
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

// Setup product type selector to show/hide dynamic options
function setupProductTypeSelector() {
    const productTypeSelect = document.getElementById('productType');
    if (!productTypeSelect) {
        console.error('Product type select not found!');
        return;
    }

    console.log('Product type selector found, setting up event listener');

    productTypeSelect.addEventListener('change', function() {
        const selectedType = this.value;
        console.log('Product type changed to:', selectedType);

        // Hide all product option sections
        const coffeeOptions = document.getElementById('coffeeOptions');
        const clothesOptions = document.getElementById('clothesOptions');
        const placeholder = document.getElementById('optionsPlaceholder');

        if (!coffeeOptions || !clothesOptions || !placeholder) {
            console.error('One or more option sections not found');
            return;
        }

        // Reset visibility
        coffeeOptions.classList.add('hidden');
        clothesOptions.classList.add('hidden');
        placeholder.classList.add('hidden');

        // Filter categories based on product type
        filterCategoriesByType(selectedType);

        // Show relevant section based on selected type
        if (selectedType === 'coffee') {
            console.log('Showing coffee options');
            coffeeOptions.classList.remove('hidden');
            // Clear clothes-specific fields
            clearClothesFields();
        } else if (selectedType === 'clothes') {
            console.log('Showing clothes options');
            clothesOptions.classList.remove('hidden');
            // Clear coffee-specific fields
            clearCoffeeFields();
        } else {
            console.log('Showing placeholder');
            placeholder.classList.remove('hidden');
        }
    });

    // Trigger change event if there's already a selected value
    if (productTypeSelect.value) {
        productTypeSelect.dispatchEvent(new Event('change'));
    }
}

// Filter categories based on product type
function filterCategoriesByType(productType) {
    const categorySelect = document.getElementById('categoryId');
    if (!categorySelect) return;

    // Store all options (from ViewBag.Categories) if not already stored
    if (!categorySelect.dataset.allOptions) {
        const options = Array.from(categorySelect.options);
        categorySelect.dataset.allOptions = JSON.stringify(
            options.map(opt => ({ value: opt.value, text: opt.text }))
        );
    }

    // Clear current options
    categorySelect.innerHTML = '<option value="">Select a category</option>';

    if (productType === 'coffee') {
        // For coffee: Use categories from ViewBag.Categories (database)
        const allOptions = JSON.parse(categorySelect.dataset.allOptions);
        allOptions.forEach(option => {
            if (option.value !== '') {
                const newOption = document.createElement('option');
                newOption.value = option.value;
                newOption.text = option.text;
                categorySelect.appendChild(newOption);
            }
        });
        console.log('Loaded coffee categories from database');

    } else if (productType === 'clothes') {
        // For clothes: Use mock data
        const clothesCategories = ['T-shirt', 'Jeans', 'Coat', 'Hoodie', 'Jacket'];

        clothesCategories.forEach((categoryName, index) => {
            const newOption = document.createElement('option');
            // Use a temporary ID format for mock categories (will be handled on backend)
            newOption.value = `clothes-${index + 1}`;
            newOption.text = categoryName;
            categorySelect.appendChild(newOption);
        });
        console.log('Loaded clothes categories from mock data');

    } else {
        // No type selected: show all from database
        const allOptions = JSON.parse(categorySelect.dataset.allOptions);
        allOptions.forEach(option => {
            if (option.value !== '') {
                const newOption = document.createElement('option');
                newOption.value = option.value;
                newOption.text = option.text;
                categorySelect.appendChild(newOption);
            }
        });
    }
}

// Clear coffee-specific fields when switching to clothes
function clearCoffeeFields() {
    const coffeeFields = ['availableSizes', 'availableGrinds', 'region', 'process', 'tastingNote'];
    coffeeFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) field.value = '';
    });
}

// Clear clothes-specific fields when switching to coffee
function clearClothesFields() {
    const clothesFields = ['clothesSizes', 'availableColors', 'material', 'style'];
    clothesFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) field.value = '';
    });
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
    // Get product type
    const productType = document.getElementById('productType')?.value;

    // Get basic fields
    const formData = {
        name: document.getElementById('name').value.trim(),
        description: document.getElementById('description').value.trim(),
        shortDescription: document.getElementById('shortDescription')?.value.trim() || null,
        price: parseFloat(document.getElementById('price').value),
        discountPrice: parseFloat(document.getElementById('discountPrice')?.value) || null,
        stockQuantity: parseInt(document.getElementById('stockQuantity')?.value || 0),
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

    // Get product-specific fields based on type
    if (productType === 'coffee') {
        // Get coffee-specific fields
        const sizesInput = document.getElementById('availableSizes')?.value.trim();
        if (sizesInput) {
            formData.availableSizes = sizesInput.split(',').map(s => s.trim()).filter(s => s);
        }

        const grindsInput = document.getElementById('availableGrinds')?.value.trim();
        if (grindsInput) {
            formData.availableGrinds = grindsInput.split(',').map(g => g.trim()).filter(g => g);
        }

        const region = document.getElementById('region')?.value.trim();
        if (region) {
            formData.region = region;
        }

        const process = document.getElementById('process')?.value.trim();
        if (process) {
            formData.process = process;
        }

        const tastingNote = document.getElementById('tastingNote')?.value.trim();
        if (tastingNote) {
            formData.tastingNote = tastingNote;
        }

    } else if (productType === 'clothes') {
        // Get clothes-specific fields
        const clothesSizesInput = document.getElementById('clothesSizes')?.value.trim();
        if (clothesSizesInput) {
            formData.availableSizes = clothesSizesInput.split(',').map(s => s.trim()).filter(s => s);
        }

        const colorsInput = document.getElementById('availableColors')?.value.trim();
        if (colorsInput) {
            formData.availableColors = colorsInput.split(',').map(c => c.trim()).filter(c => c);
        }

        const material = document.getElementById('material')?.value.trim();
        if (material) {
            formData.material = material;
        }

        const style = document.getElementById('style')?.value.trim();
        if (style) {
            formData.style = style;
        }
    }

    return formData;
}

// Validate form data
function validateFormData(data) {
    // Check product type is selected
    const productType = document.getElementById('productType')?.value;
    if (!productType) {
        showError('Product type is required');
        return false;
    }

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

    if (data.stockQuantity < 0) {
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
