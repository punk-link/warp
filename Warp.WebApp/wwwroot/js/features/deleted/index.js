import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { core } from '/js/core/initialize.js';
import { elements } from './elements.js';


core.initialize();

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


window.addDeletedEvents = addDeletedEvents;