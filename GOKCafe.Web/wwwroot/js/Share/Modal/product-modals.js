/**
 * Product Modals Module
 * Handles all modal-related functionality for product management
 * - Add Product Type Modal
 * - Add Category Modal
 * - Manage Attributes Modal (with full CRUD)
 */

export const ProductModals = {
    // Configuration
    API_BASE_URL: null,

    // State
    currentManagingProductTypeId: null,
    currentAttributes: [],

    /**
     * Initialize the module
     * @param {string} apiBaseUrl - Base URL for API calls
     */
    init(apiBaseUrl) {
        this.API_BASE_URL = apiBaseUrl;
        this.setupEventListeners();
    },

    /**
     * Setup event listeners for form submissions
     */
    setupEventListeners() {
        // Add Category Form
        const categoryForm = document.getElementById('addCategoryForm');
        if (categoryForm) {
            categoryForm.addEventListener('submit', (e) => this.handleAddCategorySubmit(e));
        }

        // Add Product Type Form
        const productTypeForm = document.getElementById('addProductTypeForm');
        if (productTypeForm) {
            productTypeForm.addEventListener('submit', (e) => this.handleAddProductTypeSubmit(e));
        }
    },

    // ========================================
    // Add Product Type Modal
    // ========================================

    openAddProductTypeModal() {
        document.getElementById('addProductTypeModal').classList.remove('hidden');
    },

    closeAddProductTypeModal() {
        document.getElementById('addProductTypeModal').classList.add('hidden');
        document.getElementById('addProductTypeForm').reset();
    },

    async handleAddProductTypeSubmit(event) {
        event.preventDefault();

        const name = document.getElementById('newProductTypeName').value.trim();

        // Validate name
        if (!name) {
            alert('Please enter a product type name');
            return;
        }

        try {
            const response = await fetch(`${this.API_BASE_URL}/api/v1/producttypes`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    name: name,
                    description: '',
                    displayOrder: 999
                })
            });

            const result = await response.json();

            if (result.success) {
                // Add new option to dropdown
                const select = document.getElementById('productType');
                const option = new Option(result.data.name, result.data.id);
                select.add(option);

                // Select the newly created product type
                select.value = result.data.id;
                select.dispatchEvent(new Event('change'));

                // Close modal
                this.closeAddProductTypeModal();

                // Show success message
                alert(`Product type "${result.data.name}" created successfully!`);
            } else {
                alert('Failed to create product type: ' + (result.message || 'Unknown error'));
            }
        } catch (error) {
            console.error('Error creating product type:', error);
            alert('Error creating product type. Please try again.');
        }
    },

    // ========================================
    // Add Category Modal
    // ========================================

    openAddCategoryModal() {
        document.getElementById('addCategoryModal').classList.remove('hidden');
    },

    closeAddCategoryModal() {
        document.getElementById('addCategoryModal').classList.add('hidden');
        document.getElementById('addCategoryForm').reset();
    },

    async handleAddCategorySubmit(event) {
        event.preventDefault();

        const name = document.getElementById('newCategoryName').value.trim();

        // Validate name
        if (!name) {
            alert('Please enter a category name');
            return;
        }

        try {
            const response = await fetch(`${this.API_BASE_URL}/api/v1/categories`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    name: name,
                    description: '',
                    displayOrder: 999
                })
            });

            const result = await response.json();

            if (result.success) {
                // Add new option to dropdown
                const select = document.getElementById('categoryId');
                const option = new Option(result.data.name, result.data.id);
                select.add(option);

                // Select the newly created category
                select.value = result.data.id;

                // Close modal
                this.closeAddCategoryModal();

                // Show success message
                alert(`Category "${result.data.name}" created successfully!`);
            } else {
                alert('Failed to create category: ' + (result.message || 'Unknown error'));
            }
        } catch (error) {
            console.error('Error creating category:', error);
            alert('Error creating category. Please try again.');
        }
    },

    // ========================================
    // Manage Attributes Modal (Advanced Version)
    // ========================================

    async openManageAttributesModal(productTypeId) {
        this.currentManagingProductTypeId = productTypeId;
        document.getElementById('manageAttributesModal').classList.remove('hidden');
        await this.loadExistingAttributesForEditing(productTypeId);
    },

    closeManageAttributesModal() {
        document.getElementById('manageAttributesModal').classList.add('hidden');
        this.currentManagingProductTypeId = null;
        this.currentAttributes = [];

        // Refresh the product type dropdown to reload attributes
        const productTypeSelect = document.getElementById('productType');
        if (productTypeSelect && productTypeSelect.value) {
            productTypeSelect.dispatchEvent(new Event('change'));
        }
    },

    async loadExistingAttributesForEditing(productTypeId) {
        const listContent = document.getElementById('attributesListContent');
        listContent.innerHTML = '<div class="text-center py-8"><p class="text-gray-600">Loading attributes...</p></div>';

        try {
            const response = await fetch(`${this.API_BASE_URL}/api/v1/producttypes/${productTypeId}/attributes-with-values`);
            const result = await response.json();

            this.currentAttributes = [];

            if (result.success && result.data.attributes && result.data.attributes.length > 0) {
                result.data.attributes.forEach(attr => {
                    this.currentAttributes.push({
                        id: attr.id,
                        name: attr.name,
                        displayName: attr.displayName || attr.name,
                        isRequired: attr.isRequired,
                        allowMultipleSelection: attr.allowMultipleSelection,
                        values: attr.values || [],
                        isExisting: true
                    });
                });
            }

            this.renderAttributeFields();
        } catch (error) {
            console.error('Error loading attributes:', error);
            listContent.innerHTML = '<div class="text-center py-8"><p class="text-red-600">Error loading attributes</p></div>';
        }
    },

    renderAttributeFields() {
        const listContent = document.getElementById('attributesListContent');
        listContent.innerHTML = '';

        if (this.currentAttributes.length === 0) {
            listContent.innerHTML = '<div class="text-center py-8"><p class="text-gray-600">No attributes yet. Click "+ Add Attribute" to create one.</p></div>';
            return;
        }

        this.currentAttributes.forEach((attr, index) => {
            const fieldCard = document.getElementById('attributeFieldTemplate').content.cloneNode(true);

            // Set attribute number
            fieldCard.querySelector('.attribute-number').textContent = `Attribute ${index + 1}`;

            // Set attribute name
            const nameInput = fieldCard.querySelector('.attribute-name-input');
            nameInput.value = attr.name || '';
            nameInput.addEventListener('input', (e) => {
                this.currentAttributes[index].name = e.target.value;
            });

            // Set type dropdown
            const typeSelect = fieldCard.querySelector('.attribute-type-select');
            const hasValues = attr.values && attr.values.length > 0;
            typeSelect.value = hasValues ? 'Drop-down' : 'Text field';

            // Get values section elements
            const valuesSection = fieldCard.querySelector('.attribute-values-section');
            const valuesContainer = fieldCard.querySelector('.values-container');
            const toggleBtn = fieldCard.querySelector('.toggle-values-btn');
            const toggleIcon = fieldCard.querySelector('.toggle-icon');
            const addValueBtn = fieldCard.querySelector('.add-value-btn');

            // Show/hide values section based on type
            const updateValuesSection = (type) => {
                if (type === 'Drop-down') {
                    valuesSection.classList.remove('hidden');
                    this.currentAttributes[index].allowMultipleSelection = true;
                } else {
                    valuesSection.classList.add('hidden');
                    valuesContainer.classList.add('hidden');
                    addValueBtn.classList.add('hidden');
                    this.currentAttributes[index].allowMultipleSelection = false;
                }
            };

            // Initial update
            updateValuesSection(typeSelect.value);

            // Handle type change
            typeSelect.addEventListener('change', (e) => {
                updateValuesSection(e.target.value);
            });

            // Toggle values section
            toggleBtn.addEventListener('click', () => {
                const isHidden = valuesContainer.classList.contains('hidden');
                valuesContainer.classList.toggle('hidden');
                addValueBtn.classList.toggle('hidden');
                toggleIcon.classList.toggle('fa-chevron-down');
                toggleIcon.classList.toggle('fa-chevron-up');
            });

            // Add value button
            addValueBtn.addEventListener('click', () => {
                if (!this.currentAttributes[index].values) {
                    this.currentAttributes[index].values = [];
                }
                this.currentAttributes[index].values.push({
                    value: 'New value',
                    displayOrder: this.currentAttributes[index].values.length + 1
                });
                this.renderAttributeValues(index, valuesContainer);
            });

            // Delete attribute button
            const deleteBtn = fieldCard.querySelector('.delete-attribute-btn');
            deleteBtn.addEventListener('click', () => {
                // Remove from local array and re-render UI immediately
                this.currentAttributes.splice(index, 1);
                this.renderAttributeFields();
            });

            // Render initial values
            this.renderAttributeValues(index, valuesContainer);

            listContent.appendChild(fieldCard);
        });
    },

    renderAttributeValues(attrIndex, container) {
        container.innerHTML = '';
        const attr = this.currentAttributes[attrIndex];

        if (!attr.values || attr.values.length === 0) {
            container.innerHTML = '<p class="text-xs text-gray-500">No values yet. Click "Add value" to create one.</p>';
            return;
        }

        attr.values.forEach((val, valIndex) => {
            const valueEl = document.getElementById('attributeValueTemplate').content.cloneNode(true);

            const valueInput = valueEl.querySelector('.value-input');
            valueInput.value = val.value || val;
            valueInput.addEventListener('input', (e) => {
                if (typeof this.currentAttributes[attrIndex].values[valIndex] === 'object') {
                    this.currentAttributes[attrIndex].values[valIndex].value = e.target.value;
                } else {
                    this.currentAttributes[attrIndex].values[valIndex] = e.target.value;
                }
            });

            const deleteBtn = valueEl.querySelector('.delete-value-btn');
            deleteBtn.addEventListener('click', () => {
                this.currentAttributes[attrIndex].values.splice(valIndex, 1);
                this.renderAttributeValues(attrIndex, container);
            });

            container.appendChild(valueEl);
        });
    },

    addNewAttributeField() {
        this.currentAttributes.push({
            name: '',
            displayName: '',
            isRequired: false,
            allowMultipleSelection: true,
            values: [],
            isExisting: false
        });
        this.renderAttributeFields();
    },

    async saveAllAttributes() {
        if (!this.currentManagingProductTypeId) {
            alert('No product type selected');
            return;
        }

        try {
            // Validate attribute names (only if there are attributes)
            for (const attr of this.currentAttributes) {
                if (!attr.name || attr.name.trim() === '') {
                    alert('Please fill in all attribute names');
                    return;
                }
            }

            // Prepare attributes data to send to BE
            const attributesData = this.currentAttributes.map((attr, index) => ({
                id: attr.id || null,
                name: attr.name,
                displayName: attr.displayName || attr.name,
                productTypeId: this.currentManagingProductTypeId,
                isRequired: attr.isRequired || false,
                allowMultipleSelection: attr.allowMultipleSelection || false,
                displayOrder: index + 1,
                values: (attr.values || []).map((val, valIndex) => ({
                    id: (typeof val === 'object' ? val.id : null) || null,
                    value: typeof val === 'object' ? val.value : val,
                    displayOrder: valIndex + 1
                }))
            }));

            // Send all attributes (including empty array) to BE
            const response = await fetch(`${this.API_BASE_URL}/api/v1/producttypes/${this.currentManagingProductTypeId}/attributes`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    attributes: attributesData
                })
            });

            const result = await response.json();
            if (!result.success) {
                alert('Failed to save attributes: ' + (result.message || 'Unknown error'));
                return;
            }

            alert('Attributes saved successfully!');
            this.closeManageAttributesModal();
        } catch (error) {
            console.error('Error saving attributes:', error);
            alert('Error saving attributes. Please try again.');
        }
    }
};
