/**
 * Core application initialization module.
 * Handles setting up error tracking and other essential services that should run on all pages.
 * This module should be imported at the beginning of each page's entry point.
 */


import { errorHandlers } from '/js/utils/error-handlers.js';


/**
 * Initializes core application services.
 * This function should be called as early as possible in each page's lifecycle.
 */
const initializeCore = () => {
    errorHandlers.initializeErrorTracking();
    errorHandlers.setupGlobalErrorHandlers();
};


export const core = {
    initialize: initializeCore
};