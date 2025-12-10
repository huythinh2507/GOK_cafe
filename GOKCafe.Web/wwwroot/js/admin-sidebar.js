// ========================================
// Admin Sidebar Controls
// ========================================

// Mobile Sidebar Toggle
document.addEventListener('DOMContentLoaded', function() {
    const mobileSidebarToggle = document.getElementById('mobileSidebarToggle');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('adminSidebar');
    const overlay = document.getElementById('sidebarOverlay');

    // Mobile menu toggle
    if (mobileSidebarToggle) {
        mobileSidebarToggle.addEventListener('click', function() {
            openSidebar();
        });
    }

    // Close button in sidebar
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            closeSidebar();
        });
    }

    // Click overlay to close
    if (overlay) {
        overlay.addEventListener('click', function() {
            closeSidebar();
        });
    }

    // Desktop sidebar collapse toggle (optional feature)
    initDesktopSidebarCollapse();
});

// Open Sidebar (Mobile)
function openSidebar() {
    const sidebar = document.getElementById('adminSidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (sidebar) {
        sidebar.classList.add('open');
    }

    if (overlay) {
        overlay.classList.remove('hidden');
        overlay.classList.add('show');
    }

    // Prevent body scroll
    document.body.style.overflow = 'hidden';
}

// Close Sidebar (Mobile)
function closeSidebar() {
    const sidebar = document.getElementById('adminSidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (sidebar) {
        sidebar.classList.remove('open');
    }

    if (overlay) {
        overlay.classList.remove('show');
        overlay.classList.add('hidden');
    }

    // Re-enable body scroll
    document.body.style.overflow = '';
}

// Desktop Sidebar Collapse (Optional)
function initDesktopSidebarCollapse() {
    // You can add a toggle button to collapse sidebar on desktop
    // This will reduce sidebar to icon-only mode

    const collapseBtn = document.getElementById('desktopSidebarCollapseBtn');
    if (!collapseBtn) return;

    collapseBtn.addEventListener('click', function() {
        const sidebar = document.getElementById('adminSidebar');
        sidebar.classList.toggle('collapsed');

        // Save state to localStorage
        const isCollapsed = sidebar.classList.contains('collapsed');
        localStorage.setItem('admin_sidebar_collapsed', isCollapsed);
    });

    // Restore state from localStorage
    const isCollapsed = localStorage.getItem('admin_sidebar_collapsed') === 'true';
    if (isCollapsed) {
        document.getElementById('adminSidebar').classList.add('collapsed');
    }
}

// Close sidebar when ESC key is pressed
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        closeSidebar();
    }
});

// Close sidebar on window resize (if mobile to desktop)
window.addEventListener('resize', function() {
    if (window.innerWidth >= 1024) {
        closeSidebar();
    }
});
