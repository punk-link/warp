import { dom, uiState } from '/js/utils/ui-core.js';
import { ROUTES, redirectTo } from '/js/utils/routes.js';
import { copyUrl } from '/js/functions/copier.js';
import { initializeCountdown } from '/js/components/countdown.js';
import { repositionBackgroundImage } from '/js/functions/image-positioner.js';
import { makeHttpRequest, DELETE } from '/js/functions/http-client.js';


const elements = {
    getActionButtons: () => ({
        copyLink: dom.get('copy-link-button'),
        edit: dom.get('edit-button'),
        delete: dom.get('delete-button')
    }),
    getGalleryItems: () => dom.queryAll('[data-fancybox]'),
    getRoamingImage: () => dom.get('roaming-image')
};


const handlers = {
    actions: (() => {
        const handleError = async (response) => {
            const problemDetails = await response.json();
            redirectTo(ROUTES.ERROR, { 
                details: JSON.stringify(problemDetails) 
            });
        };

        const handleDelete = async (entryId) => {
            const response = await makeHttpRequest(`${ROUTES.ENTRY}/${entryId}`, DELETE);
            if (response.ok)
                return redirectTo(ROUTES.DELETED);

            if (!(response.ok && response.redirected)) 
                await handleError(response);
        };

        const handleCopyLink = (entryId, editButton) => {
            const entryUrl = `${window.location.origin}${ROUTES.ENTRY}/${entryId}`;
            copyUrl(entryUrl);
            
            uiState.toggleClasses(editButton, { 
                remove: ['hidden'], 
                add: ['catchy-fade-in'] 
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

    gallery: {
        init: () => {
            const galleryItems = elements.getGalleryItems();
            if (!galleryItems.length) 
                return;

            Fancybox.bind("[data-fancybox]", { caption: (fancybox, slide) => slide.thumbEl?.alt || "" });
        }
    },

    background: {
        init: () => {
            const roamingImage = elements.getRoamingImage();
            if (!roamingImage) 
                return;

            repositionBackgroundImage(roamingImage);
        }
    },

    countdown: {
        init: (expirationDate) => {
            initializeCountdown(expirationDate);
        }
    }
};


export const addPreviewEvents = (entryId, expirationDate) => {
    handlers.background.init();
    handlers.countdown.init(expirationDate);
    handlers.actions.init(entryId);
    handlers.gallery.init();
};