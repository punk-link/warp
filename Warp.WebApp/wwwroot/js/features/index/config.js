/**
 * Configuration constants for the index page
 */
export const CONFIG = Object.freeze({
    DELAYS: {
        BUTTON_UPDATE: 50,
        TEXTAREA_RESIZE: 100
    },
    SELECTORS: {
        IMAGE_CONTAINERS: '.image-container:not(#empty-image-container)'
    },
    RETRY: {
        MAX_ATTEMPTS: 3,
        DELAY_MS: 1000
    }
});