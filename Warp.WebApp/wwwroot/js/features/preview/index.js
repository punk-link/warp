import { dom, uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { initializeCountdown } from '/js/components/countdown.js'; 
import { galleryViewer } from '/js/components/gallery/viewer.js';
import { repositionBackgroundImage } from '/js/components/background/image-positioner.js';
import { http } from '/js/services/http/client.js';
import { elements } from './elements.js';


const handlers = {
    actions: (() => {
        const handleError = async (response) => {
            const problemDetails = await response.json();
            redirectTo(ROUTES.ERROR, {
                details: JSON.stringify(problemDetails)
            });
        };

        const handleDelete = async (entryId) => {
            const response = await http.delete(`${ROUTES.ENTRY}/${entryId}`);

            if (response.ok)
                return redirectTo(ROUTES.DELETED);

            if (!(response.ok && response.redirected))
                await handleError(response);
        };

        const handleCopyLink = (entryId, editButton) => {
            const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
            copyUrl(entryUrl);

            uiState.toggleClasses(editButton, {
                remove: [CSS_CLASSES.HIDDEN],
                add: [CSS_CLASSES.ANIMATE]
            });
        };

        return {
            init: (entryId) => {
                const { copyLink, edit, delete: deleteButton } = elements.getActionButtons();

                copyLink.addEventListener('click', () => handleCopyLink(entryId, edit));
                edit.addEventListener('click', () => redirectTo(ROUTES.ROOT, { id: entryId }));
                deleteButton.addEventListener('click', () => handleDelete(entryId));
            }
        };
    })(),

    gallery: (() => {
        return {
            init: () => {
                const galleryItems = elements.getGalleryItems();
                if (!galleryItems.length)
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


export const addPreviewEvents = (entryId, expirationDate) => {
    handlers.background.init();
    handlers.countdown.init(expirationDate);
    handlers.actions.init(entryId);
    handlers.gallery.init();
};