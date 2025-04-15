import { EDIT_MODE } from '/js/constants/enums.js';
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


export const addIndexEvents = (entryId, currentEditMode) => {
    const roamingImage = elements.getRoamingImage();
    animateBackgroundImage(roamingImage);
    
    editMode.init(currentEditMode, elements);
    
    const { advancedButton, simpleButton } = elements.getModeElements();
    advancedButton.addEventListener('click', editMode.switch(EDIT_MODE.Advanced, elements));
    simpleButton.addEventListener('click', editMode.switch(EDIT_MODE.Simple, elements));
    
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