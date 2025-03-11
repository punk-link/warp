import { dom } from '/js/utils/ui-core.js';


export const elements = {
    getActionButtons: () => ({
        close: dom.get('page-close-button'),
        report: dom.get('report-button')
    }),
    getGalleryItems: () => dom.queryAll('[data-fancybox]'),
    getRoamingImage: () => dom.get('roaming-image'),
    getCountdownElement: () => dom.query('.countdown')
};