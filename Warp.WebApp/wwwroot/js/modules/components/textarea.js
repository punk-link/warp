import { remToPx } from '../utils/utils.js';

// on js input event
// document.getElementsByTagName("textarea")[0].dispatchEvent(new Event('input', { bubbles: true }));

const defaultHeightRem = 3;


function adjustInputHeight(e) {
  this.style.height = 'auto';
  this.style.height = (this.scrollHeight) + 'px';
}


function setInitialHeight(textarea) {
    let targetHeight = 0;
    
    if (textarea.value == '') {
        targetHeight = remToPx(defaultHeightRem);
    } else {
        targetHeight = textarea.scrollHeight;
    }

    textarea.setAttribute('style', 'height:' + (targetHeight) + 'px;overflow-y:hidden;');
}


export function addTextareaEvents() {
    let textareas = document.getElementsByTagName('textarea');
    for(let textarea of textareas) {
        setInitialHeight(textarea);

        textarea.addEventListener('input', adjustInputHeight, false);
    }
}