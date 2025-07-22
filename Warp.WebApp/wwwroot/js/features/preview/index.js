import { dom, uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { copyUrl } from '/js/utils/clipboard.js';
import { initializeCountdown } from '/js/components/countdown.js'; 
import { galleryViewer } from '/js/components/gallery/viewer.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { http } from '/js/services/http/client.js';
import { core } from '/js/core/initialize.js';
import { elements } from './elements.js';


core.initialize();


let pageController = null;


export const addPreviewEvents = async (entryId) => {
    try {
        pageController = new PreviewPageController(entryId, elements);
        await pageController.initialize();
    } catch (error) {
        console.error('Failed to initialize index page:', error);
    }
}


const handlers = {
    actions: (() => {
        const handleError = async (response) => {
            const problemDetails = await response.json();
            redirectTo(ROUTES.ERROR, {
                details: JSON.stringify(problemDetails)
            });
        };

        const handleDelete = async (entryId) => {
            const response = await http.delete(ROUTES.API.ENTRIES.DELETE(entryId));

            if (response.ok)
                return redirectTo(ROUTES.DELETED);

            if (!(response.ok && response.redirected))
                await handleError(response);
        };

        const handleCopyLink = async (entryId, editButton) => {
            const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
            const success = await copyUrl(entryUrl);

            if (success) {
                uiState.toggleClasses(editButton, {
                    remove: [CSS_CLASSES.HIDDEN],
                    add: [CSS_CLASSES.ANIMATE]
                });

                const { created, copied } = elements.getServiceMessages();
                if (created && copied) {
                    uiState.toggleClasses(created, { add: [CSS_CLASSES.HIDDEN] });
                    uiState.toggleClasses(copied, {
                        remove: [CSS_CLASSES.HIDDEN],
                        add: [CSS_CLASSES.ANIMATE, CSS_CLASSES.ANIMATE_SLOW_OUT]
                    });

                    setTimeout(() => {
                        uiState.toggleClasses(copied, {
                            add: [CSS_CLASSES.HIDDEN],
                            remove: [CSS_CLASSES.ANIMATE]
                        });
                        uiState.toggleClasses(created, { 
                            remove: [CSS_CLASSES.HIDDEN], 
                            add: [CSS_CLASSES.ANIMATE_SLOW]
                        });
                    }, 4000);
                }
            }
        };

        return {
            init: (entryId) => {
                const { copyLink, edit, delete: deleteButton } = elements.getActionButtons();

                copyLink.addEventListener('click', () => handleCopyLink(entryId, edit));
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


//export const addPreviewEvents = (entryId, expirationDate) => {
//    handlers.background.init();
//    handlers.countdown.init(expirationDate);
//    handlers.actions.init(entryId);
//    handlers.gallery.init();
//};


window.addPreviewEvents = addPreviewEvents;