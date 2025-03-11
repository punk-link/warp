import { dom, uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';


const SELECTORS = Object.freeze({
    DISMISS: '[data-dismiss="modal"]',
    TOGGLE: '[data-toggle="modal"]'
});


const handlers = {
    modal: (() => {
        const registerDismissEvent = (modalElement, onDismiss) => {
            const dismissElement = modalElement.querySelector(SELECTORS.DISMISS);
            if (dismissElement)
                dismissElement.addEventListener('click', onDismiss);
        };

        const createModal = (modalElement) => {
            let isShown = true;

            const hide = () => {
                uiState.toggleClasses(modalElement, {
                    remove: [CSS_CLASSES.SHOW],
                    add: [CSS_CLASSES.FADE]
                });

                isShown = false;
            };

            const show = () => {
                uiState.toggleClasses(modalElement, {
                    remove: [CSS_CLASSES.FADE],
                    add: [CSS_CLASSES.SHOW]
                });

                isShown = true;
            };

            const toggle = () => {
                if (isShown) {
                    hide();
                } else {
                    show();
                }
            };

            hide();
            registerDismissEvent(modalElement, hide);

            return { hide, show, toggle };
        };

        return {
            init: () => {
                const toggleButtons = dom.queryAll(SELECTORS.TOGGLE);

                toggleButtons.forEach(button => {
                    const modalElement = dom.query(button.dataset.target);
                    if (!modalElement)
                        return;

                    const modal = createModal(modalElement);
                    button.addEventListener('click', () => { modal.toggle(); });
                });
            },

            create: (modalElement) => createModal(modalElement)
        };
    })()
};


document.addEventListener('DOMContentLoaded', handlers.modal.init);


export const modal = handlers.modal;