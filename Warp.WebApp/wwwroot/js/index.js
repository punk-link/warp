import { addDropAreaEvents, pasteImages } from './modules/image-processor.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
    

function addPasteImageEventListener() {
    document.body.addEventListener('keydown', async function (e) {
    if (e.ctrlKey && (e.key === 'v' || e.key === 'V'))
        await pasteImages();
    });
}


function disableCreateButton() {
    let sendButton = document.getElementById('create-button');
    sendButton.disabled = true;
}


function enableCreateButton() {
    let sendButton = document.getElementById('create-button');
    sendButton.disabled = false;
}


function overrideFormSubmitEvent(form, sourceSpan, targetTextbox) {
    form.addEventListener('submit', function () {
        targetTextbox.value = sourceSpan.innerHTML;
    });
}


export function addIndexEvents() {
    let backgroundImageContainer = document.getElementById('roaming-image');
    repositionBackgroundImage(backgroundImageContainer);

    let textModeButton = document.getElementById('warp-text');
    textModeButton.classList.add('active');

    let textModeTextarea = document.getElementById('warp-text');
    textModeTextarea.addEventListener('input', () => {
        if (textModeTextarea.value == '') {
            disableCreateButton();
        } else {
            enableCreateButton();
        }
    }, false);

    //addPasteImageEventListener();

    //let dropArea = document.getElementsByClassName('drop-area')[0];
    //let fileInput = document.getElementById('file');
    //let uploadButton = document.getElementById('upload-button');
    //addDropAreaEvents(dropArea, fileInput, uploadButton);

    //let warpContentTextarea = document.getElementById('warp-content-textarea');
    //let warpContentForm = document.getElementsByTagName('form')[0];
    //let warpContentSpan = document.getElementById('warp-content-textarea-span');
    
    //overrideFormSubmitEvent(warpContentForm, warpContentSpan, warpContentTextarea);
}