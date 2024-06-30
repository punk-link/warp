import { addDropAreaEvents, pasteImages } from './modules/image-processor.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
    

function addPasteImageEventListener() {
    document.body.addEventListener('keydown', async function (e) {
    if (e.ctrlKey && (e.key === 'v' || e.key === 'V'))
        await pasteImages();
    });
}


function disableCreateButton(sendButton) {
    sendButton.disabled = true;
}


function enableCreateButton(sendButton) {
    sendButton.disabled = false;
}


function overrideFormSubmitEvent(form, sourceSpan, targetTextbox) {
    form.addEventListener('submit', function () {
        targetTextbox.value = sourceSpan.innerHTML;
    });
}


function addShowEntryContainerEvent(displayedButton, hiddenButton, displayedContainer, hiddenContainer) {
    displayedButton.addEventListener('click', function () {
        hiddenContainer.classList.add('d-none');

        hiddenButton.classList.remove('active');
        displayedButton.classList.add('active');

        displayedContainer.classList.remove('d-none');
    });
}


export function addIndexEvents() {
    let backgroundImageContainer = document.getElementById('roaming-image');
    repositionBackgroundImage(backgroundImageContainer);

    let advancedModeButton = document.getElementById('mode-advanced');
    let textModeButton = document.getElementById('mode-text');

    let advancedModeContainer = document.getElementById('advanced-mode');
    let textModeContainer = document.getElementById('text-mode');

    addShowEntryContainerEvent(advancedModeButton, textModeButton, advancedModeContainer, textModeContainer);
    addShowEntryContainerEvent(textModeButton, advancedModeButton, textModeContainer, advancedModeContainer);

    let textModeTextarea = document.getElementById('warp-text');
    let sendButton = document.getElementById('create-button');
    if (textModeTextarea.value !== '') {
        sendButton.disabled = false;
    }

    textModeTextarea.addEventListener('input', () => {
        if (textModeTextarea.value === '') {
            disableCreateButton(sendButton);
        } else {
            enableCreateButton(sendButton);
        }
    }, false);

    let dropArea = document.getElementsByClassName('drop-area')[0];
    let fileInput = document.getElementById('file');
    let uploadButton = document.getElementById('empty-image-container');
    addDropAreaEvents(dropArea, fileInput, uploadButton);

    //addPasteImageEventListener();
}