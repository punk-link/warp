import { core } from '/js/core/initialize.js';
import { elements } from './elements.js';
import { PreviewPageController } from './preview-page-controller.js';


export const initPreviewPage = (entryId) => {
    try {
        core.initialize();

        let pageController = new PreviewPageController(elements);

        document.addEventListener('DOMContentLoaded', async () => {
            await pageController.initialize(entryId);
        });
    } catch (error) {
        console.error('Failed to initialize index page:', error);
    }
}


window.initPreviewPage = initPreviewPage;