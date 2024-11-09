import { eventNames } from '/js/events/events.js';
import { adjustTextareaSize } from '/js/components/textarea.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
import { addDropAreaEvents, pasteImages } from './modules/image-processor.js';


const EditMode = { 
    Unknown: 0, 
    Text: 1, 
    Advanced: 2
};


function addPasteImageEventListener() {
    document.body.addEventListener('keydown', async function (e) {
    if (e.ctrlKey && (e.key === 'v' || e.key === 'V'))
        await pasteImages();
    });
}


function addEntryContainerEvents(advancedModeButton, textModeButton, editMode) {
    let advancedModeContainer = document.getElementById('advanced-mode');
    let textModeContainer = document.getElementById('text-mode');

    addShowEntryContainerEvent(advancedModeButton, textModeButton, advancedModeContainer, textModeContainer, EditMode.Advanced);
    addShowEntryContainerEvent(textModeButton, advancedModeButton, textModeContainer, advancedModeContainer, EditMode.Text);
}


function addShowEntryContainerEvent(displayedButton, hiddenButton, displayedContainer, hiddenContainer, mode) {
    let editModeInputs = document.getElementsByClassName('edit-mode-state');
    
    for (let editModeInput of editModeInputs) {
        displayedButton.addEventListener('click', () => {
            hiddenContainer.classList.add('d-none');
            hiddenButton.classList.remove('active');

            displayedButton.classList.add('active');
            displayedContainer.classList.remove('d-none');

            editModeInput.value = mode;
        });
    }
}


function addCreateButtonEvents(advancedModeButton, textModeButton) {
    let advancedModeTextarea = document.getElementById('warp-advanced');
    let textModeTextarea = document.getElementById('warp-text');
    
    let createButtons = document.getElementsByClassName('create-button');
    for (let createButton of createButtons) {
        if (advancedModeTextarea.value !== '' && textModeTextarea.value !== '') 
            createButton.disabled = false;

        toggleCreateButtonState(createButton, advancedModeButton, advancedModeTextarea);
        toggleCreateButtonState(createButton, textModeButton, textModeTextarea);

        document.addEventListener(eventNames.uploadFinished, () => {
            createButton.disabled = false;
        });
    }
}


function initializeEditModes(editMode) {
    let advancedModeButton = document.getElementById('mode-advanced');
    let textModeButton = document.getElementById('mode-text');

    switch (editMode) {
        case EditMode.Advanced:
            advancedModeButton.classList.add('active');
            break;
        case EditMode.Unknown:
        case EditMode.Text:
            textModeButton.classList.add('active');
            break;
    }

    addEntryContainerEvents(advancedModeButton, textModeButton, editMode);
    addCreateButtonEvents(advancedModeButton, textModeButton);    
}


function toggleCreateButtonState(createButton, targetModeButton, targetTextarea) {
    targetTextarea.addEventListener('input', () => {
        if (!targetModeButton.classList.contains('active'))
            return;

        if (targetTextarea.value === '') 
            createButton.disabled = true;
        else 
            createButton.disabled = false;
    }, false);
}


export function addIndexEvents(entryId, editMode) {
    let backgroundImageContainer = document.getElementById('roaming-image');
    repositionBackgroundImage(backgroundImageContainer);

    initializeEditModes(editMode);

    let dropArea = document.getElementsByClassName('drop-area')[0];
    let fileInput = document.getElementById('file');
    let uploadButton = document.getElementById('empty-image-container');
    
    addDropAreaEvents(entryId, dropArea, fileInput, uploadButton);
    addPasteImageEventListener(entryId);
}


export function adjustTextareaSizes(editMode) {
    let textareas = document.getElementsByTagName('textarea');
    for (let textarea of textareas) {
        adjustTextareaSize(textarea);
    }

    let advancedModeContainer = document.getElementById('advanced-mode');
    let textModeContainer = document.getElementById('text-mode');

    switch (editMode) {
        case EditMode.Advanced:
            textModeContainer.classList.add('d-none');
            break;
        case EditMode.Unknown:
        case EditMode.Text:
            advancedModeContainer.classList.add('d-none');
            break;
    }
}