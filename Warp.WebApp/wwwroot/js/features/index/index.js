import { core } from '/js/core/initialize.js';
import { IndexPageController } from './index-page-controller.js';
import { elements } from './elements.js';
import { creatorApi } from '/js/api/creator-api.js';
import { entryApi } from '/js/api/entry-api.js';
import { metadataApi } from '/js/api/metadata-api.js';


export const initIndexPage = (entryId) => {
    try {
        core.initialize();

        let pageController = new IndexPageController(elements, creatorApi, entryApi, metadataApi);

        document.addEventListener('DOMContentLoaded', async () => {
            await pageController.initialize(entryId);

            window.addEventListener('beforeunload', () => {
                pageController.cleanup();
            });
        });

        window.addEventListener('load', () => {
            const editMode = document.getElementById('edit-mode-state').value;
            pageController.setupTextareas(editMode);
        });
    } catch (error) {
        console.error('Failed to initialize index page:', error);
    }
}


window.initIndexPage = initIndexPage;