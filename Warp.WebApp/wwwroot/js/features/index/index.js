import { EDIT_MODE } from '/js/constants/enums.js'; 
import { uiState } from '/js/utils/ui-core.js';
import { addDropAreaEvents, pasteImages } from '/js/components/gallery/upload.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { preview } from '/js/components/gallery/preview.js'; 
import { core } from '/js/core/initialize.js';
import { elements } from './elements.js';
import { createButtonService } from './create-button-service.js';
import { editMode } from './edit-mode.js';
import { adjustTextareaSizes } from './textarea.js';
import { creatorApi } from '/js/api/creator-api.js';
import { entryApi } from '/js/api/entry-api.js';
import { metadataApi } from '/js/api/metadata-api.js';


core.initialize();


const collectFormData = (entryId) => {
    const { advancedTextarea, simpleTextarea, editModeInput } = elements.getModeElements();
    const currentMode = parseInt(editModeInput.value, 10);

    // Get text from the active textarea based on current mode
    const textContent = currentMode === EDIT_MODE.Advanced
        ? advancedTextarea.value
        : simpleTextarea.value;

    const expirationPeriod = elements.getExpirationSelector().value;

    // Get image IDs if any
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
            redirectTo(ROUTES.ENTRY, { id: response.id });
        } else {
            console.error('Failed to create entry - no valid response received');
            uiState.setElementDisabled(createButton, false);
        }
    } catch (error) {
        console.error('Error submitting form:', error);
        uiState.setElementDisabled(createButton, false);
    }
};


const initPaste = (entryId) => {
    document.addEventListener('paste', async (e) => {
        await pasteImages(entryId, e);
    });
};


const populateExpirationSelector = (expirationPeriods) => {
    const expirationSelector = elements.getExpirationSelector();

    const options = expirationSelector.querySelectorAll('option');
    options.forEach(option => {
        // Option values have a 1-based index, so we subtract 1 to get the correct key
        const key = option.value - 1;
        if (expirationPeriods[key]) {
            option.value = expirationPeriods[key];
        }
    });
}


export const initIndex = async () => {
    await creatorApi.getOrSet();

    const expirationPeriods = await metadataApi.getExpirationPeriods();
    populateExpirationSelector(expirationPeriods);

    const initialEntry = await entryApi.create();
    
    if (initialEntry) {
        const idInput = elements.getId();
        uiState.setElementValue(idInput, initialEntry.id);

        const { advancedTextarea, simpleTextarea, editModeInput } = elements.getModeElements();
        if (advancedTextarea)
            uiState.setElementValue(advancedTextarea, initialEntry.textContent || '');
            
        if (simpleTextarea)
            uiState.setElementValue(simpleTextarea, initialEntry.textContent || '');

        const expirationSelector = elements.getExpirationSelector();
        uiState.setElementValue(expirationSelector, initialEntry.expirationPeriod);

        uiState.setElementValue(editModeInput, initialEntry.editMode);
    }
};


export const addIndexEvents = (entryId, initialEditMode) => {
    const roamingImage = elements.getRoamingImage();
    animateBackgroundImage(roamingImage);

    const isSwitchAvailable = editMode.isSwitchAvailable(initialEditMode);
    editMode.init(initialEditMode, elements);
    
    if (isSwitchAvailable) { 
        const { advancedButton, simpleButton } = elements.getModeElements();
        advancedButton.addEventListener('click', editMode.switch(EDIT_MODE.Advanced, elements));
        simpleButton.addEventListener('click', editMode.switch(EDIT_MODE.Simple, elements));

        uiState.setElementDisabled(advancedButton, false);
        uiState.setElementDisabled(simpleButton, false);
    }
    
    let createButton = createButtonService.init(elements);
    
    const form = elements.getForm();
    form.addEventListener('submit', handleFormSubmit(entryId, createButton));

    addDropAreaEvents(entryId);
    initPaste(entryId);

    preview.initPreloadedImages(entryId);
};


export const setupTextareas = (currentEditMode) => {
    const textareas = elements.getTextareas();
    adjustTextareaSizes(textareas, elements, currentEditMode);
};


window.addIndexEvents = addIndexEvents;
window.initIndex = initIndex;
window.setupTextareas = setupTextareas;