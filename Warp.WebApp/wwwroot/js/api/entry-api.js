import { Result } from '/js/models/result.js';
import { UnitResult } from '/js/models/unit-result.js';
import { http } from '/js/services/http/client.js';
import { sentryService } from '/js/services/sentry.js';
import { ROUTES } from '/js/utils/routes.js';


function captureException(error, entryId, errorMessage, action) {
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

    const response = await http.post(ROUTES.API.ENTRIES.ADD_OR_UPDATE(entryId), requestBody, entryId);
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
            captureException(error, entryId, errorMessage, 'addEntry');
        }
    },

    /**
     * Creates a copy of an entry
     * @param {string} entryId - The encoded entry ID to copy
     * @returns {Promise<Result>} The copied entry data
     */
    copy: async (entryId) => {
        try {
            const endpoint = ROUTES.API.ENTRIES.COPY(entryId);
            const response = await http.post(endpoint);
            const json = await response.json();

            return Result.fromJson(json);
        } catch (error) {
            const errorMessage = `Failed to copy entry: ${entryId}`;
            captureException(error, entryId, errorMessage, 'copyEntry');
        }
    },

    /**
     * Creates a new entry
     * @return {Promise<Result>} The newly created entry data
     */
    create: async () => {
        try {
            const response = await http.get(ROUTES.API.ENTRIES.CREATE);
            const json = await response.json();

            return Result.fromJson(json);
        } catch (error) {
            captureException(error, null, 'Failed to create new entry', 'createEntry');
        }
    },

    /**
     * Deletes an entry
     * @param {string} entryId - The encoded entry ID to delete
     * @returns {Promise<void>}
     */
    delete: async (entryId) => {
        try {
            const endpoint = ROUTES.API.ENTRIES.DELETE(entryId);
            const response = await http.delete(endpoint);
            if (!response.ok) 
                return UnitResult.failure('Failed to delete entry');

            return UnitResult.success();
        } catch (error) {
            const errorMessage = `Failed to delete entry: ${entryId}`;
            captureException(error, entryId, errorMessage, 'deleteEntry');
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
            const json = await response.json();

            return Result.fromJson(json);
        } catch (error) {
            const errorMessage = `Failed to load entry${entryId ? ': ' + entryId : ''}`;
            captureException(error, entryId, errorMessage, 'getEntry');
        }
    },

    isEditable: async (entryId) => {
        try {
            const endpoint = ROUTES.API.ENTRIES.IS_EDITABLE(entryId);
            const response = await http.get(endpoint);
            const json = await response.json();

            return Result.fromJson(json);
        } catch (error) {
            const errorMessage = `Failed to check if entry is editable: ${entryId}`;
            captureException(error, entryId, errorMessage, 'isEditableEntry');
        }
    },

    /**
     * Reports an entry for abuse
     * @param {string} entryId - The encoded entry ID to report
     * @returns {Promise<void>}
     */
    report: async (entryId) => {
        try {
            return await http.post(ROUTES.API.ENTRIES.REPORT(entryId));
        } catch (error) {
            const errorMessage = `Failed to report entry: ${entryId}`;
            captureException(error, entryId, errorMessage, 'reportEntry');
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
            captureException(error, entryId, errorMessage, 'updateEntry');
        }
    }
};
