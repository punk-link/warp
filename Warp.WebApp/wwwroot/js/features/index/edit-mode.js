import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { EDIT_MODE } from '/js/constants/enums.js';


const DISABLED_STATE_EXCLUDED_INPUT_IDS = new Set([
    'id',
    'create-button',
    'edit-mode-state', 
    'expiration-selector'
]);


const setContainerInputsState = (container, isDisabled) => {
    if (!container) return;
    
    const inputs = container.querySelectorAll('input, textarea, select');
    inputs.forEach(input => {
        if (DISABLED_STATE_EXCLUDED_INPUT_IDS.has(input.id)) return;
        uiState.setElementDisabled(input, isDisabled);
    });
};


export const editMode = {
    init: (editMode, elements) => {
        const { 
            advancedButton, 
            simpleButton,
            advancedContainer,
            simpleContainer 
        } = elements.getModeElements();

        const activeButton = editMode === EDIT_MODE.Advanced ? advancedButton : simpleButton;
        uiState.toggleClasses(activeButton, { add: [CSS_CLASSES.ACTIVE] });

        const activeContainer = editMode === EDIT_MODE.Advanced ? advancedContainer : simpleContainer;
        const inactiveContainer = editMode === EDIT_MODE.Advanced ? simpleContainer : advancedContainer;
        
        setContainerInputsState(inactiveContainer, true);
        setContainerInputsState(activeContainer, false);
    },

    switch: (displayedButton, hiddenButton, displayedContainer, hiddenContainer, mode, elements) => {
        return () => {
            uiState.toggleClasses(hiddenButton, { remove: [CSS_CLASSES.ACTIVE] });
            uiState.toggleClasses(displayedButton, { add: [CSS_CLASSES.ACTIVE] });
    
            uiState.toggleClasses(hiddenContainer, { add: [CSS_CLASSES.HIDDEN] });
            uiState.toggleClasses(displayedContainer, { remove: [CSS_CLASSES.HIDDEN] });

            setContainerInputsState(hiddenContainer, true);
            setContainerInputsState(displayedContainer, false);
            
            const { editModeInput } = elements.getModeElements();
            uiState.setElementValue(editModeInput, mode);
        };
    }
};