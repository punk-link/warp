import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { initializeCountdown } from '/js/components/countdown.js';
import { galleryViewer } from '/js/components/gallery/viewer.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { http } from '/js/services/http/client.js';
import { modal } from '/js/components/modal.js';
import { core } from '/js/core/initialize.js';
import { elements } from './elements.js';


core.initialize();

const handlers = {
    actions: (() => {
        const handleClose = () => {
            redirectTo(ROUTES.ROOT);
        };

        const handleCopyLink = async (entryId) => {
            const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
            const success = await copyUrl(entryUrl);
        };

        return {
            init: (entryId) => {
                const { close, copyLink } = elements.getActionButtons();

                close.addEventListener('click', handleClose);
                copyLink?.addEventListener('click', () => handleCopyLink(entryId));
            }
        };
    })(),

    modal: (() => {
        const handleReport = async (entryId) => {
            const response = await http.post(ROUTES.API.ENTRIES.REPORT(entryId));
            if (response.status === 204)
                redirectTo(ROUTES.ROOT);
        };

        return {
            init: (entryId) => {
                const { reportModal, reportButton, cancelButton } = elements.getModalElements();
                const { showReportModalButton } = elements.getActionButtons();

                if (reportModal && showReportModalButton) {
                    const reportModalControl = modal.create(reportModal);

                    showReportModalButton.addEventListener('click', () => {
                        reportModalControl.show();
                    });

                    if (cancelButton) {
                        cancelButton.addEventListener('click', () => {
                            reportModalControl.hide();
                        });
                    }

                    if (reportButton) {
                        reportButton.addEventListener('click', () => handleReport(entryId));
                    }
                }
            }
        };
    })(),

    gallery: (() => {
        return {
            init: () => {
                const galleryItems = elements.getGalleryItems();
                if (!galleryItems?.length)
                    return;

                galleryViewer.init();
            }
        };
    })(),

    background: (() => {
        return {
            init: () => {
                const roamingImage = elements.getRoamingImage();
                if (!roamingImage)
                    return;

                animateBackgroundImage(roamingImage);
            }
        };
    })(),

    countdown: (() => {
        return {
            init: (expirationDate) => {
                if (!expirationDate)
                    return;

                initializeCountdown(expirationDate);
            }
        };
    })()
};


export const addEntryEvents = (entryId, expirationDate) => {
    handlers.background.init();
    handlers.countdown.init(expirationDate);
    handlers.gallery.init();
    handlers.actions.init(entryId);
    handlers.modal.init(entryId);
};


window.addEntryEvents = addEntryEvents;