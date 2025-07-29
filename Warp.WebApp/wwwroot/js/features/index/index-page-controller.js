import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { uiState } from '/js/utils/ui-core.js';
import { adjustTextareaSizes } from './textarea.js';
import { IndexFormService } from './form-service.js';
import { IndexEventService } from './event-service.js';
import { editModeService } from './edit-mode-service.js';
import { createButtonService } from './create-button-service.js';
import { BasePageController } from '/js/features/base-page-controller.js';


export class IndexPageController extends BasePageController {
    constructor(elements, creatorApi, entryApi, metadataApi) {
        super(elements);

        this.creatorApi = creatorApi;
        this.entryApi = entryApi;
        this.metadataApi = metadataApi;

        this.eventService = new IndexEventService();
        this.formService = new IndexFormService(elements, this.entryApi);

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
                this.#initGallery(entry);
                this.#initEventListeners(entry);
            } catch (error) {
                captureException(error, 'Failed to initialize index page', 'initialize');
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


    #createFormSubmitHandler(entry) {
        return async (event) => {
            if (!entry || !this.createButton)
                return;

            uiState.setElementDisabled(this.createButton, true);

            try {
                const formData = this.formService.collectFormData(entry.id);
                const validation = this.formService.validateFormData(formData);
                if (!validation.isValid) {
                    this.displayError(validation.errors.join(', '));
                    return;
                }

                const result = await this.formService.submitWithRetry(entry.id, formData);
                if (result.success && result.data?.id) 
                    redirectTo(ROUTES.PREVIEW(result.data.id));
            } finally {
                uiState.setElementDisabled(this.createButton, false);
            }
        };
    }


    async #getOrCreateEntry(entryId) {
        await this.creatorApi.getOrSet();

        return entryId !== null
            ? await this.entryApi.get(entryId)
            : await this.entryApi.create();
    }


    #initEditModeAndTextareas(entry) {
        const { advancedTextarea, simpleTextarea } = this.elements.getModeElements();

        uiState.setElementValue(advancedTextarea, entry.textContent || '');
        uiState.setElementValue(simpleTextarea, entry.textContent || '');

        editModeService.init(entry.editMode, this.elements);
        if (editModeService.isSwitchAvailable(entry.editMode)) 
            this.#setupEditModeButtons(editModeService);
    }


    #initEventListeners(entry) {
        if (!entry || !this.createButton)
            return;

        const form = this.elements.getForm();
        this.eventService.addFormSubmitListener(form, this.#createFormSubmitHandler(entry));

        this.eventService.addPasteListener(entry.id);
    }


    async #initExpirationSelector(entry) {
        const expirationSelector = this.elements.getExpirationSelector();

        await this.#populateExpirationSelector(expirationSelector);
        uiState.setElementValue(expirationSelector, entry.expirationPeriod);
    }


    #initGallery(entry) {
        if (!this.entry)
            return;

        this.eventService.initGalleryEvents(entry.id);
    }


    async #initUI(entry) {
        await this.#initExpirationSelector(entry);
        this.#initEditModeAndTextareas(entry);

        this.createButton = createButtonService.init(this.elements);
    }


    async #populateExpirationSelector(expirationSelector) {
        const expirationPeriods = await this.metadataApi.getExpirationPeriods();

        const options = expirationSelector.querySelectorAll('option');
        options.forEach(option => {
            // Option values have a 1-based index, so we subtract 1 to get the correct key
            const key = option.value - 1;
            if (expirationPeriods[key]) {
                option.value = expirationPeriods[key];
            }
        });
    }


    #setEntryId(entryId) {
        const idInput = this.elements.getId();
        if (!idInput)
            throw new Error("Can't set the entry ID.");

        uiState.setElementValue(idInput, entryId);
    }


    #setupEditModeButtons(editModeService) {
        const { advancedButton, simpleButton } = this.elements.getModeElements();

        advancedButton.addEventListener('click', editModeService.switch(EDIT_MODE.Advanced, this.elements));
        simpleButton.addEventListener('click', editModeService.switch(EDIT_MODE.Simple, this.elements));

        uiState.setElementDisabled(advancedButton, false);
        uiState.setElementDisabled(simpleButton, false);
    }
}