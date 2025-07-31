import { http } from '/js/services/http/client.js';
import { sentryService } from '/js/services/sentry.js';
import { ROUTES } from '/js/utils/routes.js';

function captureError(error, errorMessage, action) {
    sentryService.captureError(error, { action }, errorMessage);
    throw new Error(errorMessage);
}

/**
 * API service for Preview page operations
 */
export class PreviewApiService {
    /**
     * Initializes a new instance of the PreviewApiService class
     */
    constructor() {
        // Initialize necessary services
    }

    /**
     * Copies an entry
     * @param {string} id - The entry ID to copy
     * @returns {Promise<{success: boolean, data?: any, userMessage?: string}>}
     */
    async copyEntry(id) {
        try {
            const response = await http.post(ROUTES.API.ENTRIES.COPY(id));
            if (!response.ok) {
                return { success: false, userMessage: 'Failed to copy entry' };
            }
            
            const data = await response.json();
            return { success: true, data };
        } catch (error) {
            console.error('Error copying entry:', error);
            return { success: false, userMessage: 'An error occurred while copying the entry' };
        }
    }

    /**
     * Deletes an entry
     * @param {string} id - The entry ID to delete
     * @returns {Promise<{success: boolean, userMessage?: string}>}
     */
    async deleteEntry(id) {
        try {
            const response = await http.delete(ROUTES.API.ENTRIES.DELETE(id));
            if (response.ok) {
                return { success: true };
            }
            
            return { success: false, userMessage: 'Failed to delete entry' };
        } catch (error) {
            console.error('Error deleting entry:', error);
            return { success: false, userMessage: 'An error occurred while deleting the entry' };
        }
    }
}