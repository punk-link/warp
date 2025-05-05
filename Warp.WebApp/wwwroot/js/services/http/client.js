import { ROUTES, buildUrl } from '/js/utils/routes.js';
import { sentryService } from '/js/services/sentry.js';


const HTTP_METHOD = Object.freeze({
    POST: 'POST',
    GET: 'GET',
    PUT: 'PUT',
    PATCH: 'PATCH',
    DELETE: 'DELETE'
});


const HTTP_HEADERS = Object.freeze({
    FORM_DATA: {
        'Accept': 'application/json; charset=utf-8'
        // Note: Content-Type is automatically set by the browser when using FormData
    },
    JSON: {
        'Accept': 'application/json; charset=utf-8',
        'Content-Type': 'application/json; charset=utf-8'
    }
});


const HTTP_ERROR = Object.freeze({
    MISSING_URL: 'URL is required',
    INVALID_METHOD: 'Invalid HTTP method',
    REQUEST_FAILED: 'HTTP Request failed'
});


const handlers = {
    request: (() => {
        const validateRequest = (url, method) => {
            if (!url) 
                throw new Error(HTTP_ERROR.MISSING_URL);
            
            if (!Object.values(HTTP_METHOD).includes(method)) 
                throw new Error(HTTP_ERROR.INVALID_METHOD);
        };

        const buildRequestOptions = (method, body = null, headers = HTTP_HEADERS.JSON) => {
            const options = { method, headers };

            if (body) {
                if (body instanceof FormData) {
                    options.headers = HTTP_HEADERS.FORM_DATA;
                    options.body = body;
                } else {
                    options.body = typeof body === 'string' 
                        ? body 
                        : JSON.stringify(body);
                }
            }

            return options;
        };

        const validateResponse = async (response, url) => {
            if (response.ok) 
                return response;

            const problemDetails = await response.json();
            
            sentryService.captureError(
                new Error(HTTP_ERROR.REQUEST_FAILED), 
                {
                    url: url,
                    status: response.status,
                    statusText: response.statusText,
                    problemDetails: problemDetails
                },
                'HTTP Request failed'
            );
            
            // Redirect to error page if the response contains problem details
            // This is by design to handle specific error cases
            // Until we implement a front-end error handling strategy
            location.href = buildUrl(ROUTES.ERROR, { 
                details: JSON.stringify(problemDetails) 
            });
            
            throw new Error(HTTP_ERROR.REQUEST_FAILED);
        };

        return {
            execute: async (url, method, body = null) => {
                try {
                    validateRequest(url, method);
                    
                    const options = buildRequestOptions(method, body);
                    const response = await fetch(url, options);

                    await validateResponse(response, url);

                    return response;
                } catch (error) {
                    sentryService.captureError(
                        error, 
                        {
                            url: url,
                            method: method,
                            context: 'httpClient'
                        },
                        'HTTP Request error'
                    );
                }
            }
        };
    })()
};


export const { POST, GET, PUT, PATCH, DELETE } = HTTP_METHOD;

export const makeHttpRequest = async (url, method, body = null) => handlers.request.execute(url, method, body);

export const http = {
    get: (url) => 
        makeHttpRequest(url, GET),
    
    post: (url, data) => 
        makeHttpRequest(url, POST, data),
    
    put: (url, data) => 
        makeHttpRequest(url, PUT, data),
    
    patch: (url, data) => 
        makeHttpRequest(url, PATCH, data),
    
    delete: (url) => 
        makeHttpRequest(url, DELETE)
};