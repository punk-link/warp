import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { elements } from './elements.js';


const handlers = {
    actions: (() => {
        const handleCreate = () => {
            redirectTo(ROUTES.ROOT);
        };

        return {
            init: () => {
                const { create } = elements.getActionButtons();
                create.addEventListener('click', handleCreate);
            }
        };
    })()
};


export const addDeletedEvents = () => {
    handlers.actions.init();
};