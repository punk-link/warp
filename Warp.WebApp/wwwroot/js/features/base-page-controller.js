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


    animateNumberCount(element, targetValue, duration = 1500) {
        const startValue = parseInt(element.textContent) || 0;
        const startTime = performance.now();
        
        element.classList.add('view-counter');
        
        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            const easeOutCubic = 1 - Math.pow(1 - progress, 3);
            const currentValue = Math.round(startValue + (targetValue - startValue) * easeOutCubic);
            
            element.textContent = currentValue;
            
            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                setTimeout(() => {
                    element.classList.remove('view-counter');
                }, 500);
            }
        };
        
        requestAnimationFrame(animate);
    }


    #setCursor(state) {
        document.body.style.cursor = state;
    }
}