import { http } from '/js/services/http/client.js';
import { sentryService } from '/js/services/sentry.js';
import { ROUTES } from '/js/utils/routes.js';


/**
 * API for managing creator data
 */
export const creatorApi = {
    /**
     * Fetches or sets the creator data.
     * @returns {Promise<object>} The response data
     */
    getOrSet: async () => {
        try {
            const response = await http.get(ROUTES.API.CREATORS.GET_OR_SET);
            return await response.json();
        } catch (error) {
            const errorMessage = 'Failed to get or set creator data';
            sentryService.captureError(error, { action: 'getOrSetCreator' }, errorMessage);

            throw new Error(errorMessage);
        }
    }
};