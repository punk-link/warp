import { eventNames } from'/js/events/events.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
import { addDropAreaEvents, pasteImages } from './modules/image-processor.js';


const EditMode = { Advanced: 2, Text: 1 };


function addPasteImageEventListener() {
    document.body.addEventListener('keydown', async function (e) {
    if (e.ctrlKey && (e.key === 'v' || e.key === 'V'))
        await pasteImages();
    });
}


function addEntryContainerEvents(advancedModeButton, textModeButton) {
    let advancedModeContainer = document.getElementById('advanced-mode');
    let textModeContainer = document.getElementById('text-mode');

    addShowEntryContainerEvent(advancedModeButton, textModeButton, advancedModeContainer, textModeContainer, EditMode.Advanced);
    addShowEntryContainerEvent(textModeButton, advancedModeButton, textModeContainer, advancedModeContainer, EditMode.Text);
}


function addShowEntryContainerEvent(displayedButton, hiddenButton, displayedContainer, hiddenContainer, mode) {
    let editModeInput = document.getElementById('edit-mode-state');
    
    displayedButton.addEventListener('click', () => {
            hiddenContainer.classList.add('d-none');
            hiddenButton.classList.remove('active');

            displayedButton.classList.add('active');
            displayedContainer.classList.remove('d-none');

            editModeInput.value = mode;
            console.log('edit mode: ' + editModeInput.value);
        });
}


function addCreateButtonEvents(advancedModeButton, textModeButton) {
    let advancedModeTextarea = document.getElementById('warp-advanced');
    let textModeTextarea = document.getElementById('warp-text');
    
    let createButton = document.getElementById('create-button');
    if (advancedModeTextarea.value !== '' && textModeTextarea.value !== '') 
        createButton.disabled = false;

    toggleCreateButtonState(createButton, advancedModeButton, advancedModeTextarea);
    toggleCreateButtonState(createButton, textModeButton, textModeTextarea);

    document.addEventListener(eventNames.uploadFinished, () => {
            createButton.disabled = false;
        });
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


export function addIndexEvents() {
    let backgroundImageContainer = document.getElementById('roaming-image');
    repositionBackgroundImage(backgroundImageContainer);

    let advancedModeButton = document.getElementById('mode-advanced');
    let textModeButton = document.getElementById('mode-text');

    addEntryContainerEvents(advancedModeButton, textModeButton);
    addCreateButtonEvents(advancedModeButton, textModeButton);    

    let dropArea = document.getElementsByClassName('drop-area')[0];
    let fileInput = document.getElementById('file');
    let uploadButton = document.getElementById('empty-image-container');
    
    addDropAreaEvents(dropArea, fileInput, uploadButton);
    addPasteImageEventListener();
}