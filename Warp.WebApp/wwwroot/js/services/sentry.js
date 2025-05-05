/**
 * Provides Sentry error tracking services for the application.
 * This service handles initialization of the Sentry client and error capture functionality.
 * Sentry is bypassed in the 'Local' environment.
 */
import * as Sentry from '@sentry/browser';


const handlers = {
    sentry: (() => {
        let isInitialized = false;
        let currentEnvironment = window.appConfig?.environment;


        const logErrorToConsole = (error, context = {}, prefix = '') => {
            const contextString = Object.keys(context).length > 0
                ? `\nContext: ${JSON.stringify(context, null, 2)}`
                : '';

            console.error(`${prefix.length > 0 ? prefix + ': ' : ''}${error.message || error}${contextString}`);
        };


        const initialize = () => {
            if (currentEnvironment === 'Local') {
                console.info('Sentry bypassed in Local environment');
                return;
            }
            
            Sentry.init({
                dsn: window.appConfig?.sentryDsn,
                environment: currentEnvironment,
                integrations: [ new Sentry.BrowserTracing() ],
                tracesSampleRate: 1.0,
            });
            
            isInitialized = true;
        };


        /**
         * Captures an error with additional context and sends it to Sentry.
         * Also logs the error to the console in all environments.
         * Does not send to Sentry when in 'Local' environment.
         * @param {Error} error - The error object to capture
         * @param {Object} context - Additional contextual information to include with the error
         * @param {string} consolePrefix - Optional prefix for console error messages
         */
        const captureError = (error, context = {}, consolePrefix = '') => {
            logErrorToConsole(error, context, consolePrefix);
            
            if (currentEnvironment === 'Local' || !isInitialized) 
                return;
            
            Sentry.withScope(scope => {
                Object.entries(context).forEach(([key, value]) => {
                    scope.setExtra(key, value);
                });
                Sentry.captureException(error);
            });
        };


        return {
            initialize,
            captureError
        };
    })()
};


export const sentryService = handlers.sentry;