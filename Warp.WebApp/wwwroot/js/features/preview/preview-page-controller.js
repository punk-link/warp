import { entryApi } from '/js/api/entry-api.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { sentryService } from '/js/services/sentry.js';
import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';


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
        this.apiService = new PreviewApiService();
        this.entryId = entryId;

        this.isEditable = false;
    }
    

    async initialize() {
        try {
            this._initRoamingImage();

            const entryResult = await this._getEntry(this.entryId);
            if (entryResult.isFailure) {
                displayError(entryResult.error());
                return;
            }

            const entry = entryResult.value();
            isEditableResult = await this._isEditable(entry.id);
            if (!isEditableResult.isSuccess)
                this.isEditable = isEditableResult.value;

            this._setupEventHandlers(entryId);
            
            this._updateUIWithCanEdit();
            
        } catch (error) {
            captureException(error, 'Failed to initialize preview page', 'initialize');
        }
    }


    async _getEntry(entryId) {
        return await entryApi.get(entryId);
    }


    async _handleCopyLink(entryId) {
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


    async _handleEdit(entryId) {
        const result = await this.entryApi.copy(entryId);
        if (result.success) 
            redirectTo(ROUTES.ROOT, { id: result.data.id });
        else 
            displayError(result.error);
    }


    async _handleDelete(entryId) {
        const result = await this.entryApi.delete(entryId);
        if (result.success)
            redirectTo(ROUTES.DELETED);
        else
            displayError(result.error);
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


    async _isEditable(entryId) {
        try {
            const response = await http.get(ROUTES.API.ENTRIES.IS_EDITABLE(entryId));
            return await response.json();
        } catch (error) {
            captureException(error, 'Failed to check edit permissions', 'isEditable');
        }
    }


    _setupEventHandlers(entryId) {
        const { copyLink: copyLinkButton, edit: editButton, delete: deleteButton } = this.elements.getActionButtons();
        
        copyLinkButton.addEventListener('click', () => this._handleCopyLink(entryId));
        editButton.addEventListener('click', () => this._handleEdit(entryId));
        deleteButton.addEventListener('click', () => this._handleDelete(entryId));
    }


    _updateUIWithCanEdit() {
        // Show/hide edit buttons based on canEdit flag from server
        const editButton = this.elements.getActionButtons().edit;
        const editEntryButton = document.getElementById('edit-entry-button');
        
        if (editButton) {
            editButton.classList.toggle('hidden', !this.canEdit);
        }
        
        if (editEntryButton) {
            editEntryButton.style.display = this.canEdit ? 'block' : 'none';
        }
    }
}