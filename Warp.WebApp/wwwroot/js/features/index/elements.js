import { dom } from '/js/utils/ui-core.js';


export const elements = {
    getId: () => dom.query('input[name="id"]'),
    getCreateButton: () => dom.get('create-button'),
    getExpirationSelector: () => dom.get('expiration-selector'),
    getForm: () => dom.get('entry-form'),
    getModeElements: () => ({
        advancedButton: dom.get('advanced-mode-nav-button'),
        simpleButton: dom.get('simple-mode-nav-button'),
        advancedContainer: dom.get('advanced-mode-container'),
        simpleContainer: dom.get('simple-mode-container'),
        advancedTextarea: dom.get('advanced-text-content'),
        simpleTextarea: dom.get('simple-text-content'),
        editModeInput: dom.get('edit-mode-state')
    }),
    getRoamingImage: () => dom.get('roaming-image'),
    getTextareas: () => dom.queryAll('textarea'),
    getGallery: () => dom.query('.gallery'),
    getActualImageContainers: () => {
        const gallery = elements.getGallery();
        return gallery ? gallery.querySelectorAll('.image-container:not(#empty-image-container)') : [];
    }
};