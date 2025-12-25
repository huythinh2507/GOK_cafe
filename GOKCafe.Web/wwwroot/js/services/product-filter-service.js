class ProductFilterService {
    constructor(apiService) {
        this.apiService = apiService;
        this.abortController = null;
        this.isLoading = false;
    }

    async applyFilters(filterState) {
        if (this.isLoading) {
            this.abortController?.abort();
        }

        this.abortController = new AbortController();
        this.isLoading = true;

        try {
            const params = this.buildApiParams(filterState);
            const response = await this.apiService.getProducts(params);
            this.isLoading = false;
            return response;
        } catch (error) {
            this.isLoading = false;
            if (error.name !== 'AbortError') {
                throw error;
            }
            return null;
        }
    }

    buildApiParams(filterState) {
        const params = {
            pageNumber: filterState.page,
            pageSize: 12
        };

        if (filterState.categoryIds.length > 0) {
            params.categoryIds = filterState.categoryIds;
        }

        if (filterState.flavourIds.length > 0) {
            params.flavourProfileIds = filterState.flavourIds;
        }

        if (filterState.equipmentIds.length > 0) {
            params.equipmentIds = filterState.equipmentIds;
        }

        if (filterState.inStock !== null) {
            params.inStock = filterState.inStock;
        }

        if (filterState.search) {
            params.search = filterState.search;
        }

        return params;
    }

    buildUrlParams(filterState) {
        const params = new URLSearchParams();

        if (filterState.page > 1) {
            params.set('page', filterState.page);
        }

        filterState.categoryIds.forEach(id => params.append('category', id));
        filterState.flavourIds.forEach(id => params.append('flavour', id));
        filterState.equipmentIds.forEach(id => params.append('equipment', id));

        if (filterState.inStock !== null) {
            params.set('inStock', filterState.inStock);
        }

        if (filterState.search) {
            params.set('search', filterState.search);
        }

        return params;
    }

    parseUrlParams() {
        const urlParams = new URLSearchParams(window.location.search);

        return {
            page: parseInt(urlParams.get('page')) || 1,
            categoryIds: urlParams.getAll('category').filter(id => id),
            flavourIds: urlParams.getAll('flavour').filter(id => id),
            equipmentIds: urlParams.getAll('equipment').filter(id => id),
            search: urlParams.get('search') || '',
            inStock: this.parseInStockParam(urlParams.get('inStock'))
        };
    }

    parseInStockParam(value) {
        if (value === 'true') return true;
        if (value === 'false') return false;
        return null;
    }
}

window.ProductFilterService = ProductFilterService;
