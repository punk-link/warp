﻿import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { http } from '/js/services/http/client.js';
import { dispatchEvent } from '/js/services/events.js';
import { ROUTES } from '/js/utils/routes.js';
import { elements } from './elements.js';


const SELECTORS = {
    IMAGE_CONTAINER: '.image-container',
    DELETE_BUTTON: '.delete-image-button'
};


const handlers = {
    preview: (() => {
        const parseResult = result => {
            try {
                const parsed = JSON.parse(result);
                return (parsed?.title && parsed?.detail)
                    ? parsed
                    : null;
            } catch {
                return null;
            }
        };

        const createPreview = (template, imageUrl) => {
            const parser = new DOMParser();
            const container = parser.parseFromString(template, 'text/html')
                .querySelector(SELECTORS.IMAGE_CONTAINER);

            const image = document.createElement('img');
            image.src = imageUrl;

            elements.getImageContainer(container).image.replaceWith(image);
            return container;
        };

        const deleteImage = async (e, entryId) => {
            const container = e.target.closest(SELECTORS.IMAGE_CONTAINER);
            const { imageId } = elements.getImageContainer(container);

            const response = await http.delete(ROUTES.API.IMAGES.DELETE(entryId, imageId));
            if (response.ok) {
                container.remove();
                dispatchEvent.imageDeleted();
            }
        };

        const attachEventHandlers = (container, entryId) => {
            const deleteButton = container.querySelector(SELECTORS.DELETE_BUTTON);
            if (deleteButton && !deleteButton._hasDeleteHandler) {
                deleteButton.addEventListener('click', e => deleteImage(e, entryId));
                deleteButton._hasDeleteHandler = true;
            }
        };

        const animateContainer = (container, gallery) => {
            const emptyContainer = elements.getUploadElements().uploadContainer;
            if (emptyContainer && emptyContainer.classList.contains(CSS_CLASSES.HIDDEN)) 
                emptyContainer.classList.remove(CSS_CLASSES.HIDDEN);

            uiState.toggleClasses(container, { add: [CSS_CLASSES.HIDDEN] });

            if (emptyContainer && gallery.contains(emptyContainer)) {
                gallery.insertBefore(container, emptyContainer);
            } else {
                gallery.prepend(container);
            }

            uiState.toggleClasses(container, {
                remove: [CSS_CLASSES.HIDDEN],
                add: [CSS_CLASSES.ANIMATE]
            });
        };

        const attachEventsToPreloadedImages = (entryId) => {
            const gallery = elements.getGallery();
            if (!gallery) 
                return;

            const preloadedContainers = gallery.querySelectorAll(SELECTORS.IMAGE_CONTAINER);
            preloadedContainers.forEach(container => {
                attachEventHandlers(container, entryId);
            });
        };

        return {
            render: (entryId, files, results, onDelete) => {
                const gallery = elements.getGallery();

                files.forEach(file => {
                    const reader = new FileReader();
                    reader.onloadend = () => {
                        const result = results[file.name];
                        if (!result)
                            return;

                        const error = parseResult(result);
                        if (error) {
                            console.error(`Error: ${error.title} - ${error.detail}`);
                            return;
                        }

                        const container = createPreview(result, reader.result);

                        attachEventHandlers(container, entryId, onDelete);
                        animateContainer(container, gallery);
                    };

                    reader.readAsDataURL(file);
                });
            },

            initPreloadedImages: (entryId) => {
                attachEventsToPreloadedImages(entryId);
            }
        };
    })()
};


export const preview = handlers.preview;