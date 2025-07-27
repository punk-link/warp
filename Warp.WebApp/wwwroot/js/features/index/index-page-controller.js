import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { uiState } from '/js/utils/ui-core.js';
import { adjustTextareaSizes } from './textarea.js';
import { IndexFormService } from './form-service.js';
import { IndexUIService } from './ui-service.js';
import { IndexEventService } from './event-service.js';
import { IndexApiService } from './api-service.js';
import { editModeService } from './edit-mode-service.js';
import { createButtonService } from './create-button-service.js';
import { BasePageController } from '/js/features/base-page-controller.js';

export class IndexPageController extends BasePageController {
    constructor(elements, creatorApi, entryApi, metadataApi) {
        super(elements);

        this.creatorApi = creatorApi;
        this.entryApi = entryApi;
        this.metadataApi = metadataApi;

        this.apiService = new IndexApiService();
        this.uiService = new IndexUIService(elements);
        this.eventService = new IndexEventService();
        this.formService = new IndexFormService(elements, this.apiService.getApis());
        
        this.entry = null;
        this.createButton = null;
    }


    async initialize(entryId) {
        this.executeWithLoadingIndicator(async () => {
            try {
                this.initRoamingImage();

                const entryResult = await this.#getOrCreateEntry(entryId);
                if (entryResult.isFailure) {
                    this.displayError(entryResult.error);
                    return;
                }

                const entry = entryResult.value;

                this.#setEntryId(entry.id);

                await this.#initUI(entry);
                this._initializeEventListeners();
                this._initializeGallery();

            } catch (error) {
                console.error('Error during page initialization:', error);
                this.uiService.showError('Failed to initialize page. Please refresh.');
            }
        });
    }


    setupTextareas(currentEditMode) {
        try {
            const textareas = this.elements.getTextareas();
            adjustTextareaSizes(textareas, this.elements, currentEditMode);
        } catch (error) {
            console.error('Error setting up textareas:', error);
        }
    }


    cleanup() {
        this.eventService.cleanup();
    }


    async #getOrCreateEntry(entryId) {
        await this.creatorApi.getOrSet();

        return entryId !== null
            ? await this.entryApi.get(entryId)
            : await this.entryApi.create();
    }


    async #initUI(entry) {
        this.#setEntryId(entry.id);
        await this.uiService.initExpirationSelector(entry);
        this.uiService.initEditModeAndTextareas(entry, editModeService);

        this.createButton = createButtonService.init(this.elements);
    }


    #setEntryId(entryId) {
        const idInput = this.elements.getId();
        if (!idInput)
            throw new Error("Can't set the entry ID.");

        uiState.setElementValue(idInput, entryId);
    }


    _initializeEventListeners() {
        if (!this.entry || !this.createButton) return;

        const form = this.elements.getForm();
        if (form) {
            this.eventService.addFormSubmitListener(
                form, 
                this._createFormSubmitHandler()
            );
        }

        this.eventService.addPasteListener(this.entry.id);
    }


    _initializeGallery() {
        if (!this.entry) return;
        
        this.eventService.initGalleryEvents(this.entry.id);
    }


    _createFormSubmitHandler() {
        return async (event) => {
            if (!this.entry || !this.createButton) return;

            uiState.setElementDisabled(this.createButton, true);
            this.uiService.setLoadingState(true);

            try {
                const formData = this.formService.collectFormData(this.entry.id);
                
                const validation = this.formService.validateFormData(formData);
                if (!validation.isValid) {
                    this.uiService.showError(validation.errors.join(', '));
                    return;
                }

                const result = await this.formService.submitWithRetry(this.entry.id, formData);
                
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