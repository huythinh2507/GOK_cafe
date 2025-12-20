// Payment Management System for GOK Cafe

class PaymentManager {
    constructor() {
        this.currentPayment = null;
        this.paymentCheckInterval = null;
        this.PAYMENT_CHECK_INTERVAL = 10000; // Check every 10 seconds
    }

    /**
     * Initialize payment for an order
     * @param {string} orderId - Order ID
     * @param {number} paymentMethod - Payment method (0=Cash, 4=BankTransfer)
     * @param {string} bankCode - Bank code for bank transfer
     */
    async initializePayment(orderId, paymentMethod, bankCode = null) {
        try {
            const paymentData = {
                orderId: orderId,
                paymentMethod: paymentMethod,
                bankCode: bankCode
            };

            const response = await window.apiService.createPayment(paymentData);

            if (response.success && response.data) {
                this.currentPayment = response.data;
                return {
                    success: true,
                    payment: response.data.payment,
                    qrCodeInfo: response.data.qrCodeInfo
                };
            } else {
                return {
                    success: false,
                    message: response.message || 'Failed to create payment'
                };
            }
        } catch (error) {
            console.error('Error initializing payment:', error);
            return {
                success: false,
                message: error.message || 'An error occurred while creating payment'
            };
        }
    }

    /**
     * Display QR code payment modal
     * @param {Object} qrCodeInfo - QR code information from payment creation
     */
    displayQRCodePayment(qrCodeInfo) {
        const modal = document.getElementById('qrPaymentModal');
        if (!modal) {
            console.error('QR Payment Modal not found');
            return;
        }

        // Update QR code image
        const qrImage = document.getElementById('qrCodeImage');
        if (qrImage && qrCodeInfo.qrCodeImageUrl) {
            qrImage.src = qrCodeInfo.qrCodeImageUrl;
        }

        // Update bank details
        document.getElementById('bankName').textContent = qrCodeInfo.bankName || '';
        document.getElementById('accountNumber').textContent = qrCodeInfo.accountNumber || '';
        document.getElementById('accountName').textContent = qrCodeInfo.accountName || '';
        document.getElementById('paymentAmount').textContent = this.formatCurrency(qrCodeInfo.amount);
        document.getElementById('paymentDescription').textContent = qrCodeInfo.paymentDescription || '';

        // Update expiry countdown
        if (qrCodeInfo.expiresAt) {
            this.startExpiryCountdown(new Date(qrCodeInfo.expiresAt));
        }

        // Show modal
        modal.classList.remove('hidden');
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';

        // TODO: Payment status verification will be implemented later
        // this.startPaymentStatusCheck();
    }

    /**
     * Start countdown timer for payment expiry
     * @param {Date} expiryDate - Payment expiry date
     */
    startExpiryCountdown(expiryDate) {
        const countdownElement = document.getElementById('paymentCountdown');
        if (!countdownElement) return;

        const updateCountdown = () => {
            const now = new Date();
            const diff = expiryDate - now;

            if (diff <= 0) {
                countdownElement.textContent = 'Expired';
                countdownElement.classList.add('text-red-600');
                this.stopPaymentStatusCheck();
                return;
            }

            const minutes = Math.floor(diff / 60000);
            const seconds = Math.floor((diff % 60000) / 1000);
            countdownElement.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
        };

        updateCountdown();
        this.countdownInterval = setInterval(updateCountdown, 1000);
    }

    /**
     * Start periodic checking of payment status
     */
    startPaymentStatusCheck() {
        if (!this.currentPayment || !this.currentPayment.payment) return;

        const paymentId = this.currentPayment.payment.id;

        this.paymentCheckInterval = setInterval(async () => {
            try {
                const response = await window.apiService.verifyPayment({
                    paymentId: paymentId
                });

                if (response.success && response.data) {
                    const status = response.data.status;

                    // Status: 0=Pending, 1=Paid, 2=Failed, 3=Refunded
                    if (status === 1) {
                        this.handlePaymentSuccess(response.data);
                    } else if (status === 2) {
                        this.handlePaymentFailed(response.data);
                    }
                }
            } catch (error) {
                console.error('Error checking payment status:', error);
            }
        }, this.PAYMENT_CHECK_INTERVAL);
    }

    /**
     * Stop payment status checking
     */
    stopPaymentStatusCheck() {
        if (this.paymentCheckInterval) {
            clearInterval(this.paymentCheckInterval);
            this.paymentCheckInterval = null;
        }
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
            this.countdownInterval = null;
        }
    }

    /**
     * Handle successful payment
     * @param {Object} paymentData - Payment verification data
     */
    handlePaymentSuccess(paymentData) {
        this.stopPaymentStatusCheck();
        this.closeQRPaymentModal();

        // Show success message
        this.showNotification('Payment successful! Your order has been confirmed.', 'success');

        // Redirect to order confirmation page
        setTimeout(() => {
            if (this.currentPayment && this.currentPayment.payment) {
                window.location.href = `/order-confirmation?orderId=${this.currentPayment.payment.orderId}`;
            }
        }, 2000);
    }

    /**
     * Handle failed payment
     * @param {Object} paymentData - Payment verification data
     */
    handlePaymentFailed(paymentData) {
        this.stopPaymentStatusCheck();
        this.showNotification('Payment failed. Please try again.', 'error');
    }

    /**
     * Close QR payment modal
     */
    closeQRPaymentModal() {
        const modal = document.getElementById('qrPaymentModal');
        if (modal) {
            modal.classList.remove('show');
            modal.classList.add('hidden');
            document.body.style.overflow = '';
        }
        this.stopPaymentStatusCheck();
    }

    /**
     * Cancel current payment
     */
    async cancelPayment() {
        if (!this.currentPayment || !this.currentPayment.payment) return;

        try {
            const response = await window.apiService.cancelPayment(this.currentPayment.payment.id);

            if (response.success) {
                this.showNotification('Payment cancelled', 'info');
                this.closeQRPaymentModal();
                this.currentPayment = null;
            }
        } catch (error) {
            console.error('Error cancelling payment:', error);
        }
    }

    /**
     * Format currency (VND)
     * @param {number} amount - Amount to format
     * @returns {string} Formatted currency string
     */
    formatCurrency(amount) {
        return amount.toLocaleString('vi-VN') + 'đ';
    }

    /**
     * Show notification message
     * @param {string} message - Message to display
     * @param {string} type - Type: success, error, info
     */
    showNotification(message, type = 'info') {
        const bgColors = {
            success: '#10B981',
            error: '#EF4444',
            info: '#3B82F6'
        };

        const notification = document.createElement('div');
        notification.style.cssText = `
            position: fixed;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%) translateY(100px);
            z-index: 10000;
            background: ${bgColors[type]};
            color: white;
            padding: 16px 24px;
            border-radius: 8px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
            display: flex;
            align-items: center;
            gap: 12px;
            font-weight: 500;
            min-width: 320px;
            transition: transform 0.3s ease;
        `;

        const icon = type === 'success' ? 'check-circle' : type === 'error' ? 'times-circle' : 'info-circle';
        notification.innerHTML = `
            <i class="fas fa-${icon}" style="font-size: 20px;"></i>
            <span>${message}</span>
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.transform = 'translateX(-50%) translateY(0)';
        }, 10);

        setTimeout(() => {
            notification.style.transform = 'translateX(-50%) translateY(100px)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
}

// Initialize global payment manager
window.paymentManager = new PaymentManager();

console.log('Payment Manager initialized successfully');
