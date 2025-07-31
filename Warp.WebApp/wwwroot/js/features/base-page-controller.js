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
        if (roamingImage) {
            animateBackgroundImage(roamingImage);
        }
    }


    #setCursor(state) {
        document.body.style.cursor = state;
    }
}