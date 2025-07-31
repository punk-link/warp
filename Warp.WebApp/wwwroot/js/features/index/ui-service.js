import { uiState } from '/js/utils/ui-core.js';
import { metadataApi } from '/js/api/metadata-api.js';
import { EDIT_MODE } from '/js/constants/enums.js';

/**
 * Service for handling UI initialization and updates for the index page
 */
export class IndexUIService {
    constructor(elements) {
        this.elements = elements;
    }

    /**
     * Initializes ID input field
     */
    initIdInput(entry) {
        const idInput = this.elements.getId();
        uiState.setElementValue(idInput, entry.id);
    }

    /**
     * Initializes and populates expiration selector
     */
    async initExpirationSelector(entry) {
        const expirationSelector = this.elements.getExpirationSelector();

        await this._populateExpirationSelector(expirationSelector);
        uiState.setElementValue(expirationSelector, entry.expirationPeriod);
    }

    /**
     * Initializes edit mode and textareas
     */
    initEditModeAndTextareas(entry, editModeService) {
        const { advancedTextarea, simpleTextarea } = this.elements.getModeElements();
        
        uiState.setElementValue(advancedTextarea, entry.textContent || '');
        uiState.setElementValue(simpleTextarea, entry.textContent || '');

        const isSwitchAvailable = editModeService.isSwitchAvailable(entry.editMode);
        editModeService.init(entry.editMode, this.elements);

        if (isSwitchAvailable) {
            this._setupEditModeButtons(editModeService);
        }
    }

    /**
     * Shows user-friendly error message
     */
    showError(message) {
        // In a real implementation, this would show a toast/modal/banner
        // For now, we'll use console.error and could extend to UI notifications
        console.error('User Error:', message);
        
        // TODO: Implement proper UI error display
        // Could show a toast notification, error banner, or modal
    }

    /**
     * Shows loading state
     */
    setLoadingState(isLoading) {
        // TODO: Implement loading spinner/overlay
        console.log('Loading state:', isLoading);
    }

    async _populateExpirationSelector(expirationSelector) {
        const expirationPeriods = await metadataApi.getExpirationPeriods();

        const options = expirationSelector.querySelectorAll('option');
        options.forEach(option => {
            // Option values have a 1-based index, so we subtract 1 to get the correct key
            const key = option.value - 1;
            if (expirationPeriods[key]) {
                option.value = expirationPeriods[key];
            }
        });
    }

    _setupEditModeButtons(editModeService) {
        const { advancedButton, simpleButton } = this.elements.getModeElements();
        
        advancedButton.addEventListener('click', editModeService.switch(EDIT_MODE.Advanced, this.elements));
        simpleButton.addEventListener('click', editModeService.switch(EDIT_MODE.Simple, this.elements));

        uiState.setElementDisabled(advancedButton, false);
        uiState.setElementDisabled(simpleButton, false);
    }
}