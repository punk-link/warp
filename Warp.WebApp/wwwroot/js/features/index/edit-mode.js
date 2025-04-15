import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { EDIT_MODE } from '/js/constants/enums.js';


const DISABLED_STATE_EXCLUDED_INPUT_IDS = new Set([
    'id',
    'create-button',
    'edit-mode-state', 
    'expiration-selector'
]);


const STORAGE_KEY = 'warp_edit_mode';


const saveEditModeToStorage = (mode) => {
    try {
        localStorage.setItem(STORAGE_KEY, mode);
    } catch (e) {
        console.warn('Failed to save edit mode to localStorage:', e);
    }
};


const loadEditModeFromStorage = () => {
    try {
        const savedMode = localStorage.getItem(STORAGE_KEY);
        if (savedMode !== null) {
            const parsedMode = parseInt(savedMode, 10);

            if (parsedMode === EDIT_MODE.Advanced) 
                return EDIT_MODE.Advanced;
        }

        return EDIT_MODE.Simple;
    } catch (e) {
        console.warn('Failed to load edit mode from localStorage:', e);
        return EDIT_MODE.Simple;
    }
};


const setContainerInputsState = (container, isDisabled) => {
    if (!container) 
        return;
    
    const inputs = container.querySelectorAll('input, textarea, select');
    inputs.forEach(input => {
        if (DISABLED_STATE_EXCLUDED_INPUT_IDS.has(input.id)) 
            return;
        
        uiState.setElementDisabled(input, isDisabled);
    });
};


const determineActiveElements = (mode, elements) => {
    const { advancedButton, simpleButton, advancedContainer, simpleContainer } = elements.getModeElements();

    if (mode === EDIT_MODE.Advanced) {
        return {
            activeButton: advancedButton,
            inactiveButton: simpleButton,
            activeContainer: advancedContainer,
            inactiveContainer: simpleContainer
        };
    } else {
        return {
            activeButton: simpleButton,
            inactiveButton: advancedButton,
            activeContainer: simpleContainer,
            inactiveContainer: advancedContainer
        };
    }
};

function applyModeUIState(mode, elements) {
    const { activeButton, inactiveButton, activeContainer, inactiveContainer } = determineActiveElements(mode, elements);
        
    uiState.toggleClasses(activeButton, { add: [CSS_CLASSES.ACTIVE] });
    uiState.toggleClasses(inactiveButton, { remove: [CSS_CLASSES.ACTIVE] });

    uiState.toggleClasses(activeContainer, { remove: [CSS_CLASSES.HIDDEN] });
    uiState.toggleClasses(inactiveContainer, { add: [CSS_CLASSES.HIDDEN] });

    setContainerInputsState(inactiveContainer, true);
    setContainerInputsState(activeContainer, false);

    const { editModeInput } = elements.getModeElements();
    uiState.setElementValue(editModeInput, mode);
}


export const editMode = {
    init: (editMode, elements) => {
        const modeToUse = editMode === EDIT_MODE.Unset ? loadEditModeFromStorage() : editMode;
        applyModeUIState(modeToUse, elements);
    },

    switch: (mode, elements) => {
        return () => {
            applyModeUIState(mode, elements);
            saveEditModeToStorage(mode);
        };
    }
};
