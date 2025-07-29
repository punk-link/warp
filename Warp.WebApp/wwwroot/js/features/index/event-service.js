import { addDropAreaEvents, pasteImages } from '/js/components/gallery/upload.js';
import { preview } from '/js/components/gallery/preview.js';


export class IndexEventService {
    constructor() {
        this.eventListeners = new Map();
        this.abortController = new AbortController();
    }


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


    initGalleryEvents(entryId) {
        try {
            addDropAreaEvents(entryId);
            preview.initPreloadedImages(entryId);
        } catch (error) {
            console.error('Error initializing gallery events:', error);
        }
    }


    cleanup() {
        this.abortController.abort();
        this.eventListeners.clear();
    }


    getListenerCount() {
        return this.eventListeners.size;
    }


    _trackListener(name, config) {
        this.eventListeners.set(name, config);
    }
}