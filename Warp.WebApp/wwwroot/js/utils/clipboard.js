import { sentryService } from '/js/services/sentry.js';


const handlers = {
    clipboard: (() => {
        const setCursor = (state) => {
            document.body.style.cursor = state;
        };

        return {
            copyUrl: async (url) => {
                try {
                    setCursor('wait');
                    const targetUrl = url || window.location.href;
                    await navigator.clipboard.writeText(targetUrl);

                    return true;
                } catch (error) {
                    sentryService.captureError(error, { 
                        feature: 'clipboard', 
                        action: 'copyUrl' 
                    }, 'Copy to clipboard failed');
                    return false;
                } finally {
                    setCursor('auto');
                }
            },

            copyText: async (textContainer) => {
                try {
                    setCursor('wait');

                    const textToCopy = textContainer.textContent.trim();
                    await navigator.clipboard.writeText(textToCopy);

                    return true;
                } catch (error) {
                    sentryService.captureError(error, { 
                        feature: 'clipboard', 
                        action: 'copyText' 
                    }, 'Copy to clipboard failed');
                    return false;
                } finally {
                    setCursor('auto');
                }
            }
        };
    })()
};


export const copyUrl = handlers.clipboard.copyUrl;
export const copyText = handlers.clipboard.copyText;
