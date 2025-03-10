import { applyFontScaling } from './font-scaler.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { EDIT_MODE } from '/js/constants/enums.js';
import { uiState } from '/js/utils/ui-core.js';


const CONFIG = Object.freeze({
    DEFAULT_HEIGHT_REMS: 2,
    PADDING_PX: 8,
    PROBE_CHAR: 'a'
});


const handlers = {
    textarea: (() => {
        const getRemSize = () => {
            const doc = document.documentElement;
            const fontSize = getComputedStyle(doc).fontSize;
            return parseFloat(fontSize);
        };

        const remToPx = (value) => {
            return value * getRemSize();
        }; 
        
        const createProbeSpan = (textarea) => {
            const span = document.createElement('span');

            span.style.visibility = 'hidden';
            span.style.whiteSpace = 'pre';
            span.style.font = window.getComputedStyle(textarea).font;
            span.textContent = CONFIG.PROBE_CHAR;

            return span;
        };

        const calculateCharactersPerRow = (textarea) => {
            const span = createProbeSpan(textarea);
            document.body.appendChild(span);

            const charWidth = span.getBoundingClientRect().width;
            document.body.removeChild(span);

            return Math.floor(textarea.clientWidth / charWidth);
        };

        const adjustHeight = (textarea) => {
            textarea.style.height = 'auto';
            textarea.style.height = `${textarea.scrollHeight}px`;
        };

        const setInitialHeight = (textarea) => {
            let targetHeight = remToPx(CONFIG.DEFAULT_HEIGHT_REMS);

            if (textarea.value !== '') {
                let rows = textarea.rows;

                if (textarea.clientWidth !== 0) {
                    const charactersPerRow = calculateCharactersPerRow(textarea);
                    rows = Math.ceil(textarea.textLength / charactersPerRow);
                }

                const lineHeight = parseInt(window.getComputedStyle(textarea).lineHeight);
                targetHeight = lineHeight * rows;
            }

            textarea.setAttribute(
                'style',
                `height:${targetHeight + CONFIG.PADDING_PX}px;overflow-y:hidden;`
            );
        };

        const toggleContainerVisibility = (elements, currentEditMode) => {
            const { advancedContainer, simpleContainer } = elements.getModeElements();
            const containerToHide = currentEditMode === EDIT_MODE.Advanced
                ? simpleContainer
                : advancedContainer;

            uiState.toggleClasses(containerToHide, { add: [CSS_CLASSES.HIDDEN] });
        };

        return {
            init: (textareas, elements, currentEditMode) => {
                textareas.forEach(textarea => { 
                    textarea.addEventListener('input', () => adjustHeight(textarea));

                    applyFontScaling(textarea);
                    setInitialHeight(textarea);
                });

                if (elements && currentEditMode !== undefined) 
                    toggleContainerVisibility(elements, currentEditMode);
            }
        };
    })()
};


export const adjustTextareaSizes = (textareas, elements, currentEditMode) => {
    handlers.textarea.init(textareas, elements, currentEditMode);
};