class ProductGridUIController {
    constructor() {
        this.productGridSelector = '.grid.grid-cols-1.sm\\:grid-cols-2';
        this.paginationSelector = '#product-pagination';
    }

    showLoading() {
        const container = document.querySelector(this.productGridSelector);
        if (!container) return;

        this.disableFilters(true);
        container.innerHTML = `
            <div class="col-span-full flex items-center justify-center min-h-[300px] w-full">
                <div class="flex flex-col items-center justify-center text-center">
                    <i class="fas fa-spinner fa-spin text-4xl text-primary mb-3"></i>
                    <p class="text-gray-600">Loading products...</p>
                </div>
            </div>
        `;
    }

    hideLoading() {
        this.disableFilters(false);
    }

    updateProductGrid(data) {
        const container = document.querySelector(this.productGridSelector);
        if (!container) return;

        if (!data.items || data.items.length === 0) {
            this.showEmptyState(container);
            return;
        }

        container.innerHTML = data.items.map(product => this.buildProductCard(product)).join('');
    }

    showEmptyState(container) {
        container.innerHTML = `
            <div class="col-span-1 sm:col-span-2 lg:col-span-3 2xl:col-span-4 flex items-center justify-center py-12">
                <div class="text-center">
                    <i class="fas fa-coffee text-6xl text-gray-300 mb-4"></i>
                    <p class="text-gray-500 text-lg">No products found.</p>
                </div>
            </div>
        `;
    }

    showError(message) {
        const container = document.querySelector(this.productGridSelector);
        if (!container) return;

        container.innerHTML = `
            <div class="col-span-full text-center py-12">
                <i class="fas fa-exclamation-triangle text-6xl text-red-300 mb-4"></i>
                <p class="text-red-600 text-lg mb-2">Error Loading Products</p>
                <p class="text-gray-500 text-sm">${message}</p>
                <button onclick="productFilterController.applyFilters()" class="mt-4 px-6 py-2 bg-primary text-white rounded hover:bg-primary-dark transition-colors">
                    Try Again
                </button>
            </div>
        `;
    }

    buildProductCard(product) {
        const productUrl = `/product-details?id=${product.id}`;
        const imageHtml = product.imageUrl
            ? `<img src="${product.imageUrl}" alt="${product.name}" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />`
            : `<div class="w-full h-full bg-[#E8DCC4] flex items-center justify-center">
                <i class="fas fa-coffee text-6xl text-gray-400"></i>
            </div>`;

        const priceHtml = this.buildPriceHtml(product);

        return `
            <div class="group">
                <a href="${productUrl}" class="block overflow-hidden mb-4 flex items-center justify-center bg-[#E8DCC4] w-full h-[420px] relative cursor-pointer">
                    ${imageHtml}
                </a>
                <div class="text-center w-full mb-4">
                    <p class="text-sm font-medium text-gray-500 uppercase mb-1">${product.categoryName || ''}</p>
                    <h3 class="text-lg font-bold text-gray-900 mb-2 uppercase truncate">
                        <a href="${productUrl}" class="hover:text-primary transition-colors">${product.name}</a>
                    </h3>
                    <div class="text-gray-600 mb-2 flex items-center justify-center gap-2">
                        ${priceHtml}
                    </div>
                </div>
                <button onclick="openProductModal('${product.id}')" class="block w-full px-4 py-3 border-2 border-primary text-primary font-semibold hover:bg-primary hover:text-white transition-all duration-300 cursor-pointer flex items-center justify-center gap-2">
                    <i class="fas fa-shopping-cart"></i>
                    <span>Buy Now</span>
                </button>
            </div>
        `;
    }

    buildPriceHtml(product) {
        const price = product.price || 0;
        const discountPrice = product.discountPrice;

        if (discountPrice && discountPrice > 0 && discountPrice < price) {
            const discountPercent = Math.round(((price - discountPrice) / price) * 100);
            const priceDisplay = price % 1 === 0 ? Math.floor(price) : price;
            const discountPriceDisplay = discountPrice % 1 === 0 ? Math.floor(discountPrice) : discountPrice;

            return `
                <div>From</div>
                <span class="text-gray-400 line-through text-base">${priceDisplay}</span>
                <span class="font-semibold text-primary text-lg">${discountPriceDisplay}</span>
                <span class="bg-primary text-white px-3 py-1 rounded-xl text-sm font-bold">-${discountPercent}%</span>
            `;
        }

        const priceDisplay = price % 1 === 0 ? Math.floor(price) : price;
        return `<span>From <span class="font-semibold text-lg">${priceDisplay}</span></span>`;
    }

    updatePagination(data) {
        const container = document.getElementById('product-pagination');
        if (!container) return;

        if (data.totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        const { pageNumber: currentPage, totalPages } = data;
        let html = '';

        html += this.buildPrevButton(currentPage);
        html += this.buildPageNumbers(currentPage, totalPages);
        html += this.buildNextButton(currentPage, totalPages);

        container.innerHTML = html;
    }

    buildPrevButton(currentPage) {
        if (currentPage > 1) {
            return `
                <button onclick="productFilterController.changePage(${currentPage - 1})" class="w-12 h-12 flex items-center justify-center border border-gray-300 text-gray-600 hover:bg-gray-50 transition-colors rounded cursor-pointer">
                    <i class="fas fa-chevron-left"></i>
                </button>
            `;
        }
        return `
            <span class="w-12 h-12 flex items-center justify-center border border-gray-300 text-gray-300 rounded cursor-not-allowed">
                <i class="fas fa-chevron-left"></i>
            </span>
        `;
    }

    buildPageNumbers(currentPage, totalPages) {
        let html = '';

        for (let i = 1; i <= Math.min(3, totalPages); i++) {
            html += this.buildPageButton(i, currentPage);
        }

        if (totalPages > 4) {
            html += `<span class="px-2 text-gray-500">...</span>`;
            html += this.buildPageButton(totalPages, currentPage);
        }

        return html;
    }

    buildPageButton(pageNum, currentPage) {
        if (pageNum === currentPage) {
            return `
                <span class="w-12 h-12 flex items-center justify-center border-2 border-primary bg-primary text-white font-medium transition-colors rounded">
                    ${pageNum}
                </span>
            `;
        }
        return `
            <button onclick="productFilterController.changePage(${pageNum})" class="w-12 h-12 flex items-center justify-center border border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors rounded cursor-pointer">
                ${pageNum}
            </button>
        `;
    }

    buildNextButton(currentPage, totalPages) {
        if (currentPage < totalPages) {
            return `
                <button onclick="productFilterController.changePage(${currentPage + 1})" class="w-12 h-12 flex items-center justify-center border border-gray-300 text-gray-600 hover:bg-gray-50 transition-colors rounded cursor-pointer">
                    <i class="fas fa-chevron-right"></i>
                </button>
            `;
        }
        return `
            <span class="w-12 h-12 flex items-center justify-center border border-gray-300 text-gray-300 rounded cursor-not-allowed">
                <i class="fas fa-chevron-right"></i>
            </span>
        `;
    }

    disableFilters(disabled) {
        const selectors = [
            '.category-checkbox',
            '.flavour-checkbox',
            '.equipment-checkbox',
            'input[name="availability"]',
            '.mobile-category-checkbox',
            '.mobile-flavour-checkbox',
            '.mobile-equipment-checkbox',
            'input[name="mobile-availability"]'
        ];

        document.querySelectorAll(selectors.join(', ')).forEach(el => {
            el.disabled = disabled;
        });

        const searchInputs = [
            document.getElementById('productSearchInput'),
            document.getElementById('mobileProductSearchInput')
        ];

        searchInputs.forEach(input => {
            if (input) input.disabled = disabled;
        });
    }

    scrollToProducts() {
        const productsSection = document.querySelector('.lg\\:min-w-80');
        if (productsSection) {
            const offset = 100;
            const top = productsSection.getBoundingClientRect().top + window.pageYOffset - offset;
            window.scrollTo({ top, behavior: 'smooth' });
        }
    }

    syncFiltersWithState(filterState) {
        this.syncCheckboxes('.category-checkbox', 'data-category-id', filterState.categoryIds);
        this.syncCheckboxes('.flavour-checkbox', 'data-flavour-id', filterState.flavourIds);
        this.syncCheckboxes('.equipment-checkbox', 'data-equipment-id', filterState.equipmentIds);
        this.syncCheckboxes('.mobile-category-checkbox', 'data-category-id', filterState.categoryIds);
        this.syncCheckboxes('.mobile-flavour-checkbox', 'data-flavour-id', filterState.flavourIds);
        this.syncCheckboxes('.mobile-equipment-checkbox', 'data-equipment-id', filterState.equipmentIds);

        this.syncAvailabilityRadios(filterState.inStock);
        this.syncSearchInputs(filterState.search);
    }

    syncCheckboxes(selector, attribute, selectedIds) {
        document.querySelectorAll(selector).forEach(cb => {
            const id = cb.getAttribute(attribute);
            cb.checked = selectedIds.includes(id);
        });
    }

    syncAvailabilityRadios(inStock) {
        document.querySelectorAll('input[name="availability"]').forEach(radio => {
            const onchangeAttr = radio.getAttribute('onchange');
            if (onchangeAttr) {
                if (onchangeAttr.includes('null') && inStock === null) radio.checked = true;
                else if (onchangeAttr.includes('true') && inStock === true) radio.checked = true;
                else if (onchangeAttr.includes('false') && inStock === false) radio.checked = true;
            }
        });

        document.querySelectorAll('.mobile-availability-radio').forEach(radio => {
            const value = radio.getAttribute('data-value');
            if (value === '' && inStock === null) radio.checked = true;
            else if (value === 'true' && inStock === true) radio.checked = true;
            else if (value === 'false' && inStock === false) radio.checked = true;
        });
    }

    syncSearchInputs(search) {
        const desktopInput = document.getElementById('productSearchInput');
        const mobileInput = document.getElementById('mobileProductSearchInput');
        if (desktopInput) desktopInput.value = search;
        if (mobileInput) mobileInput.value = search;
    }
}

window.ProductGridUIController = ProductGridUIController;
