// ========================================
// GOK Cafe Admin JavaScript
// ========================================

// Toggle Notifications Dropdown
window.toggleNotifications = function() {
    const dropdown = document.getElementById('notificationsDropdown');
    const userMenu = document.getElementById('userMenuDropdown');

    // Close user menu if open
    if (userMenu && !userMenu.classList.contains('hidden')) {
        userMenu.classList.add('hidden');
    }

    // Toggle notifications
    dropdown.classList.toggle('hidden');
};

// Toggle User Menu Dropdown
window.toggleUserMenu = function() {
    const dropdown = document.getElementById('userMenuDropdown');
    const notificationsMenu = document.getElementById('notificationsDropdown');

    // Close notifications if open
    if (notificationsMenu && !notificationsMenu.classList.contains('hidden')) {
        notificationsMenu.classList.add('hidden');
    }

    // Toggle user menu
    dropdown.classList.toggle('hidden');
};

// Close dropdowns when clicking outside
document.addEventListener('click', function(event) {
    const notificationsBtn = event.target.closest('[onclick="toggleNotifications()"]');
    const userMenuBtn = event.target.closest('[onclick="toggleUserMenu()"]');
    const notificationsDropdown = document.getElementById('notificationsDropdown');
    const userMenuDropdown = document.getElementById('userMenuDropdown');

    if (!notificationsBtn && notificationsDropdown) {
        notificationsDropdown.classList.add('hidden');
    }

    if (!userMenuBtn && userMenuDropdown) {
        userMenuDropdown.classList.add('hidden');
    }
});

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log('GOK Cafe Admin Panel Initialized');

    // Add active class to current sidebar link
    highlightActiveSidebarLink();
});

// Highlight Active Sidebar Link
function highlightActiveSidebarLink() {
    const currentPath = window.location.pathname;
    const sidebarLinks = document.querySelectorAll('.sidebar-link');

    sidebarLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href === currentPath || (currentPath.startsWith(href) && href !== '/admin/dashboard')) {
            link.classList.add('bg-primary', 'text-white');
            link.classList.remove('text-gray-300');
        } else {
            link.classList.remove('bg-primary', 'text-white');
            link.classList.add('text-gray-300');
        }
    });
}

// Confirmation Dialog
window.confirmDelete = function(message) {
    return confirm(message || 'Are you sure you want to delete this item?');
};

// Show Success Toast
window.showSuccessToast = function(message) {
    showToast(message, 'success');
};

// Show Error Toast
window.showErrorToast = function(message) {
    showToast(message, 'error');
};

// Generic Toast Function
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `fixed top-4 right-4 z-50 px-6 py-4 rounded-lg shadow-lg text-white transition-all transform translate-x-full`;

    // Set color based on type
    if (type === 'success') {
        toast.classList.add('bg-green-500');
    } else if (type === 'error') {
        toast.classList.add('bg-red-500');
    } else {
        toast.classList.add('bg-blue-500');
    }

    // Set icon
    let icon = 'fa-info-circle';
    if (type === 'success') icon = 'fa-check-circle';
    if (type === 'error') icon = 'fa-exclamation-circle';

    toast.innerHTML = `
        <div class="flex items-center space-x-3">
            <i class="fas ${icon} text-xl"></i>
            <span>${message}</span>
        </div>
    `;

    document.body.appendChild(toast);

    // Slide in
    setTimeout(() => {
        toast.classList.remove('translate-x-full');
    }, 10);

    // Remove after 3 seconds
    setTimeout(() => {
        toast.classList.add('translate-x-full');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Table Row Selection
window.toggleRowSelection = function(checkbox) {
    const row = checkbox.closest('tr');
    if (checkbox.checked) {
        row.classList.add('bg-blue-50');
    } else {
        row.classList.remove('bg-blue-50');
    }
};

// Select All Checkboxes
window.toggleSelectAll = function(checkbox) {
    const checkboxes = document.querySelectorAll('.row-checkbox');
    checkboxes.forEach(cb => {
        cb.checked = checkbox.checked;
        toggleRowSelection(cb);
    });
};
