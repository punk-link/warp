import { dom } from '/js/utils/ui-core.js';


export const elements = {
    getRequestId: () => ({
        code: dom.get('request-id-code'),
        tooltip: dom.get('copy-tooltip')
    })
};