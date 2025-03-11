import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { initializeCountdown } from '/js/components/countdown.js';
import { galleryViewer } from '/js/components/gallery/viewer.js';
import { repositionBackgroundImage } from '/js/components/background/image-positioner.js';
import { http } from '/js/services/http/client.js';
import { elements } from './elements.js';


const handlers = {
    actions: (() => {
        const handleReport = async (entryId) => {
            const response = await http.post(`/api/entries/${entryId}/report`);

            if (response.status === 204)
                redirectTo(ROUTES.ROOT);
        };

        const handleClose = () => {
            redirectTo(ROUTES.ROOT);
        };

        return {
            init: (entryId) => {
                const { close, report } = elements.getActionButtons();

                close.addEventListener('click', handleClose);
                report.addEventListener('click', () => handleReport(entryId));
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

                repositionBackgroundImage(roamingImage);
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
};