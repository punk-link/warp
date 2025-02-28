import { dom, uiState } from '/js/utils/ui-core.js';
import { eventNames } from '/js/events/events.js';
import { adjustTextareaSize } from '/js/components/textarea.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
import { addDropAreaEvents, pasteImages } from './modules/image-processor.js';


const EditMode = Object.freeze({
    Unknown: 0,
    Simple: 1,
    Advanced: 2
});


const elements = {
    getCreateButton: () => dom.get('create-button'),
    getImageElements: () => ({
        dropArea: dom.query('.drop-area'),
        fileInput: dom.get('file'),
        uploadButton: dom.get('empty-image-container')
    }),
    getModeElements: () => ({
        advancedButton: dom.get('advanced-mode-nav-button'),
        simpleButton: dom.get('simple-mode-nav-button'),
        advancedContainer: dom.get('advanced-mode-container'),
        simpleContainer: dom.get('simple-mode-container'),
        advancedTextarea: dom.get('advanced-text-content'),
        simpleTextarea: dom.get('simple-text-content'),
        editModeInput: dom.get('edit-mode-state')
    }),
    getRoamingImage: () => dom.get('roaming-image')
};


const handlers = {
    createButton: {
        init: () => {
            const createButton = elements.getCreateButton();
            const { advancedTextarea, simpleTextarea, advancedButton, simpleButton } = elements.getModeElements();

            const updateButtonState = () => {
                const isAdvancedMode = advancedButton.classList.contains('active');
                const activeTextarea = isAdvancedMode ? advancedTextarea : simpleTextarea;

                uiState.setElementDisabled(createButton, !activeTextarea.value);
            };

            [advancedTextarea, simpleTextarea].forEach(textarea => {
                textarea.addEventListener('input', updateButtonState);
            });

            [advancedButton, simpleButton].forEach(button => {
                button.addEventListener('click', updateButtonState);
            });

            document.addEventListener(eventNames.uploadFinished, () => uiState.setElementDisabled(createButton, false));

            updateButtonState();
        }
    },
    
    mode: {
        init: (editMode) => {
            const { advancedButton, simpleButton } = elements.getModeElements();
            const activeButton = editMode === EditMode.Advanced ? advancedButton : simpleButton;
            uiState.toggleClasses(activeButton, { add: ['active'] });
        },

        switch: (displayedButton, hiddenButton, displayedContainer, hiddenContainer, mode) => {
            return () => {
                uiState.toggleClasses(hiddenButton, { remove: ['active'] });
                uiState.toggleClasses(displayedButton, { add: ['active'] });
        
                uiState.toggleClasses(hiddenContainer, { add: ['hidden'] });
                uiState.toggleClasses(displayedContainer, { remove: ['hidden'] });
                
                const { editModeInput } = elements.getModeElements();
                uiState.setElementValue(editModeInput, mode);
            };
        }
    },

    paste: {
        init: () => {
            const handlePaste = async e => {
                if (e.ctrlKey && e.key.toLowerCase() === 'v') {
                    await pasteImages();
                }
            };

            document.body.addEventListener('keydown', handlePaste);
        }
    }
};


export const addIndexEvents = (entryId, editMode) => {
    const roamingImage = elements.getRoamingImage();
    repositionBackgroundImage(roamingImage);
    
    handlers.mode.init(editMode);
    
    const { advancedButton, simpleButton, advancedContainer, simpleContainer } = elements.getModeElements();
    
    advancedButton.addEventListener('click', handlers.mode.switch(advancedButton, simpleButton, advancedContainer, simpleContainer, EditMode.Advanced));
    simpleButton.addEventListener('click', handlers.mode.switch(simpleButton, advancedButton, simpleContainer, advancedContainer, EditMode.Simple));
    
    handlers.createButton.init();
    
    const { dropArea, fileInput, uploadButton } = elements.getImageElements();
    addDropAreaEvents(entryId, dropArea, fileInput, uploadButton);
    handlers.paste.init();
};


export const adjustTextareaSizes = (editMode) => {
    dom.queryAll('textarea').forEach(adjustTextareaSize);
    
    const { advancedContainer, simpleContainer } = elements.getModeElements();
    const containerToHide = editMode === EditMode.Advanced ? simpleContainer : advancedContainer;
    uiState.toggleClasses(containerToHide, { add: ['hidden'] });
};