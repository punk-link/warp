import { elements } from './elements.js';
import { createButton } from './create-button.js';
import { editMode } from './edit-mode.js';
import { adjustTextareaSizes } from './textarea.js';
import { dom } from '/js/utils/ui-core.js';
import { EDIT_MODE } from '/js/constants/enums.js';
import { addDropAreaEvents, pasteImages } from '/js/components/gallery/upload.js';
import { repositionBackgroundImage } from '/js/components/background/image-positioner.js';


const initPaste = () => {
    const handlePaste = async e => {
        if (e.ctrlKey && e.key.toLowerCase() === 'v') {
            await pasteImages();
        }
    };

    document.body.addEventListener('keydown', handlePaste);
};


export const addIndexEvents = (entryId, currentEditMode) => {
    const roamingImage = elements.getRoamingImage();
    repositionBackgroundImage(roamingImage);
    
    editMode.init(currentEditMode, elements);
    
    const { advancedButton, simpleButton, advancedContainer, simpleContainer } = elements.getModeElements();
    
    advancedButton.addEventListener('click', editMode.switch(advancedButton, simpleButton, advancedContainer, simpleContainer, EDIT_MODE.Advanced, elements));
    simpleButton.addEventListener('click', editMode.switch(simpleButton, advancedButton, simpleContainer, advancedContainer, EDIT_MODE.Simple, elements));
    
    createButton.init(elements);
    
    const { dropArea, fileInput, uploadButton } = elements.getImageElements();
    addDropAreaEvents(entryId, dropArea, fileInput, uploadButton);
    initPaste();
};


export const setupTextareas = (currentEditMode) => {
    const textareas = elements.getTextareas();
    adjustTextareaSizes(textareas, elements, currentEditMode);
};