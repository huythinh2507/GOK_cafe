/**
 * Product Image Upload Module
 * Handles all image upload, crop, preview, and management functionality
 */

export const ProductImageUpload = {
    // Configuration
    API_BASE_URL: null,

    // State
    uploadedImages: [],

    /**
     * Initialize the module
     * @param {string} apiBaseUrl - Base URL for API calls
     */
    init(apiBaseUrl) {
        this.API_BASE_URL = apiBaseUrl;
    },

    // ========================================
    // File Selection & Validation
    // ========================================

    handleFileSelect(event) {
        const files = event.target.files;
        if (files.length > 0) {
            Array.from(files).forEach(file => this.openCropModalForFile(file));
        }
    },

    openCropModalForFile(file) {
        // Validate file type
        if (!file.type.startsWith('image/')) {
            alert('Please upload an image file');
            return;
        }

        // Validate file size (15MB max)
        const maxSize = 15 * 1024 * 1024;
        if (file.size > maxSize) {
            alert('File size must be less than 15MB');
            return;
        }

        // Read file and open crop modal
        const reader = new FileReader();
        reader.onload = (e) => {
            // Call global openCropModal function (defined in modal-crop-image.js)
            if (typeof window.openCropModal === 'function') {
                window.openCropModal(file, e.target.result);
            }
        };
        reader.readAsDataURL(file);
    },

    async processFile(file) {
        // Check if file has attached dataUrl (from crop modal)
        let dataUrl = file.dataUrl;

        if (!dataUrl) {
            // If no dataUrl attached, read the file (fallback for direct upload)
            const reader = new FileReader();
            reader.onload = (e) => {
                this.uploadImageToAPI(file, e.target.result);
            };
            reader.readAsDataURL(file);
        } else {
            // Use the provided dataUrl (from crop modal)
            this.uploadImageToAPI(file, dataUrl);
        }
    },

    // ========================================
    // Drag & Drop
    // ========================================

    handleDragOver(event) {
        event.preventDefault();
        event.stopPropagation();
        event.currentTarget.classList.add('bg-gray-100', 'border-primary-dark');
    },

    handleDragLeave(event) {
        event.preventDefault();
        event.stopPropagation();
        event.currentTarget.classList.remove('bg-gray-100', 'border-primary-dark');
    },

    handleDrop(event) {
        event.preventDefault();
        event.stopPropagation();
        event.currentTarget.classList.remove('bg-gray-100', 'border-primary-dark');

        const files = event.dataTransfer.files;
        if (files.length > 0) {
            Array.from(files).forEach(file => this.openCropModalForFile(file));
        }
    },

    // ========================================
    // Upload & API Integration
    // ========================================

    async uploadImageToAPI(file, dataUrl) {
        // Show uploading status
        const tempId = Date.now() + Math.random();
        const tempImageData = {
            id: tempId,
            name: file.name,
            size: file.size,
            dataUrl: dataUrl,
            isDefault: this.uploadedImages.length === 0,
            uploading: true,
            uploadProgress: 0
        };

        this.uploadedImages.push(tempImageData);
        this.addImageToList(tempImageData);

        try {
            // Create FormData and append the file
            const formData = new FormData();
            formData.append('file', file);

            // Upload to GOKCafe API
            const response = await fetch(`${this.API_BASE_URL}/api/v1/media/upload`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error(`Upload failed with status ${response.status}`);
            }

            const result = await response.json();

            // Check if upload was successful
            if (!result.success) {
                throw new Error(result.message || 'Upload failed');
            }

            // Update the image data with the API response
            const imageIndex = this.uploadedImages.findIndex(img => img.id === tempId);
            if (imageIndex !== -1) {
                this.uploadedImages[imageIndex] = {
                    id: tempId,
                    name: result.data.fileName || file.name,
                    size: file.size,
                    dataUrl: dataUrl,
                    uploadedUrl: result.data.fileUrl,
                    fileType: result.data.fileType,
                    fileSize: result.data.fileSize,
                    isDefault: this.uploadedImages[imageIndex].isDefault,
                    uploading: false,
                    uploadSuccess: true
                };

                // Re-render the image item to show success status
                const listContainer = document.getElementById('uploadedImagesList');
                listContainer.innerHTML = '';
                this.uploadedImages.forEach(img => this.addImageToList(img));

                // Update hidden input with uploaded URL if this is the default image
                if (this.uploadedImages[imageIndex].isDefault) {
                    document.getElementById('imageUrl').value = result.data.fileUrl;
                    this.showMainPreview(this.uploadedImages[imageIndex]);
                }
            }

        } catch (error) {
            console.error('Error uploading image:', error);

            // Update image status to show error
            const imageIndex = this.uploadedImages.findIndex(img => img.id === tempId);
            if (imageIndex !== -1) {
                this.uploadedImages[imageIndex].uploading = false;
                this.uploadedImages[imageIndex].uploadError = true;

                // Re-render to show error status
                const listContainer = document.getElementById('uploadedImagesList');
                listContainer.innerHTML = '';
                this.uploadedImages.forEach(img => this.addImageToList(img));
            }

            alert('Failed to upload image: ' + error.message);
        }
    },

    // ========================================
    // UI Management
    // ========================================

    addImageToList(imageData) {
        const listContainer = document.getElementById('uploadedImagesList');

        const imageItem = document.createElement('div');
        imageItem.id = `image-${imageData.id}`;
        imageItem.className = 'flex items-center gap-3 p-3 border-2 rounded-lg hover:bg-gray-50 cursor-pointer transition';

        // Set border color based on whether it's default
        if (imageData.isDefault) {
            imageItem.classList.add('border-primary', 'bg-blue-50');
        } else if (imageData.uploadError) {
            imageItem.classList.add('border-red-300', 'bg-red-50');
        } else {
            imageItem.classList.add('border-gray-200');
        }

        // Click on the container sets as default (only if not uploading or error)
        if (!imageData.uploading && !imageData.uploadError) {
            imageItem.onclick = () => this.setDefaultImage(imageData.id);
        }

        // Status badge
        let statusBadge = '';
        if (imageData.uploading) {
            statusBadge = '<span class="px-2 py-0.5 bg-yellow-500 text-white text-xs rounded-full"><i class="fas fa-spinner fa-spin mr-1"></i>Uploading...</span>';
        } else if (imageData.uploadSuccess) {
            statusBadge = '<span class="px-2 py-0.5 bg-green-500 text-white text-xs rounded-full"><i class="fas fa-check mr-1"></i>Uploaded</span>';
        } else if (imageData.uploadError) {
            statusBadge = '<span class="px-2 py-0.5 bg-red-500 text-white text-xs rounded-full"><i class="fas fa-exclamation-triangle mr-1"></i>Failed</span>';
        }

        // Create buttons with proper binding
        const previewBtn = `<button type="button" class="p-2 text-gray-500 hover:text-primary transition" title="Preview image" ${imageData.uploading ? 'disabled' : ''} data-action="preview" data-image-id="${imageData.id}"><i class="fas fa-eye text-sm"></i></button>`;
        const downloadBtn = `<button type="button" class="p-2 text-gray-500 hover:text-primary transition" title="Download image" ${imageData.uploading ? 'disabled' : ''} data-action="download" data-image-id="${imageData.id}"><i class="fas fa-download text-sm"></i></button>`;
        const removeBtn = `<button type="button" class="p-2 text-gray-500 hover:text-red-500 transition" title="Remove image" ${imageData.uploading ? 'disabled' : ''} data-action="remove" data-image-id="${imageData.id}"><i class="fas fa-times text-sm"></i></button>`;

        imageItem.innerHTML = `
            <div class="w-10 h-10 flex-shrink-0">
                <img src="${imageData.dataUrl}" alt="${imageData.name}" class="w-full h-full object-cover rounded ${imageData.uploading ? 'opacity-50' : ''}">
            </div>
            <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 mb-1">
                    <p class="text-sm font-medium text-gray-700 truncate">${imageData.name}</p>
                    ${imageData.isDefault ? '<span class="px-2 py-0.5 bg-primary text-white text-xs rounded-full">Default</span>' : ''}
                    ${statusBadge}
                </div>
                <p class="text-xs text-gray-500">${this.formatFileSize(imageData.size)}</p>
                ${imageData.uploadedUrl ? `<p class="text-xs text-green-600 truncate" title="${imageData.uploadedUrl}"><i class="fas fa-link mr-1"></i>Uploaded to cloud</p>` : ''}
            </div>
            <div class="flex items-center gap-1">
                ${previewBtn}
                ${downloadBtn}
                ${removeBtn}
            </div>
        `;

        // Add event listeners to buttons
        const buttons = imageItem.querySelectorAll('button[data-action]');
        buttons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const action = btn.getAttribute('data-action');
                const imageId = parseFloat(btn.getAttribute('data-image-id'));

                if (action === 'preview') {
                    this.showMainPreview(imageData);
                } else if (action === 'download') {
                    this.downloadImage(imageData.dataUrl, imageData.name);
                } else if (action === 'remove') {
                    this.removeImageFromList(imageId);
                }
            });
        });

        listContainer.appendChild(imageItem);
    },

    setDefaultImage(imageId) {
        // Update all images - remove default from all, set only selected one as default
        this.uploadedImages.forEach(img => {
            img.isDefault = (img.id === imageId);
        });

        // Re-render all images to update star icons and borders
        const listContainer = document.getElementById('uploadedImagesList');
        listContainer.innerHTML = '';
        this.uploadedImages.forEach(img => this.addImageToList(img));

        // Update preview and hidden input
        const defaultImage = this.uploadedImages.find(img => img.isDefault);
        if (defaultImage) {
            this.showMainPreview(defaultImage);
            // Use uploaded URL if available, otherwise use dataUrl
            document.getElementById('imageUrl').value = defaultImage.uploadedUrl || defaultImage.dataUrl;
        }
    },

    showMainPreview(imageData) {
        const mainPreview = document.getElementById('mainImagePreview');
        const mainPreviewImg = document.getElementById('mainPreviewImg');

        mainPreview.classList.remove('hidden');
        mainPreviewImg.src = imageData.dataUrl;

        // Update hidden input with main image URL (prefer uploaded URL)
        document.getElementById('imageUrl').value = imageData.uploadedUrl || imageData.dataUrl;
    },

    removeImageFromList(imageId) {
        // Check if removing default image
        const removingDefault = this.uploadedImages.find(img => img.id === imageId)?.isDefault;

        // Remove from array
        this.uploadedImages = this.uploadedImages.filter(img => img.id !== imageId);

        // Remove from DOM
        const imageItem = document.getElementById(`image-${imageId}`);
        if (imageItem) {
            imageItem.remove();
        }

        // If we removed the default image and there are still images left
        if (removingDefault && this.uploadedImages.length > 0) {
            // Set first remaining image as default
            this.uploadedImages[0].isDefault = true;

            // Re-render all images
            const listContainer = document.getElementById('uploadedImagesList');
            listContainer.innerHTML = '';
            this.uploadedImages.forEach(img => this.addImageToList(img));

            this.showMainPreview(this.uploadedImages[0]);
            document.getElementById('imageUrl').value = this.uploadedImages[0].uploadedUrl || this.uploadedImages[0].dataUrl;
        } else if (this.uploadedImages.length === 0) {
            // No images left
            const mainPreview = document.getElementById('mainImagePreview');
            mainPreview.classList.add('hidden');
            document.getElementById('imageUrl').value = '';
        }
    },

    // ========================================
    // Utilities
    // ========================================

    downloadImage(dataUrl, filename) {
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    },

    /**
     * Get all uploaded images
     * @returns {Array} Array of uploaded image objects
     */
    getUploadedImages() {
        return this.uploadedImages;
    }
};
