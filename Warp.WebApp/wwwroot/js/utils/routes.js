export const ROUTES = Object.freeze({
    // App routes
    ROOT: '/',
    DELETED: '/deleted',
    ERROR: '/error',
    
    // API routes
    ENTRY: '/entry',
    PREVIEW: '/preview',

    API: {
        CREATORS: {
            GET_OR_SET: '/api/creators'
        },

        ENTRIES: {
            ADD_OR_UPDATE: (entryId) => `/api/entries/${entryId}`,
            COPY: (entryId) => `/api/entries/${entryId}/copy`,
            CREATE: `/api/entries`,
            DELETE: (entryId) => `/api/entries/${entryId}`,
            GET: (entryId) => `/api/entries/${entryId}`,
            REPORT: (entryId) => `/api/entries/${entryId}/report`
        },

        IMAGES: {
            ADD: (entryId) => `/api/images/entry-id/${entryId}`,
            DELETE: (entryId, imageId) => `/api/images/entry-id/${entryId}/image-id/${imageId}`
        },

        METADATA: {
            DEFAULT_OPENGRAPH: '/api/metadata/opengraph/default',
            EDIT_MODES: '/api/metadata/enums/edit-modes',
            EXPIRATION_PERIODS: '/api/metadata/enums/expiration-periods'
        }
    }
});


export const buildUrl = (path, params = {}) => {
    const queryString = Object.entries(params)
        .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
        .join('&');

    return queryString ? `${path}?${queryString}` : path;
};

export const redirectTo = (path, params = {}) => {
    location.href = buildUrl(path, params);
};