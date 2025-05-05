/**
 * Provides global error handling functionality for the application.
 * Sets up event listeners for uncaught exceptions and unhandled promise rejections.
 * Also handles initialization of error tracking services.
 */


import { sentryService } from '/js/services/sentry.js';


const initializeErrorTracking = () => {
    sentryService.initialize();
};


/**
 * Sets up global error handlers for uncaught exceptions and promise rejections
 */
const setupGlobalErrorHandlers = () => {
    window.addEventListener('error', (event) => {
        sentryService.captureError(event.error, {
            errorType: 'uncaughtException',
            errorEvent: event.type
        });
    });


    window.addEventListener('unhandledrejection', (event) => {
        sentryService.captureError(event.reason, {
            errorType: 'unhandledRejection',
            errorEvent: event.type
        });
    });
};


export const errorHandlers = {
    initializeErrorTracking,
    setupGlobalErrorHandlers
};