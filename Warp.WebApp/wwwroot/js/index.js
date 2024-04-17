import { applyFontScaling } from './modules/font-scaler.js';
import { addDropAreaEvents, pasteImages } from './modules/image-processor.js';
import { addTextareaEvents } from './modules/components/textarea.js';
    

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
    addTextareaEvents();
        
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
    //applyFontScaling(warpContentSpan, 1000, 250, 6, 1.25, 0.05);
}