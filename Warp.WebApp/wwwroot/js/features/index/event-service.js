import { addDropAreaEvents, pasteImages } from '/js/components/gallery/upload.js';
import { preview } from '/js/components/gallery/preview.js';

/**
 * Service for managing event listeners and their cleanup for the index page
 */
export class IndexEventService {
    constructor() {
        this.eventListeners = new Map();
        this.abortController = new AbortController();
    }

    /**
     * Adds form submission event listener
     */
    addFormSubmitListener(form, handler) {
        const wrappedHandler = (event) => {
            event.preventDefault();
            handler(event);
        };

        form.addEventListener('submit', wrappedHandler, {
            signal: this.abortController.signal
        });

        this._trackListener('form-submit', { element: form, handler: wrappedHandler });
    }

    /**
     * Adds paste event listener for images
     */
    addPasteListener(entryId) {
        const handler = async (e) => {
            try {
                await pasteImages(entryId, e);
            } catch (error) {
                console.error('Error handling paste:', error);
            }
        };

        document.addEventListener('paste', handler, {
            signal: this.abortController.signal
        });

        this._trackListener('paste', { element: document, handler });
    }

    /**
     * Initializes gallery-related events
     */
    initGalleryEvents(entryId) {
        try {
            addDropAreaEvents(entryId);
            preview.initPreloadedImages(entryId);
        } catch (error) {
            console.error('Error initializing gallery events:', error);
        }
    }

    /**
     * Cleans up all event listeners
     */
    cleanup() {
        this.abortController.abort();
        this.eventListeners.clear();
    }

    /**
     * Gets current number of active listeners (for debugging)
     */
    getListenerCount() {
        return this.eventListeners.size;
    }

    _trackListener(name, config) {
        this.eventListeners.set(name, config);
    }
}