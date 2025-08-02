import { uiState } from '/js/utils/ui-core.js';
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

        const triggerContainerAnimation = (imageContainer) => {
            uiState.toggleClasses(imageContainer, {
                remove: [CSS_CLASSES.HIDDEN],
                add: [CSS_CLASSES.ANIMATE]
            });
        };

        const animateContainerImmediately = (imageContainer) => {
            requestAnimationFrame(() => {
                triggerContainerAnimation(imageContainer);
            });
        };

        const handleAlreadyLoadedImage = (imageContainer, imageElement) => {
            if (imageElement.complete && imageElement.naturalHeight !== 0) {
                animateContainerImmediately(imageContainer);
                return true;
            }

            return false;
        };

        const attachImageLoadHandlers = (imageContainer, imageElement) => {
            const cleanup = (loadHandler, errorHandler) => {
                imageElement.removeEventListener('load', loadHandler);
                imageElement.removeEventListener('error', errorHandler);
            };

            const handleImageLoad = () => {
                triggerContainerAnimation(imageContainer);
                cleanup(handleImageLoad, handleImageError);
            };
            
            const handleImageError = () => {
                // Even if image fails to load, show the container
                triggerContainerAnimation(imageContainer);
                cleanup(handleImageLoad, handleImageError);
            };
            
            imageElement.addEventListener('load', handleImageLoad);
            imageElement.addEventListener('error', handleImageError);
        };

        const processImageElement = (imageContainer, imageElement) => {
            if (!imageElement) {
                animateContainerImmediately(imageContainer);
                return;
            }

            const isAlreadyLoaded = handleAlreadyLoadedImage(imageContainer, imageElement);
            if (!isAlreadyLoaded) 
                attachImageLoadHandlers(imageContainer, imageElement);
        };

        const parseContainerFromHtml = (containerHtml) => {
            const parser = new DOMParser();
            const tempContainer = parser.parseFromString(containerHtml, 'text/html');

            return tempContainer.querySelector('.image-container');
        };

        const animateReadOnlyContainer = (containerHtml, gallery) => {
            const imageContainer = parseContainerFromHtml(containerHtml);
            
            if (!imageContainer) 
                return null;
            
            uiState.toggleClasses(imageContainer, { add: [CSS_CLASSES.HIDDEN] });
            gallery.appendChild(imageContainer);
            
            const imageElement = imageContainer.querySelector('img');
            processImageElement(imageContainer, imageElement);
            
            return imageContainer;
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
            render: async (entryId, files, results, onDelete) => {
                const gallery = elements.getGallery();

                for (const file of files) {
                    const readFileAsync = () => new Promise((resolve, reject) => {
                        const reader = new FileReader();

                        reader.onloadend = () => resolve(reader.result);
                        reader.onerror = reject;
                        reader.readAsDataURL(file);
                    });

                    try {
                        const imageUrl = await readFileAsync();

                        const result = results.find(r => r.key === file.name)?.value;
                        if (!result)
                            continue;

                        const error = parseResult(result);
                        if (error) {
                            console.error(`Error: ${error.title} - ${error.detail}`);
                            continue;
                        }

                        const templateResponse = await http.get(result.partialUrl);
                        if (!templateResponse.ok) {
                            console.error(`Failed to fetch template for image: ${file.name}`);
                            continue;
                        }

                        const template = await templateResponse.text();
                        const container = createPreview(template, imageUrl);

                        attachEventHandlers(container, entryId, onDelete);
                        animateContainer(container, gallery);

                    } catch (error) {
                        console.error(`Failed to process file ${file.name}:`, error);
                    }
                }
            },

            initPreloadedImages: (entryId) => {
                attachEventsToPreloadedImages(entryId);
            },

            animateReadOnlyContainer: animateReadOnlyContainer
        };
    })()
};


export const preview = handlers.preview;