import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { uiState } from '/js/utils/ui-core.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { adjustTextareaSizes } from './textarea.js';
import { IndexFormService } from './form-service.js';
import { IndexUIService } from './ui-service.js';
import { IndexEventService } from './event-service.js';
import { IndexApiService } from './api-service.js';
import { editModeService } from './edit-mode-service.js';
import { createButtonService } from './create-button-service.js';

/**
 * Main controller for the index page functionality
 */
export class IndexPageController {
    constructor(elements) {
        this.elements = elements;
        this.apiService = new IndexApiService();
        this.uiService = new IndexUIService(elements);
        this.eventService = new IndexEventService();
        this.formService = new IndexFormService(elements, this.apiService.getApis());
        
        this.currentEntry = null;
        this.createButton = null;
    }

    /**
     * Initializes the entire page functionality
     */
    async initialize() {
        try {
            this._initRoamingImage();
            
            const entryResult = await this.apiService.initializeEntry();
            if (!entryResult.success) {
                this.uiService.showError(entryResult.userMessage);
                return;
            }

            this.currentEntry = entryResult.data;
            
            await this._initializeUI();
            this._initializeEventListeners();
            this._initializeGallery();

        } catch (error) {
            console.error('Error during page initialization:', error);
            this.uiService.showError('Failed to initialize page. Please refresh.');
        }
    }

    /**
     * Sets up textareas with proper sizing
     */
    setupTextareas(currentEditMode) {
        try {
            const textareas = this.elements.getTextareas();
            adjustTextareaSizes(textareas, this.elements, currentEditMode);
        } catch (error) {
            console.error('Error setting up textareas:', error);
        }
    }

    /**
     * Cleans up resources when page is unloaded
     */
    cleanup() {
        this.eventService.cleanup();
    }

    async _initializeUI() {
        if (!this.currentEntry) return;

        this.uiService.initIdInput(this.currentEntry);
        await this.uiService.initExpirationSelector(this.currentEntry);
        this.uiService.initEditModeAndTextareas(this.currentEntry, editModeService);
        
        this.createButton = createButtonService.init(this.elements);
    }

    _initializeEventListeners() {
        if (!this.currentEntry || !this.createButton) return;

        const form = this.elements.getForm();
        if (form) {
            this.eventService.addFormSubmitListener(
                form, 
                this._createFormSubmitHandler()
            );
        }

        this.eventService.addPasteListener(this.currentEntry.id);
    }

    _initializeGallery() {
        if (!this.currentEntry) return;
        
        this.eventService.initGalleryEvents(this.currentEntry.id);
    }

    _initRoamingImage() {
        try {
            const roamingImage = this.elements.getRoamingImage();
            if (roamingImage) {
                animateBackgroundImage(roamingImage);
            }
        } catch (error) {
            console.error('Error initializing roaming image:', error);
        }
    }

    _createFormSubmitHandler() {
        return async (event) => {
            if (!this.currentEntry || !this.createButton) return;

            uiState.setElementDisabled(this.createButton, true);
            this.uiService.setLoadingState(true);

            try {
                const formData = this.formService.collectFormData(this.currentEntry.id);
                
                const validation = this.formService.validateFormData(formData);
                if (!validation.isValid) {
                    this.uiService.showError(validation.errors.join(', '));
                    return;
                }

                const result = await this.formService.submitWithRetry(this.currentEntry.id, formData);
                
                if (result.success && result.data?.id) {
                    redirectTo(ROUTES.PREVIEW(result.data.id));
                } else {
                    this.uiService.showError(result.userMessage || 'Failed to create entry');
                }
            } catch (error) {
                console.error('Error submitting form:', error);
                this.uiService.showError('An unexpected error occurred. Please try again.');
            } finally {
                uiState.setElementDisabled(this.createButton, false);
                this.uiService.setLoadingState(false);
            }
        };
    }
}