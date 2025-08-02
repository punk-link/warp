import { core } from '/js/core/initialize.js';
import { EntryPageController } from './entry-page-controller.js';
import { elements } from './elements.js';


export const initEntryPage = (entryId) => {
    core.initialize();
    
    const controller = new EntryPageController(elements);
    controller.initialize(entryId);
};

window.initEntryPage = initEntryPage;