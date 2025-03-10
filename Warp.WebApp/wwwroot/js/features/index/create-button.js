import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { eventNames } from '/js/services/events.js';


export const createButton = {
    init: (elements) => {
        const createButton = elements.getCreateButton();
        const { advancedTextarea, simpleTextarea, advancedButton, simpleButton } = elements.getModeElements();

        const updateButtonState = () => {
            const isAdvancedMode = advancedButton.classList.contains(CSS_CLASSES.ACTIVE);
            const activeTextarea = isAdvancedMode ? advancedTextarea : simpleTextarea;
            uiState.setElementDisabled(createButton, !activeTextarea.value);
        };

        [advancedTextarea, simpleTextarea].forEach(textarea => {
            textarea.addEventListener('input', updateButtonState);
        });

        [advancedButton, simpleButton].forEach(button => {
            button.addEventListener('click', updateButtonState);
        });

        document.addEventListener(eventNames.uploadFinished, 
            () => uiState.setElementDisabled(createButton, false));

        updateButtonState();
    }
};