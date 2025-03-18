export const ROUTES = Object.freeze({
    // App routes
    ROOT: '/',
    DELETED: '/deleted',
    ERROR: '/error',
    
    // API routes
    ENTRY: '/entry',
    API: {
        ENTRIES: '/api/entries',
        REPORT: (entryId) => `/api/entries/${entryId}/report`
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