// ========================================
// Confirm Delete Modal Functions
// ========================================
let itemToDelete = null;

window.openConfirmDeleteModal = function(productId) {
    itemToDelete = productId;
    const modal = document.getElementById('confirmDeleteModal');
    const modalContent = document.getElementById('confirmDeleteModalContent');

    // Show modal
    modal.classList.remove('hidden');
    modal.classList.add('show');

    // Trigger animation
    setTimeout(() => {
        modalContent.style.transform = 'scale(1)';
        modalContent.style.opacity = '1';
    }, 10);

    // Prevent body scroll
    document.body.style.overflow = 'hidden';
};

window.closeConfirmDeleteModal = function() {
    const modal = document.getElementById('confirmDeleteModal');
    const modalContent = document.getElementById('confirmDeleteModalContent');

    // Reset animation
    modalContent.style.transform = 'scale(0.95)';
    modalContent.style.opacity = '0';

    // Hide modal after animation
    setTimeout(() => {
        modal.classList.remove('show');
        modal.classList.add('hidden');
        itemToDelete = null;
    }, 300);

    // Re-enable body scroll
    document.body.style.overflow = '';
};

window.confirmDeleteItem = function() {
    if (itemToDelete) {
        cart.removeItem(itemToDelete);
        renderCartSidebar();
    }
    closeConfirmDeleteModal();
};

// Close modal on Escape key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        const modal = document.getElementById('confirmDeleteModal');
        if (modal && modal.classList.contains('show')) {
            closeConfirmDeleteModal();
        }
    }
});
