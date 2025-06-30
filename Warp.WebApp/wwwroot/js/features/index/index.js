import { EDIT_MODE } from '/js/constants/enums.js';
import { redirectTo, ROUTES } from '/js/utils/routes.js';
import { uiState } from '/js/utils/ui-core.js';
import { addDropAreaEvents, pasteImages } from '/js/components/gallery/upload.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { preview } from '/js/components/gallery/preview.js'; 
import { core } from '/js/core/initialize.js';
import { elements } from './elements.js';
import { createButtonService } from './create-button-service.js';
import { editModeService } from './edit-mode-service.js';
import { adjustTextareaSizes } from './textarea.js';
import { creatorApi } from '/js/api/creator-api.js';
import { entryApi } from '/js/api/entry-api.js';
import { metadataApi } from '/js/api/metadata-api.js';


core.initialize();


const collectFormData = (entryId) => {
    const { advancedTextarea, simpleTextarea, editModeInput } = elements.getModeElements();
    const currentMode = editModeInput.value;

    const textContent = currentMode === EDIT_MODE.Advanced
        ? advancedTextarea.value
        : simpleTextarea.value;

    const expirationPeriod = elements.getExpirationSelector().value;

    const imageContainers = elements.getActualImageContainers();
    const imageIds = Array.from(imageContainers)
        .filter(container => container.dataset.imageId)
        .map(container => container.dataset.imageId);

    return {
        id: entryId,
        editMode: currentMode,
        expirationPeriod,
        textContent,
        imageIds
    };
};


const handleFormSubmit = (entryId, createButton) => async (event) => {
    event.preventDefault();

    uiState.setElementDisabled(createButton, true);

    try {
        const formData = collectFormData(entryId);
        const response = await entryApi.add(entryId, formData);

        if (response && response.id) {
            redirectTo(ROUTES.PREVIEW, { id: response.id });
        } else {
            console.error('Failed to create entry - no valid response received');
        }
    } catch (error) {
        console.error('Error submitting form:', error);
    }

    uiState.setElementDisabled(createButton, false);
};


const initEditModeAndTextareas = (entry) => {
    const { advancedTextarea, simpleTextarea } = elements.getModeElements();
    if (advancedTextarea)
        uiState.setElementValue(advancedTextarea, entry.textContent || '');

    if (simpleTextarea)
        uiState.setElementValue(simpleTextarea, entry.textContent || '');

    const isSwitchAvailable = editModeService.isSwitchAvailable(entry.editMode);
    editModeService.init(entry.editMode, elements);

    if (isSwitchAvailable) {
        const { advancedButton, simpleButton } = elements.getModeElements();
        advancedButton.addEventListener('click', editModeService.switch(EDIT_MODE.Advanced, elements));
        simpleButton.addEventListener('click', editModeService.switch(EDIT_MODE.Simple, elements));

        uiState.setElementDisabled(advancedButton, false);
        uiState.setElementDisabled(simpleButton, false);
    }
}


const initExpirationSelector = async (entry) => {
    const expirationSelector = elements.getExpirationSelector();

    await populateExpirationSelector(expirationSelector);
    uiState.setElementValue(expirationSelector, entry.expirationPeriod);
};


const initIdInput = (entry) => {
    const idInput = elements.getId();
    uiState.setElementValue(idInput, entry.id);
}


const initPaste = (entryId) => {
    document.addEventListener('paste', async (e) => {
        await pasteImages(entryId, e);
    });
};


const initRoamingImage = () => {
    const roamingImage = elements.getRoamingImage();
    animateBackgroundImage(roamingImage);
};


const populateExpirationSelector = async (expirationSelector) => {
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


export const addIndexEvents = async () => {
    initRoamingImage();

    await creatorApi.getOrSet();
    const entry = await entryApi.create();

    initIdInput(entry);
    await initExpirationSelector(entry);
    initEditModeAndTextareas(entry);
    
    const form = elements.getForm();
    const createButton = createButtonService.init(elements);
    form.addEventListener('submit', handleFormSubmit(entry.id, createButton));

    addDropAreaEvents(entry.id);
    initPaste(entry.id);

    preview.initPreloadedImages(entry.id);
};


export const setupTextareas = (currentEditMode) => {
    const textareas = elements.getTextareas();
    adjustTextareaSizes(textareas, elements, currentEditMode);
};


window.addIndexEvents = addIndexEvents;
window.setupTextareas = setupTextareas;