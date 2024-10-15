import { applyFontScaling } from '../functions/font-scaler.js';
import { remToPx } from '../functions/utils.js';


const DEFAULT_HEIGHT_REMS = 2;


function adjustInputHeight(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
}


function buildProbeSpan(textarea) {
    let span = document.createElement('span');
    span.style.visibility = 'hidden';
    span.style.whiteSpace = 'pre';
    span.style.font = window.getComputedStyle(textarea).font;

    // This is the character that will be used to calculate the width of a character
    span.textContent = 'a';

    return span;
}


function getCharactersPerRow(textarea) {
    let span = buildProbeSpan(textarea);
    document.body.appendChild(span);

    let charWidth = span.getBoundingClientRect().width;

    document.body.removeChild(span);

    return Math.floor(textarea.clientWidth / charWidth);
}


function setInitialHeight(textarea) {
    let targetHeight = remToPx(DEFAULT_HEIGHT_REMS);
    
    if (textarea.value !== '') {
        let rows = textarea.rows;
        if (textarea.clientWidth !== 0) {
            let charactersPerRow = getCharactersPerRow(textarea);
            rows = Math.floor(textarea.textLength / charactersPerRow);
        }

        let lineHeight = parseInt(window.getComputedStyle(textarea).lineHeight);
        targetHeight = lineHeight * rows;
    }

    textarea.setAttribute('style', 'height:' + (targetHeight + 8) + 'px;overflow-y:hidden;');
}


export function adjustTextareaSize(textarea) {
    textarea.addEventListener('input', function () {
        adjustInputHeight(textarea);
    }, false);
        
    applyFontScaling(textarea);
    setInitialHeight(textarea);
}
