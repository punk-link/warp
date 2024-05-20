import { applyFontScaling } from '../functions/font-scaler.js';
import { remToPx } from '../functions/utils.js';


const DEFAULT_HEIGHT_REMS = 2;


function adjustInputHeight(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
}


function setInitialHeight(textarea) {
    let targetHeight = 0;
    
    if (textarea.value == '') 
        targetHeight = remToPx(DEFAULT_HEIGHT_REMS);
    else 
        targetHeight = textarea.scrollHeight;

    textarea.setAttribute('style', 'height:' + (targetHeight + 8) + 'px;overflow-y:hidden;');
}



document.addEventListener('DOMContentLoaded', () => {
    let textareas = document.getElementsByTagName('textarea');
    for (let textarea of textareas) {
        applyFontScaling(textarea);
        setInitialHeight(textarea);

        textarea.addEventListener('input', function () {
            adjustInputHeight(textarea);
        }, false);
    }
});