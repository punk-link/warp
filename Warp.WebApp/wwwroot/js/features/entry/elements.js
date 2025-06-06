﻿import { dom } from '/js/utils/ui-core.js';


export const elements = {
    getActionButtons: () => ({
        close: dom.get('page-close-button'),
        showReportModalButton: dom.get('show-report-modal-button'),
        copyLink: dom.get('copy-link-button')
    }),
    getModalElements: () => ({
        reportModal: dom.get('report-modal'),
        reportButton: dom.get('report-button'),
        cancelButton: dom.query('#report-modal [data-dismiss="modal"]')
    }),
    getGalleryItems: () => dom.queryAll('[data-fancybox]'),
    getRoamingImage: () => dom.get('roaming-image'),
    getCountdownElement: () => dom.query('.countdown')
};