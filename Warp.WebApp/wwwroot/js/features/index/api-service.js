import { creatorApi } from '/js/api/creator-api.js';
import { entryApi } from '/js/api/entry-api.js';
import { metadataApi } from '/js/api/metadata-api.js';

/**
 * Service for coordinating API calls and handling responses for the index page
 */
export class IndexApiService {
    constructor() {
        this.apis = {
            creatorApi,
            entryApi,
            metadataApi
        };
    }

    /**
     * Initializes creator and creates new entry
     */
    async initializeEntry() {
        try {
            await this.apis.creatorApi.getOrSet();
            const entry = await this.apis.entryApi.create();
            
            if (!entry || !entry.id) {
                throw new Error('Failed to create entry - invalid response');
            }

            return { success: true, data: entry };
        } catch (error) {
            console.error('Error initializing entry:', error);
            return { 
                success: false, 
                error,
                userMessage: 'Failed to initialize. Please refresh the page.'
            };
        }
    }

    /**
     * Gets API instances for use by other services
     */
    getApis() {
        return this.apis;
    }

    /**
     * Submits entry data
     */
    async submitEntry(entryId, formData) {
        try {
            const response = await this.apis.entryApi.add(entryId, formData);
            return { success: true, data: response };
        } catch (error) {
            console.error('Error submitting entry:', error);
            return { 
                success: false, 
                error,
                userMessage: 'Failed to submit entry. Please try again.'
            };
        }
    }

    /**
     * Gets expiration periods metadata
     */
    async getExpirationPeriods() {
        try {
            const periods = await this.apis.metadataApi.getExpirationPeriods();
            return { success: true, data: periods };
        } catch (error) {
            console.error('Error getting expiration periods:', error);
            return { 
                success: false, 
                error,
                userMessage: 'Failed to load expiration options.'
            };
        }
    }
}