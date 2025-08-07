import { EDIT_MODE } from '/js/constants/enums.js';
import { CONFIG } from './config.js';

/**
 * Service for handling form data collection and validation for the index page
 */
export class IndexFormService {
    constructor(elements, entryApi) {
        this.elements = elements;
        this.entryApi = entryApi;
        this.isSubmitting = false;
    }

    /**
     * Collects form data from UI elements
     */
    collectFormData(entryId) {
        const { advancedTextarea, simpleTextarea, editModeInput } = this.elements.getModeElements();
        const currentMode = editModeInput.value;

        const textContent = currentMode === EDIT_MODE.Advanced
            ? advancedTextarea.value
            : simpleTextarea.value;

        const expirationPeriod = this.elements.getExpirationSelector().value;

        const imageContainers = this.elements.getActualImageContainers();
        const imageIds = Array.from(imageContainers)
            .filter(container => container.dataset.imageId)
            .map(container => container.dataset.imageId);

        return {
            id: entryId,
            editMode: currentMode,
            expirationPeriod,
            textContent,
            imageIds
        };
    }

    /**
     * Validates form data before submission
     */
    validateFormData(formData) {
        const errors = [];

        if (!formData.textContent?.trim() && formData.imageIds.length === 0) {
            errors.push('Content is required (text or images)');
        }

        if (!formData.expirationPeriod) {
            errors.push('Expiration period is required');
        }

        return {
            isValid: errors.length === 0,
            errors
        };
    }

    /**
     * Submits form data with idempotency protection and retry mechanism
     */
    async submitWithRetry(entryId, formData, maxAttempts = CONFIG.RETRY.MAX_ATTEMPTS) {
        if (this.isSubmitting) {
            return {
                success: false,
                error: new Error('Submission already in progress'),
                userMessage: 'Please wait, your request is being processed.'
            };
        }

        this.isSubmitting = true;
        let lastError;

        try {
            for (let attempt = 1; attempt <= maxAttempts; attempt++) {
                try {
                    const response = await this.entryApi.add(entryId, formData);

                    if (response && response.id) 
                        return { success: true, data: response };
                    
                    throw new Error('Invalid response received');
                } catch (error) {
                    lastError = error;

                    if (error.message.includes('409') || error.message.includes('Conflict')) {
                        return {
                            success: false,
                            error: error,
                            userMessage: 'This request has already been processed. Please refresh the page to continue.'
                        };
                    }

                    if (attempt < maxAttempts) 
                        await this.#delay(CONFIG.RETRY.DELAY_MS * attempt);
                }
            }

            return {
                success: false,
                error: lastError,
                userMessage: 'Failed to create entry. Please try again.'
            };
        } finally {
            this.isSubmitting = false;
        }
    }


    #delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}