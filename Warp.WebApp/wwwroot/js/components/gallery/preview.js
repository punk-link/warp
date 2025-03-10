import { uiState } from '/js/utils/ui-core.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { elements } from './elements.js';


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
                .querySelector('.image-container');

            const image = document.createElement('img');
            image.src = imageUrl;

            elements.getImageContainer(container).image.replaceWith(image);
            return container;
        };

        const attachEventHandlers = (container, entryId, onDelete) => {
            container.querySelector('.delete-image-button')
                .addEventListener('click', e => onDelete(e, entryId));
        };

        const animateContainer = (container, gallery) => {
            uiState.toggleClasses(container, { add: [CSS_CLASSES.HIDDEN] });
            gallery.prepend(container);

            uiState.toggleClasses(container, {
                remove: [CSS_CLASSES.HIDDEN],
                add: [CSS_CLASSES.ANIMATE]
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
            }
        };
    })()
};


export const preview = handlers.preview;