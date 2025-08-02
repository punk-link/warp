import { CSS_CLASSES } from '/js/constants/css.js';
import { animateBackgroundImage } from '/js/components/background/image-positioner.js';
import { sentryService } from '/js/services/sentry.js';
import { uiState } from '/js/utils/ui-core.js';


export class BasePageController {
    constructor(elements) {
        this.elements = elements;
    }


    captureException(error, errorMessage, action) {
        sentryService.captureError(error, { action }, errorMessage);
        throw new Error(errorMessage);
    }


    displayError(message) {
        const errorElement = this.elements.getErrorContainer(); //.getElementById('error-message');
        if (errorElement) {
            errorElement.textContent = message;
            uiState.toggleClasses(errorElement, { remove: [CSS_CLASSES.HIDDEN] });
        } else {
            console.error(message);
        }
    }


    executeWithLoadingIndicator(action) {
        try {
            this.#setCursor('wait');
            return action();
        } finally {
            this.#setCursor('auto');
        }
    }


    initRoamingImage() {
        const roamingImage = this.elements.getRoamingImage();
        animateBackgroundImage(roamingImage);
    }


    enableSmoothContentResize() {
        if (!document.querySelector('#content-resize-styles')) {
            const style = document.createElement('style');
            style.id = 'content-resize-styles';
            style.textContent = `
                /* Smooth gallery expansion */
                .gallery {
                    max-height: 0;
                    overflow: hidden;
                    transition: max-height 0.5s ease-out, opacity 0.3s ease-out;
                }
                .gallery.expanded {
                    max-height: 2000px;
                }
                
                /* Smooth text content appearance */
                .text-content {
                    opacity: 0;
                    transform: translateY(10px);
                    transition: opacity 0.4s ease-out, transform 0.4s ease-out;
                }
                .text-content.visible {
                    opacity: 1;
                    transform: translateY(0);
                }
                
                /* Individual image container animations */
                .image-container {
                    transform: scale(0.95);
                    transition: transform 0.3s ease-out, opacity 0.3s ease-out;
                }
                .image-container.loaded {
                    transform: scale(1);
                }
                
                /* Article smooth transitions */
                article {
                    transition: padding 0.3s ease-out;
                }
            `;
            document.head.appendChild(style);
        }
    }
    

    #setCursor(state) {
        document.body.style.cursor = state;
    }
}