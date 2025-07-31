import { dom } from '/js/utils/ui-core.js';

export const elements = {
    getActionButtons: () => ({
        copyLink: dom.get('copy-link-button'),
        delete: dom.get('delete-button'),
        edit: dom.get('edit-entry-button'),
        editCopy: dom.get('edit-button')
    }),

    getGallery: () => dom.query('.gallery'),
    
    getRoamingImage: () => dom.get('roaming-image'),
    
    getServiceMessages: () => ({
        created: dom.query('.service-message-container #entry-created-message'),
        copied: dom.query('.service-message-container #link-copied-message')
    }),
    
    getEditEntryButton: () => dom.get('edit-entry-button'),
    
    getTextContentElement: () => dom.query('.text-content'),
    
    getCopyForm: () => dom.get('copy-form'),
    
    getEditForm: () => dom.get('edit-form')
};