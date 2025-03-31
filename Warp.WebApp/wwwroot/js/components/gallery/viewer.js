import { Fancybox } from '@fancyapps/ui';

const handlers = {
    viewer: (() => {
        const initializeFancybox = (selector = '[data-fancybox]') => {
            Fancybox.bind(selector, {
                caption: (fancybox, slide) => slide.thumbEl?.alt || ''
            });
        };

        return {
            init: (items = null) => {
                if (items && (!Array.isArray(items) || !items.length)) 
                    return;

                initializeFancybox();
                    return true;
            }
        };
    })()
};


export const galleryViewer = handlers.viewer;