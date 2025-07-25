import { entryApi } from '/js/api/entry-api.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { sentryService } from '/js/services/sentry.js';
import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { initializeCountdown } from '/js/components/countdown.js'; 


function captureException(error, errorMessage, action) {
    sentryService.captureError(error, { action }, errorMessage);
    throw new Error(errorMessage);
}


function displayError(message) {
    const errorElement = document.getElementById('error-message');
    if (errorElement) {
        errorElement.textContent = message;
        uiState.toggleClasses(errorElement, { remove: [CSS_CLASSES.HIDDEN] });
    } else {
        console.error(message);
    }
}


export class PreviewPageController {
    constructor(entryId, elements) {
        this.elements = elements;
        this.entryId = entryId;
    }
    

    async initialize() {
        try {
            this.#initRoamingImage();

            const entryResult = await entryApi.get(this.entryId);
            if (entryResult.isFailure) {
                displayError(entryResult.error);
                return;
            }

            const entry = entryResult.value;
            this.#setupEventHandlers(entry.id);

            let isEditable = false;
            const isEditableResult = await entryApi.isEditable(entry.id);
            if (isEditableResult.isSuccess)
                isEditable = isEditableResult.value;

            this.#updateUIWithData(entry, isEditable);
        } catch (error) {
            captureException(error, 'Failed to initialize preview page', 'initialize');
        }
    }


    async #handleCopyLink(entryId) {
        const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
        const success = await copyUrl(entryUrl);

        if (success) {
            const editButton = this.elements.getActionButtons().edit;
            if (editButton) {
                uiState.toggleClasses(editButton, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE]
                });
            }

            const { created, copied } = this.elements.getServiceMessages();
            if (created && copied) {
                uiState.toggleClasses(created, { add: [CSS_CLASSES.HIDDEN] });
                uiState.toggleClasses(copied, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE, CSS_CLASSES.ANIMATE_SLOW_OUT]
                });

                setTimeout(() => {
                    uiState.toggleClasses(copied, {
                        add: [CSS_CLASSES.HIDDEN],
                        remove: [CSS_CLASSES.ANIMATE]
                    });
                    uiState.toggleClasses(created, {
                        remove: [CSS_CLASSES.HIDDEN],
                        add: [CSS_CLASSES.ANIMATE_SLOW]
                    });
                }, 4000);
            }
        }
    }


    async #handleEdit(entryId) {
        const result = await this.entryApi.copy(entryId);
        if (result.success) 
            redirectTo(ROUTES.ROOT, { id: result.data.id });
        else 
            displayError(result.error);
    }


    async #handleDelete(entryId) {
        const result = await this.entryApi.delete(entryId);
        if (result.success)
            redirectTo(ROUTES.DELETED);
        else
            displayError(result.error);
    }


    #initRoamingImage() {
        try {
            const roamingImage = this.elements.getRoamingImage();
            if (roamingImage) {
                animateBackgroundImage(roamingImage);
            }
        } catch (error) {
            console.error('Error initializing roaming image:', error);
        }
    }


    #setupEventHandlers(entryId) {
        const { copyLink: copyLinkButton, edit: editButton, delete: deleteButton } = this.elements.getActionButtons();
        
        copyLinkButton.addEventListener('click', () => this.#handleCopyLink(entryId));
        editButton.addEventListener('click', () => this.#handleEdit(entryId));
        deleteButton.addEventListener('click', () => this.#handleDelete(entryId));
    }


    #updateUIWithData(entry, isEditable) {
        initializeCountdown(new Date(entry.expiresAt));

        const textContent = this.elements.getTextContentElement();
        textContent.textContent = entry.textContent;

        const editButton = this.elements.getEditEntryButton();
        editButton.classList.toggle('hidden', !isEditable);
    }
}