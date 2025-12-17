// Admin Product Form JavaScript (Create & Edit)

// Global variable to store product type attributes
let productTypeAttributes = {};
let currentProductTypeId = null;

// API Base URL (can be overridden by setting window.API_BASE_URL before loading this script)
const API_BASE_URL = window.API_BASE_URL || '';

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

// Setup product type selector to fetch and render dynamic attributes
function setupProductTypeSelector() {
    const productTypeSelect = document.getElementById('productType');
    if (!productTypeSelect) {
        console.error('Product type select not found!');
        return;
    }

    console.log('Product type selector found, setting up event listener');

    productTypeSelect.addEventListener('change', async function() {
        const selectedProductTypeId = this.value;
        console.log('Product type changed to:', selectedProductTypeId);

        // Clear dynamic options container
        const dynamicOptionsContainer = document.getElementById('dynamicProductOptions');
        if (!dynamicOptionsContainer) {
            console.error('Dynamic options container not found');
            return;
        }

        if (!selectedProductTypeId) {
            dynamicOptionsContainer.innerHTML = '<p class="text-gray-500 text-sm">Please select a product type to see available options.</p>';
            currentProductTypeId = null;
            return;
        }

        // Show loading state
        dynamicOptionsContainer.innerHTML = '<p class="text-gray-500 text-sm"><i class="fas fa-spinner fa-spin mr-2"></i>Loading product options...</p>';

        try {
            // Fetch product type with attributes
            const response = await fetch(`${API_BASE_URL}/api/v1/producttypes/${selectedProductTypeId}/attributes-with-values`);
            const result = await response.json();

            if (result.success && result.data) {
                currentProductTypeId = selectedProductTypeId;
                productTypeAttributes = result.data.attributes || [];
                renderDynamicAttributes(productTypeAttributes);
            } else {
                dynamicOptionsContainer.innerHTML = '<p class="text-red-500 text-sm">Failed to load product options. Please try again.</p>';
            }
        } catch (error) {
            console.error('Error fetching product type attributes:', error);
            dynamicOptionsContainer.innerHTML = '<p class="text-red-500 text-sm">Error loading product options. Please try again.</p>';
        }
    });

    // Trigger change event if there's already a selected value
    if (productTypeSelect.value) {
        productTypeSelect.dispatchEvent(new Event('change'));
    }
}

// Render dynamic attributes based on product type
function renderDynamicAttributes(attributes) {
    const dynamicOptionsContainer = document.getElementById('dynamicProductOptions');
    if (!dynamicOptionsContainer) return;

    if (!attributes || attributes.length === 0) {
        dynamicOptionsContainer.innerHTML = `
            <div class="text-center py-6">
                <p class="text-gray-500 text-sm mb-4">No additional options available for this product type.</p>
                <button type="button"
                        onclick="openManageAttributesModal('${currentProductTypeId}')"
                        class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:ring-2 focus:ring-blue-500">
                    <i class="fas fa-plus mr-2"></i>Add Attributes
                </button>
            </div>
        `;
        return;
    }

    let html = '';

    // Add "Manage Attributes" button at the top
    html += `
        <div class="mb-4 flex justify-between items-center">
            <h4 class="text-sm font-semibold text-gray-700">Product Attributes</h4>
            <button type="button"
                    onclick="openManageAttributesModal('${currentProductTypeId}')"
                    class="px-3 py-1 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:ring-2 focus:ring-blue-500">
                <i class="fas fa-cog mr-1"></i>Manage Attributes
            </button>
        </div>
    `;

    html += '<div class="grid grid-cols-1 md:grid-cols-2 gap-4">';

    attributes.forEach(attribute => {
        html += renderAttributeField(attribute);
    });

    html += '</div>';
    dynamicOptionsContainer.innerHTML = html;

    // Initialize any special interactions after rendering
    initializeDynamicFieldInteractions();
}

// Render individual attribute field
function renderAttributeField(attribute) {
    const hasValues = attribute.values && attribute.values.length > 0;
    const isRequired = attribute.isRequired ? '<span class="text-red-500">*</span>' : '';
    const fieldId = `attr_${attribute.id}`;

    let html = '<div>';
    html += `<label for="${fieldId}" class="block text-sm font-medium text-gray-700 mb-2">`;
    html += `${attribute.displayName || attribute.name} ${isRequired}`;
    html += '</label>';

    if (hasValues) {
        if (attribute.allowMultipleSelection) {
            // Multi-select dropdown
            html += renderMultiSelectDropdown(attribute, fieldId);
        } else {
            // Single-select dropdown
            html += renderSingleSelectDropdown(attribute, fieldId);
        }
    } else {
        // Text input for free-form values
        html += `<input type="text" id="${fieldId}" name="${fieldId}"
                        class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                        placeholder="Enter ${attribute.displayName || attribute.name}"
                        ${attribute.isRequired ? 'required' : ''}>`;
    }

    html += '</div>';
    return html;
}

// Render multi-select dropdown
function renderMultiSelectDropdown(attribute, fieldId) {
    let html = '<div class="relative">';
    html += `<button type="button" id="${fieldId}_button"
                    onclick="toggleDropdown('${fieldId}')"
                    class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent bg-white text-left flex items-center justify-between min-h-[42px]">
                <div id="${fieldId}_selected" class="flex-1 flex items-center gap-2 flex-wrap">
                    <span class="text-gray-500">Select ${attribute.displayName || attribute.name}</span>
                </div>
                <i class="fas fa-chevron-down text-gray-400 ml-2"></i>
            </button>`;

    html += `<div id="${fieldId}_menu" class="hidden absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">`;
    html += '<div class="py-1">';

    attribute.values.forEach(value => {
        html += `<label class="flex items-center px-4 py-2 hover:bg-gray-50 cursor-pointer">
                    <input type="checkbox" value="${value.value}"
                           class="multi-select-checkbox w-4 h-4 accent-primary border-gray-300 rounded focus:ring-primary"
                           data-attribute-id="${attribute.id}"
                           onchange="updateMultiSelectDisplay('${fieldId}')">
                    <span class="ml-3 text-sm text-gray-700">${value.value}</span>
                </label>`;
    });

    html += '</div></div>';
    html += `<input type="hidden" id="${fieldId}" name="${fieldId}" value="">`;
    html += '</div>';

    return html;
}

// Render single-select dropdown
function renderSingleSelectDropdown(attribute, fieldId) {
    let html = `<select id="${fieldId}" name="${fieldId}"
                       class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                       ${attribute.isRequired ? 'required' : ''}>`;
    html += '<option value="">Select an option</option>';

    attribute.values.forEach(value => {
        html += `<option value="${value.value}">${value.value}</option>`;
    });

    html += '</select>';
    return html;
}

// Toggle dropdown visibility
function toggleDropdown(fieldId) {
    const menu = document.getElementById(`${fieldId}_menu`);
    if (menu) {
        menu.classList.toggle('hidden');
    }

    // Close other dropdowns
    document.querySelectorAll('[id$="_menu"]').forEach(otherMenu => {
        if (otherMenu.id !== `${fieldId}_menu`) {
            otherMenu.classList.add('hidden');
        }
    });
}

// Update multi-select display
function updateMultiSelectDisplay(fieldId) {
    const container = document.getElementById(`${fieldId}_selected`);
    const hiddenInput = document.getElementById(fieldId);
    const checkboxes = document.querySelectorAll(`#${fieldId}_menu input[type="checkbox"]:checked`);

    const selectedValues = Array.from(checkboxes).map(cb => cb.value);
    hiddenInput.value = JSON.stringify(selectedValues);

    if (selectedValues.length > 0) {
        let html = '';
        selectedValues.forEach(value => {
            html += `<span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-primary/10 text-primary">
                        ${value}
                    </span>`;
        });
        container.innerHTML = html;
    } else {
        const attributeName = container.closest('div').querySelector('label').textContent.trim();
        container.innerHTML = `<span class="text-gray-500">Select ${attributeName}</span>`;
    }
}

// Initialize dynamic field interactions
function initializeDynamicFieldInteractions() {
    // Close dropdowns when clicking outside
    document.addEventListener('click', function(event) {
        if (!event.target.closest('[id$="_button"]') && !event.target.closest('[id$="_menu"]')) {
            document.querySelectorAll('[id$="_menu"]').forEach(menu => {
                menu.classList.add('hidden');
            });
        }
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

        const response = await fetch(`${API_BASE_URL}/api/v1/products`, {
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

        const response = await fetch(`${API_BASE_URL}/api/v1/products/${productId}`, {
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
    const productTypeId = document.getElementById('productType')?.value;

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
        isFeatured: document.getElementById('isFeatured')?.checked || false,
        productTypeId: productTypeId || null
    };

    // Get category ID from dropdown
    const categorySelect = document.getElementById('categoryId');
    const categoryId = categorySelect?.value;
    if (categoryId) {
        formData.categoryId = categoryId;
    }

    // Collect dynamic attribute values
    const attributeSelections = [];

    if (productTypeAttributes && productTypeAttributes.length > 0) {
        productTypeAttributes.forEach(attribute => {
            const fieldId = `attr_${attribute.id}`;
            const field = document.getElementById(fieldId);

            if (field) {
                let value = field.value;

                // Handle multi-select fields (stored as JSON in hidden input)
                if (attribute.allowMultipleSelection && attribute.values && attribute.values.length > 0) {
                    try {
                        const selectedValues = JSON.parse(value || '[]');
                        if (selectedValues.length > 0) {
                            // Find the actual value IDs from the attribute
                            selectedValues.forEach(val => {
                                const attributeValue = attribute.values.find(v => v.value === val);
                                if (attributeValue) {
                                    attributeSelections.push({
                                        productAttributeId: attribute.id,
                                        productAttributeValueId: attributeValue.id
                                    });
                                }
                            });
                        }
                    } catch (e) {
                        console.error('Error parsing multi-select value:', e);
                    }
                } else if (value && value.trim() !== '') {
                    // Handle single-select or text input
                    if (attribute.values && attribute.values.length > 0) {
                        // Single-select dropdown
                        const attributeValue = attribute.values.find(v => v.value === value);
                        if (attributeValue) {
                            attributeSelections.push({
                                productAttributeId: attribute.id,
                                productAttributeValueId: attributeValue.id
                            });
                        }
                    } else {
                        // Free-form text input - create custom value
                        // Note: Backend needs to handle creating custom attribute values
                        attributeSelections.push({
                            productAttributeId: attribute.id,
                            customValue: value.trim()
                        });
                    }
                }
            }
        });
    }

    if (attributeSelections.length > 0) {
        formData.productAttributeSelections = attributeSelections;
    }

    // Collect all uploaded images
    if (typeof uploadedImages !== 'undefined' && uploadedImages.length > 0) {
        formData.images = uploadedImages
            .filter(img => img.uploadedUrl) // Only include successfully uploaded images
            .map((img, index) => ({
                imageUrl: img.uploadedUrl,
                altText: img.name || null,
                displayOrder: index,
                isPrimary: img.isDefault || false
            }));
    }

    return formData;
}

// Validate form data
function validateFormData(data) {
    // Check product type is selected
    if (!data.productTypeId) {
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

    // Validate required dynamic attributes
    if (productTypeAttributes && productTypeAttributes.length > 0) {
        for (const attribute of productTypeAttributes) {
            if (attribute.isRequired) {
                const fieldId = `attr_${attribute.id}`;
                const field = document.getElementById(fieldId);

                if (!field || !field.value || field.value.trim() === '' || field.value === '[]') {
                    showError(`${attribute.displayName || attribute.name} is required`);
                    return false;
                }
            }
        }
    }

    if (!data.price || data.price <= 0) {
        showError('Valid price is required');
        return false;
    }

    if (data.stockQuantity < 0) {
        showError('Valid stock quantity is required');
        return false;
    }

    if (!data.imageUrl && (!data.images || data.images.length === 0)) {
        showError('At least one product image is required');
        return false;
    }

    if (!data.categoryId) {
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
