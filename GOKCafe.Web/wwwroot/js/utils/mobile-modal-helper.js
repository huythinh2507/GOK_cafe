function toggleFilter(filterId) {
    const filterDiv = document.getElementById(filterId + '-filter');
    const icon = document.getElementById(filterId + '-icon');

    if (filterDiv.style.display === 'none') {
        filterDiv.style.display = 'block';
        icon.classList.remove('fa-chevron-down');
        icon.classList.add('fa-chevron-up');
    } else {
        filterDiv.style.display = 'none';
        icon.classList.remove('fa-chevron-up');
        icon.classList.add('fa-chevron-down');
    }
}

function toggleMobileSearch() {
    const mobileSearchBar = document.getElementById('mobileSearchBar');
    const mobileSearchInput = document.getElementById('mobileProductSearchInput');

    if (mobileSearchBar.classList.contains('hidden')) {
        mobileSearchBar.classList.remove('hidden');
        mobileSearchInput.focus();
    } else {
        mobileSearchBar.classList.add('hidden');
    }
}

function toggleMobileFilter() {
    const modal = document.getElementById('mobileFilterModal');
    modal.classList.remove('hidden');
    document.body.style.position = 'fixed';
    document.body.style.width = '100%';
    document.body.style.top = `-${window.scrollY}px`;
}

function closeMobileFilter() {
    const modal = document.getElementById('mobileFilterModal');
    modal.classList.add('hidden');
    const scrollY = document.body.style.top;
    document.body.style.position = '';
    document.body.style.width = '';
    document.body.style.top = '';
    window.scrollTo(0, parseInt(scrollY || '0') * -1);
}

function toggleMobileFilterSection(filterId) {
    const filterDiv = document.getElementById('mobile-' + filterId + '-filter');
    const icon = document.getElementById('mobile-' + filterId + '-icon');

    if (filterDiv.style.display === 'none') {
        filterDiv.style.display = 'block';
        icon.classList.remove('fa-chevron-down');
        icon.classList.add('fa-chevron-up');
    } else {
        filterDiv.style.display = 'none';
        icon.classList.remove('fa-chevron-up');
        icon.classList.add('fa-chevron-down');
    }
}

function applyMobileFilters() {
    if (window.productFilterController) {
        window.productFilterController.applyMobileFilters();
    }
}

function clearMobileFilters() {
    if (window.productFilterController) {
        window.productFilterController.clearMobileFilters();
    }
}

document.addEventListener('DOMContentLoaded', function() {
    const modal = document.getElementById('mobileFilterModal');
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                closeMobileFilter();
            }
        });
    }
});
