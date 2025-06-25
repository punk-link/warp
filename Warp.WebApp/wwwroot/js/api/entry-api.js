import { http } from '/js/services/http/client.js';
import { sentryService } from '/js/services/sentry.js';
import { ROUTES } from '/js/utils/routes.js';


function captureError(error, entryId, errorMessage, action) {
    sentryService.captureError(error, { entryId, action }, errorMessage);
    throw new Error(errorMessage);
}


async function addOrUpdate(entryId, formData) {
    const requestBody = {
        id: formData.id || entryId,
        editMode: formData.editMode,
        expirationPeriod: formData.expirationPeriod,
        imageIds: (formData.imageIds || []).map(id => (typeof id === 'string' ? id : id.toString())),
        textContent: formData.textContent || ''
    };

    const response = await http.post(ROUTES.API.ENTRIES.ADD_OR_UPDATE(entryId), requestBody);
    return await response.json();
}


/**
 * API for managing entries
 */
export const entryApi = {
    /**
     * Submits entry data to the API
     * @param {string} entryId - The encoded entry ID
     * @param {object} formData - The form data to submit
     * @returns {Promise<object>} The response data
     */
    add: async (entryId, formData) => {
        try {
            return await addOrUpdate(entryId, formData);
        } catch (error) {
            const errorMessage = 'Failed to add entry';
            captureError(error, entryId, errorMessage, 'addEntry');
        }
    },

    /**
     * Creates a copy of an entry
     * @param {string} entryId - The encoded entry ID to copy
     * @returns {Promise<object>} The copied entry data
     */
    copy: async (entryId) => {
        try {
            const response = await http.post(ROUTES.API.ENTRIES.COPY(entryId));
            return await response.json();
        } catch (error) {
            const errorMessage = `Failed to copy entry: ${entryId}`;
            captureError(error, entryId, errorMessage, 'copyEntry');
        }
    },

    /**
     * Creates a new entry
     * @return {Promise<object>} The newly created entry data
     */
    create: async () => {
        try {
            const response = await http.get(ROUTES.API.ENTRIES.CREATE);
            return await response.json();
        } catch (error) {
            captureError(error, null, 'Failed to create new entry', 'createEntry');
        }
    },

    /**
     * Deletes an entry
     * @param {string} entryId - The encoded entry ID to delete
     * @returns {Promise<void>}
     */
    delete: async (entryId) => {
        try {
            await http.delete(ROUTES.API.ENTRIES.DELETE(entryId));
        } catch (error) {
            const errorMessage = `Failed to delete entry: ${entryId}`;
            captureError(error, entryId, errorMessage, 'deleteEntry');
        }
    },

    /**
     * Loads entry data from the API
     * @param {string} entryId - The encoded entry ID
     * @returns {Promise<object>} The entry data
     */
    get: async (entryId) => {
        try {
            const endpoint = ROUTES.API.ENTRIES.GET(entryId);
            const response = await http.get(endpoint);
            return await response.json();
        } catch (error) {
            const errorMessage = `Failed to load entry${entryId ? ': ' + entryId : ''}`;
            captureError(error, entryId, errorMessage, 'getEntry');
        }
    },

    /**
     * Reports an entry for abuse
     * @param {string} entryId - The encoded entry ID to report
     * @returns {Promise<void>}
     */
    report: async (entryId) => {
        try {
            await http.post(ROUTES.API.ENTRIES.REPORT(entryId));
        } catch (error) {
            const errorMessage = `Failed to report entry: ${entryId}`;
            captureError(error, entryId, errorMessage, 'reportEntry');
        }
    },

    /**
     * Submits entry data to the API
     * @param {string} entryId - The encoded entry ID
     * @param {object} formData - The form data to submit
     * @returns {Promise<object>} The response data
     */
    update: async (entryId, formData) => {
        try {
            return await addOrUpdate(entryId, formData);
        } catch (error) {
            const errorMessage = 'Failed to update entry';
            captureError(error, entryId, errorMessage, 'updateEntry');
        }
    }
};
