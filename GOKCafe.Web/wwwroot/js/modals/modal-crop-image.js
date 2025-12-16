// Modal Crop Image JavaScript
// Using Cropper.js library for image cropping functionality

let cropper = null;
let currentImageFile = null;
let currentImageDataUrl = null;

// Initialize crop modal
function initCropModal() {
    console.log('Crop modal initialized');
}

// Open crop modal with image
function openCropModal(file, dataUrl) {
    currentImageFile = file;
    currentImageDataUrl = dataUrl;

    const modal = document.getElementById('cropImageModal');
    const imagePreview = document.getElementById('cropImagePreview');

    if (!modal || !imagePreview) {
        console.error('Crop modal elements not found');
        return;
    }

    // Destroy existing cropper instance first
    if (cropper) {
        cropper.destroy();
        cropper = null;
    }

    // Clear the image source first to ensure onload triggers
    imagePreview.src = '';
    imagePreview.removeAttribute('src');

    // Hide modal initially (will show after image loads)
    modal.classList.add('hidden');

    // Initialize Cropper.js after image loads
    imagePreview.onload = function() {
        console.log('Image loaded, initializing cropper');

        // Double check and destroy any existing cropper
        if (cropper) {
            cropper.destroy();
        }

        cropper = new Cropper(imagePreview, {
            viewMode: 0, // Allow crop box to extend beyond canvas
            dragMode: 'move',
            aspectRatio: 5/7, // 5:7 aspect ratio by default
            autoCropArea: 0.95, // 95% crop area
            restore: false,
            guides: true,
            center: true,
            highlight: false,
            cropBoxMovable: true,
            cropBoxResizable: true,
            toggleDragModeOnDblclick: false,
            responsive: true,
            background: false, // Disable checkered background
            zoomOnWheel: true,
            zoomOnTouch: true,
            minContainerWidth: 400,
            minContainerHeight: 400,
            ready: function() {
                console.log('Cropper ready');
                // Set default button as active
                setDefaultAspectRatioButton();

                // Show modal only after cropper is fully ready
                modal.classList.remove('hidden');
                document.body.style.overflow = 'hidden'; // Prevent body scroll

                // Fade in animation
                setTimeout(function() {
                    modal.classList.remove('opacity-0');
                    modal.classList.add('opacity-100');
                }, 10);
            }
        });
    };

    // Handle image load error
    imagePreview.onerror = function() {
        console.error('Failed to load image');
        alert('Failed to load image. Please try again.');
        closeCropModal();
    };

    // Set image source AFTER setting onload handler
    imagePreview.src = dataUrl;
}

// Set default aspect ratio button as active
function setDefaultAspectRatioButton() {
    // Ensure the default button (5:7) is highlighted
    const buttons = document.querySelectorAll('.aspect-ratio-btn');
    buttons.forEach(btn => {
        if (btn.classList.contains('aspect-ratio-default')) {
            btn.classList.remove('border', 'border-gray-300', 'text-gray-700');
            btn.classList.add('border-2', 'border-primary', 'bg-primary', 'text-white');
        } else {
            btn.classList.remove('border-2', 'border-primary', 'bg-primary', 'text-white');
            btn.classList.add('border', 'border-gray-300', 'text-gray-700');
        }
    });
}

// Close crop modal
function closeCropModal() {
    const modal = document.getElementById('cropImageModal');

    if (modal) {
        // Fade out animation
        modal.classList.remove('opacity-100');
        modal.classList.add('opacity-0');

        // Wait for animation to complete before hiding
        setTimeout(function() {
            modal.classList.add('hidden');
            document.body.style.overflow = ''; // Restore body scroll

            // Destroy cropper instance
            if (cropper) {
                cropper.destroy();
                cropper = null;
            }

            // Clear image preview
            const imagePreview = document.getElementById('cropImagePreview');
            if (imagePreview) {
                imagePreview.src = '';
                imagePreview.onload = null;
                imagePreview.onerror = null;
            }

            // Reset zoom range
            const zoomRange = document.getElementById('zoomRange');
            if (zoomRange) {
                zoomRange.value = 0;
            }

            // Reset file input to allow re-upload of same file
            const fileInput = document.getElementById('imageFileInput');
            if (fileInput) {
                fileInput.value = '';
            }

            // Clear current image data
            currentImageFile = null;
            currentImageDataUrl = null;
        }, 300); // Match transition duration
    }
}

// Set aspect ratio
function setCropAspectRatio(ratio, evt) {
    if (cropper) {
        cropper.setAspectRatio(ratio);

        // Update button styles - reset all buttons first
        const buttons = document.querySelectorAll('.aspect-ratio-btn');
        buttons.forEach(btn => {
            btn.classList.remove('border-2', 'border-primary', 'bg-primary', 'text-white');
            btn.classList.add('border', 'border-gray-300', 'text-gray-700');
        });

        // Highlight active button
        if (evt && evt.target) {
            evt.target.classList.remove('border', 'border-gray-300', 'text-gray-700');
            evt.target.classList.add('border-2', 'border-primary');

            if (ratio === null) {
                // Free ratio - just border color
                evt.target.classList.add('text-primary');
            } else {
                // Fixed ratio (5:7) - filled button
                evt.target.classList.add('bg-primary', 'text-white');
            }
        }
    }
}

// Zoom controls
function zoomCrop(ratio) {
    if (cropper) {
        cropper.zoom(ratio);
        updateZoomRange();
    }
}

function zoomCropTo(value) {
    if (cropper) {
        cropper.zoomTo(parseFloat(value));
    }
}

function updateZoomRange() {
    const zoomRange = document.getElementById('zoomRange');
    if (cropper && zoomRange) {
        const containerData = cropper.getContainerData();
        const imageData = cropper.getImageData();
        const ratio = Math.min(
            containerData.width / imageData.naturalWidth,
            containerData.height / imageData.naturalHeight
        );
        zoomRange.value = imageData.width / imageData.naturalWidth / ratio;
    }
}

// Rotate image
function rotateCrop(degree) {
    if (cropper) {
        cropper.rotate(degree);
    }
}

// Flip horizontal
function flipCropHorizontal() {
    if (cropper) {
        const imageData = cropper.getImageData();
        cropper.scaleX(imageData.scaleX === -1 ? 1 : -1);
    }
}

// Flip vertical
function flipCropVertical() {
    if (cropper) {
        const imageData = cropper.getImageData();
        cropper.scaleY(imageData.scaleY === -1 ? 1 : -1);
    }
}

// Reset crop
function resetCrop() {
    if (cropper) {
        cropper.reset();
        const zoomRange = document.getElementById('zoomRange');
        if (zoomRange) {
            zoomRange.value = 0;
        }
    }
}

// Save cropped image
function saveCroppedImage() {
    if (!cropper) {
        console.error('Cropper not initialized');
        return;
    }

    // Get cropped canvas
    const canvas = cropper.getCroppedCanvas({
        maxWidth: 4096,
        maxHeight: 4096,
        fillColor: '#fff',
        imageSmoothingEnabled: true,
        imageSmoothingQuality: 'high'
    });

    if (!canvas) {
        console.error('Failed to get cropped canvas');
        return;
    }

    // Convert canvas to blob
    canvas.toBlob(function(blob) {
        if (!blob) {
            console.error('Failed to create blob from canvas');
            return;
        }

        // Create a new File object from blob
        const fileName = currentImageFile ? currentImageFile.name : 'cropped-image.jpg';
        const croppedFile = new File([blob], fileName, {
            type: 'image/jpeg',
            lastModified: Date.now()
        });

        // Convert blob to data URL for preview
        const reader = new FileReader();
        reader.onload = function(e) {
            const croppedDataUrl = e.target.result;

            // Process the cropped image (add to upload list)
            if (typeof processFile === 'function') {
                // Create a pseudo file input event
                const pseudoFile = croppedFile;
                pseudoFile.dataUrl = croppedDataUrl; // Attach data URL to file

                processFile(pseudoFile);
            } else {
                console.error('processFile function not found');
            }

            // Close modal
            closeCropModal();
        };
        reader.readAsDataURL(blob);

    }, 'image/jpeg', 0.95); // 95% quality
}

// Handle keyboard shortcuts
document.addEventListener('keydown', function(e) {
    const modal = document.getElementById('cropImageModal');
    if (!modal || modal.classList.contains('hidden')) {
        return;
    }

    // ESC to close
    if (e.key === 'Escape') {
        closeCropModal();
    }

    // Enter to save
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
        saveCroppedImage();
    }
});

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initCropModal();
});
