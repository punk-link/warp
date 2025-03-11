import { dom } from '/js/utils/ui-core.js';

export const elements = {
    getActionButtons: () => ({
        copyLink: dom.get('copy-link-button'),
        edit: dom.get('edit-button'),
        delete: dom.get('delete-button')
    }),
    getGalleryItems: () => dom.queryAll('[data-fancybox]'),
    getRoamingImage: () => dom.get('roaming-image')
};