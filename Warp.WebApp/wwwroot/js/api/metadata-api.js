import { http } from '/js/services/http/client.js';
import { sentryService } from '/js/services/sentry.js';
import { ROUTES } from '/js/utils/routes.js';


function captureError(error, errorMessage, action) {
    sentryService.captureError(error, { action }, errorMessage);
    throw new Error(errorMessage);
}


/**
 * API for retrieving metadata including OpenGraph descriptions and enum values
 */
export const metadataApi = {
    /**
     * Gets the default OpenGraph description
     * @returns {Promise<object>} The default OpenGraph description
     */
    getDefaultOpenGraphDescription: async () => {
        try {
            const response = await http.get(ROUTES.API.METADATA.DEFAULT_OPENGRAPH);
            return await response.json();
        } catch (error) {
            captureError(error, 'Failed to get default OpenGraph description', 'getDefaultOpenGraphDescription');
        }
    },

    /**
    * Gets all available edit modes
    * @returns {Promise<object>} Dictionary of edit mode values and names
    */
    getEditModes: async () => {
        try {
            const response = await http.get(ROUTES.API.METADATA.EDIT_MODES);
            return await response.json();
        } catch (error) {
            captureError(error, 'Failed to get edit modes', 'getEditModes');
        }
    },

    /**
    * Gets all available expiration periods
    * @returns {Promise<object>} Dictionary of expiration period values and durations
    */
    getExpirationPeriods: async () => {
        try {
            const response = await http.get(ROUTES.API.METADATA.EXPIRATION_PERIODS);
            return await response.json();
        } catch (error) {
            captureError(error, 'Failed to get expiration periods', 'getExpirationPeriods');
        }
    }
};