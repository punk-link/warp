import { EDIT_MODE } from '/js/constants/enums.js';
import { CONFIG } from './config.js';

/**
 * Service for handling form data collection and validation for the index page
 */
export class IndexFormService {
    constructor(elements, entryApi) {
        this.elements = elements;
        this.entryApi = entryApi;
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
     * Submits form data with retry mechanism
     */
    async submitWithRetry(entryId, formData, maxAttempts = CONFIG.RETRY.MAX_ATTEMPTS) {
        let lastError;

        for (let attempt = 1; attempt <= maxAttempts; attempt++) {
            try {
                const response = await this.entryApi.add(entryId, formData);
                
                if (response && response.id) {
                    return { success: true, data: response };
                } else {
                    throw new Error('Invalid response received');
                }
            } catch (error) {
                lastError = error;
                
                if (attempt < maxAttempts) {
                    await this._delay(CONFIG.RETRY.DELAY_MS * attempt);
                }
            }
        }

        return { 
            success: false, 
            error: lastError,
            userMessage: 'Failed to create entry. Please try again.'
        };
    }

    _delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}