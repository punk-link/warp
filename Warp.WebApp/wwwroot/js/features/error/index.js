﻿import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { copyText } from '/js/utils/clipboard.js';
import { elements } from './elements.js';


const handlers = {
    clipboard: (() => {
        const handleCopy = async () => {
            const { code, tooltip } = elements.getRequestId();
            if (!code)
                return;

            const success = await copyText(code);
            if (success) {
                uiState.toggleClasses(tooltip, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE]
                });

                uiState.toggleClasses(code, {
                    add: [CSS_CLASSES.SUCCESS.BG, CSS_CLASSES.SUCCESS.BORDER]
                });

                setTimeout(() => {
                    uiState.toggleClasses(tooltip, {
                        add: [CSS_CLASSES.HIDDEN]
                    });

                    uiState.toggleClasses(code, {
                        remove: [CSS_CLASSES.SUCCESS.BG, CSS_CLASSES.SUCCESS.BORDER]
                    });
                }, 2000);
            }
        };

        return {
            init: () => {
                const { code } = elements.getRequestId();
                if (code) {
                    code.addEventListener('click', handleCopy);
                }
            }
        };
    })()
};


export const addErrorEvents = () => {
    handlers.clipboard.init();
};