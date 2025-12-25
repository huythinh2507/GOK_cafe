class ProductFilterController {
    constructor(filterService, uiController) {
        this.filterService = filterService;
        this.ui = uiController;
        this.searchTimeout = null;

        this.state = {
            page: 1,
            categoryIds: [],
            flavourIds: [],
            equipmentIds: [],
            inStock: null,
            search: ''
        };
    }

    init() {
        this.state = this.filterService.parseUrlParams();
        this.setupEventListeners();
        window.addEventListener('popstate', () => this.handlePopState());
    }

    setupEventListeners() {
        this.setupSearchListeners();
    }

    setupSearchListeners() {
        const desktopInput = document.getElementById('productSearchInput');
        const mobileInput = document.getElementById('mobileProductSearchInput');

        [desktopInput, mobileInput].forEach(input => {
            if (!input) return;

            input.addEventListener('input', (e) => {
                clearTimeout(this.searchTimeout);
                this.searchTimeout = setTimeout(() => {
                    this.state.search = e.target.value.trim();
                    this.state.page = 1;
                    this.applyFilters();
                }, 500);
            });

            input.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    clearTimeout(this.searchTimeout);
                    this.state.search = e.target.value.trim();
                    this.state.page = 1;
                    this.applyFilters();
                }
            });
        });
    }

    async applyFilters() {
        this.ui.showLoading();

        try {
            const response = await this.filterService.applyFilters(this.state);

            if (response && response.success) {
                this.ui.updateProductGrid(response.data);
                this.ui.updatePagination(response.data);
                this.updateBrowserUrl();
                this.ui.scrollToProducts();
            } else if (response) {
                this.ui.showError(response.message || 'Failed to load products');
            }
        } catch (error) {
            console.error('Filter error:', error);
            this.ui.showError('Failed to load products. Please try again.');
        } finally {
            this.ui.hideLoading();
        }
    }

    handleCategoryChange(checkbox) {
        const allCheckboxes = document.querySelectorAll('.category-checkbox');
        const allCategoriesCheckbox = Array.from(allCheckboxes).find(cb => {
            const id = cb.getAttribute('data-category-id');
            return !id || id === 'null' || id === '';
        });

        if (checkbox === allCategoriesCheckbox && checkbox.checked) {
            allCheckboxes.forEach(cb => {
                if (cb !== allCategoriesCheckbox) cb.checked = false;
            });
        } else if (checkbox !== allCategoriesCheckbox) {
            if (allCategoriesCheckbox) allCategoriesCheckbox.checked = false;
        }

        this.state.categoryIds = Array.from(document.querySelectorAll('.category-checkbox:checked'))
            .map(cb => cb.getAttribute('data-category-id'))
            .filter(id => id && id !== 'null' && id !== '');

        if (this.state.categoryIds.length === 0 && allCategoriesCheckbox) {
            allCategoriesCheckbox.checked = true;
        }

        this.state.page = 1;
        this.applyFilters();
    }

    handleFlavourChange() {
        this.state.flavourIds = Array.from(document.querySelectorAll('.flavour-checkbox:checked'))
            .map(cb => cb.getAttribute('data-flavour-id'));
        this.state.page = 1;
        this.applyFilters();
    }

    handleEquipmentChange() {
        this.state.equipmentIds = Array.from(document.querySelectorAll('.equipment-checkbox:checked'))
            .map(cb => cb.getAttribute('data-equipment-id'));
        this.state.page = 1;
        this.applyFilters();
    }

    handleAvailabilityChange(value) {
        if (value === 'null' || value === null) {
            this.state.inStock = null;
        } else if (value === 'true' || value === true) {
            this.state.inStock = true;
        } else if (value === 'false' || value === false) {
            this.state.inStock = false;
        }
        this.state.page = 1;
        this.applyFilters();
    }

    changePage(pageNumber) {
        this.state.page = pageNumber;
        this.applyFilters();
    }

    handleMobileCategoryChange(checkbox) {
        const allCheckboxes = document.querySelectorAll('.mobile-category-checkbox');
        const allCategoriesCheckbox = Array.from(allCheckboxes).find(cb => {
            const id = cb.getAttribute('data-category-id');
            return !id || id === 'null' || id === '';
        });

        if (checkbox === allCategoriesCheckbox && checkbox.checked) {
            allCheckboxes.forEach(cb => {
                if (cb !== allCategoriesCheckbox) cb.checked = false;
            });
        } else if (checkbox !== allCategoriesCheckbox) {
            if (allCategoriesCheckbox) allCategoriesCheckbox.checked = false;
        }

        const checkedCategories = Array.from(document.querySelectorAll('.mobile-category-checkbox:checked'))
            .map(cb => cb.getAttribute('data-category-id'))
            .filter(id => id && id !== 'null' && id !== '');

        if (checkedCategories.length === 0 && allCategoriesCheckbox) {
            allCategoriesCheckbox.checked = true;
        }
    }

    applyMobileFilters() {
        this.state.categoryIds = Array.from(document.querySelectorAll('.mobile-category-checkbox:checked'))
            .map(cb => cb.getAttribute('data-category-id'))
            .filter(id => id && id !== 'null' && id !== '');

        this.state.flavourIds = Array.from(document.querySelectorAll('.mobile-flavour-checkbox:checked'))
            .map(cb => cb.getAttribute('data-flavour-id'));

        this.state.equipmentIds = Array.from(document.querySelectorAll('.mobile-equipment-checkbox:checked'))
            .map(cb => cb.getAttribute('data-equipment-id'));

        const selectedAvailability = document.querySelector('.mobile-availability-radio:checked');
        if (selectedAvailability) {
            const value = selectedAvailability.getAttribute('data-value');
            this.state.inStock = value === 'true' ? true : value === 'false' ? false : null;
        }

        this.state.page = 1;
        this.applyFilters();
        closeMobileFilter();
    }

    clearMobileFilters() {
        document.querySelectorAll('.mobile-category-checkbox').forEach(cb => cb.checked = false);
        document.querySelectorAll('.mobile-flavour-checkbox').forEach(cb => cb.checked = false);
        document.querySelectorAll('.mobile-equipment-checkbox').forEach(cb => cb.checked = false);

        const allProductsRadio = document.querySelector('.mobile-availability-radio[data-value=""]');
        if (allProductsRadio) allProductsRadio.checked = true;

        this.state = {
            page: 1,
            categoryIds: [],
            flavourIds: [],
            equipmentIds: [],
            inStock: null,
            search: this.state.search
        };

        this.ui.syncFiltersWithState(this.state);
        this.applyFilters();
        closeMobileFilter();
    }

    updateBrowserUrl() {
        const params = this.filterService.buildUrlParams(this.state);
        const url = new URL(window.location.href);
        url.search = params.toString();
        window.history.pushState(this.state, '', url.toString());
    }

    handlePopState() {
        this.state = this.filterService.parseUrlParams();
        this.ui.syncFiltersWithState(this.state);
        this.applyFilters();
    }
}

window.ProductFilterController = ProductFilterController;
