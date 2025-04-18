import { EDIT_MODE } from '/js/constants/enums.js'; 
import { uiState } from '/js/utils/ui-core.js';
import { addDropAreaEvents, pasteImages } from '/js/components/gallery/upload.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { preview } from '/js/components/gallery/preview.js'; 
import { elements } from './elements.js';
import { createButton } from './create-button.js';
import { editMode } from './edit-mode.js';
import { adjustTextareaSizes } from './textarea.js';


const initPaste = (entryId) => {
    document.addEventListener('paste', async (e) => {
        await pasteImages(entryId, e);
    });
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
    
    createButton.init(elements);
    
    addDropAreaEvents(entryId);
    initPaste(entryId);

    preview.initPreloadedImages(entryId);
};


export const setupTextareas = (currentEditMode) => {
    const textareas = elements.getTextareas();
    adjustTextareaSizes(textareas, elements, currentEditMode);
};


window.addIndexEvents = addIndexEvents;
window.setupTextareas = setupTextareas;