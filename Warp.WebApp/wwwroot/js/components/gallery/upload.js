import { uiState } from '/js/utils/ui-core.js';
import { http } from '/js/services/http/client.js';
import { CSS_CLASSES } from '/js/constants/css.js';
import { ICONS } from '/js/utils/icons.js';
import { elements } from './elements.js';
import { preview } from './preview.js';
import { dispatchEvent } from '/js/services/events.js';


const handlers = {
    image: {
        delete: async (e, entryId) => {
            const container = e.target.closest('.image-container');
            const { imageId } = elements.getImageContainer(container);

            const response = await http.delete(`/api/images/entry-id/${entryId}/image-id/${imageId}`);
            
            if (response.ok) 
                container.remove();
        },

        toggleUploadState: (element, isUploading) => {
            const { icon } = elements.getImageContainer(element);
            const [oldState, newState] = isUploading 
                ? [ICONS.PLUS, ICONS.CLOCK]
                : [ICONS.CLOCK, ICONS.PLUS];
            
            uiState.toggleClasses(element, { 
                add: [CSS_CLASSES.HIDDEN],
                remove: [oldState] 
            });

            uiState.toggleClasses(icon, { 
                add: [newState],
                remove: [CSS_CLASSES.HIDDEN] 
            });

            uiState.toggleClasses(element, { 
                add: [CSS_CLASSES.ANIMATE] 
            });
        },

        upload: async (entryId, files) => {
            const uploadButton = elements.getUploadButton();
            handlers.image.toggleUploadState(uploadButton, true);

            const formData = new FormData();
            files.forEach(file => formData.append('Images', file, file.name));

            const response = await http.post(`/api/images/entry-id/${entryId}`, formData);

            if (!response.ok) {
                handlers.image.toggleUploadState(uploadButton, false);
                console.error(response.status, response.statusText);
                return;
            }

            const results = await response.json();
            preview.render(entryId, files, results, handlers.image.delete);
            
            handlers.image.toggleUploadState(uploadButton, false);
            dispatchEvent.uploadFinished();
        }
    },

    dropArea: {
        preventDefault: e => {
            e.preventDefault();
            e.stopPropagation();
        },

        toggleHighlight: (dropArea, isHighlighted) => {
            uiState.toggleClasses(dropArea, { 
                [isHighlighted ? 'add' : 'remove']: [CSS_CLASSES.HIGHLIGHTED] 
            });
        },

        init: (entryId, dropArea, fileInput, uploadButton) => {
            ['dragenter', 'dragover', 'dragleave', 'drop']
                .forEach(event => dropArea.addEventListener(
                    event, 
                    handlers.dropArea.preventDefault
                ));

            ['dragenter', 'dragover']
                .forEach(event => dropArea.addEventListener(
                    event, 
                    () => handlers.dropArea.toggleHighlight(dropArea, true)
                ));

            ['dragleave', 'drop']
                .forEach(event => dropArea.addEventListener(
                    event, 
                    () => handlers.dropArea.toggleHighlight(dropArea, false)
                ));

            dropArea.addEventListener('drop', async e => {
                const files = Array.from(e.dataTransfer.files);
                await handlers.image.upload(entryId, files);
            });

            fileInput.addEventListener('change', async e => {
                const files = Array.from(e.target.files);
                await handlers.image.upload(entryId, files);
            });

            uploadButton.addEventListener('click', () => fileInput.click());
        }
    },

    clipboard: {
        init: async (entryId) => {
            try {
                await navigator.permissions.query({ name: 'clipboard-read' });
                const items = await navigator.clipboard.read();
                
                const files = await Promise.all(
                    items.flatMap(async item => {
                        const imageTypes = item.types
                            .filter(type => type.startsWith('image/'));
                        
                        return Promise.all(
                            imageTypes.map(type => item.getType(type))
                        );
                    })
                );

                await handlers.image.upload(entryId, files.flat());
            } catch (error) {
                console.error('Paste failed:', error);
            }
        }
    }
};


export const { init: addDropAreaEvents } = handlers.dropArea;
export const { init: pasteImages } = handlers.clipboard;