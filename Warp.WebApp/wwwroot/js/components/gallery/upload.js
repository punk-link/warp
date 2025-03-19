import { uiState } from '/js/utils/ui-core.js';
import { ROUTES } from '/js/utils/routes.js'; 
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

            const response = await http.delete(ROUTES.API.IMAGES.DELETE(entryId, imageId));
            
            if (response.ok) 
                container.remove();
        },

        toggleUploadState: (element, isUploading) => {
            const { icon } = elements.getImageContainer(element);
            const [oldIconClass, newIconClass] = isUploading 
                ? [ICONS.PLUS, ICONS.CLOCK]
                : [ICONS.CLOCK, ICONS.PLUS];

            uiState.toggleClasses(icon, {
                add: [newIconClass],
                remove: [oldIconClass]
            });
        },

        isValidImageFile: (file) => {
            return file && file.type && file.type.startsWith('image/');
        },

        upload: async (entryId, files) => {
            const uploadContainer = elements.getUploadContainer();

            const validImageFiles = files.filter(handlers.image.isValidImageFile);
            if (validImageFiles.length === 0) 
                return;

            handlers.image.toggleUploadState(uploadContainer, true);

            const formData = new FormData();
            validImageFiles.forEach(file => formData.append('Images', file, file.name));

            const response = await http.post(ROUTES.API.IMAGES.ADD(entryId), formData);

            if (!response.ok) {
                handlers.image.toggleUploadState(uploadContainer, false);
                console.error(response.status, response.statusText);
                return;
            }

            const results = await response.json();
            preview.render(entryId, validImageFiles, results, handlers.image.delete);
            
            handlers.image.toggleUploadState(uploadContainer, false);
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
        handlePasteEvent: async (entryId, pasteEvent) => {
            if (pasteEvent?.clipboardData?.files.length > 0) {
                const files = Array.from(pasteEvent.clipboardData.files);
                await handlers.image.upload(entryId, files);

                return true;
            }

            return false;
        },

        createFileFromBlob: async (item, type, index) => {
            try {
                const blob = await item.getType(type);
                if (!handlers.image.isValidImageFile(blob)) 
                    return null;

                const extension = type.split('/')[1] || 'png';
                const timestamp = new Date().getTime();
                const fileName = `clipboard-image-${timestamp}-${index}.${extension}`;

                return new File([blob], fileName, { type: blob.type });
            } catch (error) {
                console.error(`Error processing clipboard item: ${error}`);
                return null;
            }
        },

        getImagesFromClipboard: async () => {
            await navigator.permissions.query({ name: 'clipboard-read' });
            const clipboardItems = await navigator.clipboard.read();

            let fileCounter = 1;
            const filePromises = [];

            for (const item of clipboardItems) {
                const imageTypes = item.types.filter(type => type.startsWith('image/'));

                for (const type of imageTypes) 
                    filePromises.push(handlers.clipboard.createFileFromBlob(item, type, fileCounter++));
            }

            const files = await Promise.all(filePromises);
            return files.filter(Boolean);
        },

        init: async (entryId, pasteEvent) => {
            try {
                let isPasteEventHandled = await handlers.clipboard.handlePasteEvent(entryId, pasteEvent);
                if (isPasteEventHandled) 
                    return;

                const files = await handlers.clipboard.getImagesFromClipboard();
                await handlers.image.upload(entryId, files);
            } catch (error) {
                console.error('Paste operation failed:', error);
            }
        }
    }
};


export const { init: addDropAreaEvents } = handlers.dropArea;
export const { init: pasteImages } = handlers.clipboard;