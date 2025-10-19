import { toProblemDetails, ProblemDetailsParseError, ProblemDetails } from '../types/problem-details'


export interface ApiError extends Error {
    status: number
    requestId?: string | null
    problem?: ProblemDetails
    rawBody?: unknown
}


function buildApiError(params: { message: string; status: number; requestId?: string | null; problem?: ProblemDetails; rawBody?: unknown; cause?: unknown }): ApiError {
    const err = new Error(params.message) as ApiError
    err.status = params.status
    err.requestId = params.requestId
    
    if (params.problem) 
        err.problem = params.problem

    if (params.rawBody !== undefined) 
        err.rawBody = params.rawBody

    if (params.cause) 
        (err as any).cause = params.cause

    return err
}


export async function fetchJson<T = any>(url: string, opts: RequestInit = {}): Promise<T> {
    const response = await fetch(url, { credentials: 'include', headers: { 'Accept': 'application/json', ...(opts.headers || {}) }, ...opts })

    const contentType = response.headers.get('content-type') || ''
    const requestId = response.headers.get('x-request-id')

    let parsedBody: any = undefined
    let isJson = contentType.includes('application/json')

    try {
        if (isJson) 
            parsedBody = await response.json()
        else if (!response.ok) 
            parsedBody = await response.text()
    } catch (e) {
        if (!response.ok) 
            throw buildApiError({ message: `Request failed: ${response.status}`, status: response.status, requestId, rawBody: parsedBody, cause: e })
    }

    if (!response.ok) {
        // Try to interpret ProblemDetails when JSON
        if (isJson && parsedBody && typeof parsedBody === 'object') {
            try {
                const problem = toProblemDetails(parsedBody)

                throw buildApiError({
                    message: problem.title || `Request failed: ${response.status}`,
                    status: response.status,
                    requestId: requestId || problem.traceId,
                    problem,
                    rawBody: parsedBody
                })
            } catch (err) {
                if (err instanceof ProblemDetailsParseError) {
                    throw buildApiError({
                        message: parsedBody?.message || `Request failed: ${response.status}`,
                        status: response.status,
                        requestId,
                        rawBody: parsedBody,
                        cause: err
                    })
                }
                
                throw err
            }
        }

        const msg = typeof parsedBody === 'string' && parsedBody.trim() ? parsedBody.substring(0, 300) : `Request failed: ${response.status}`
        throw buildApiError({ message: msg, status: response.status, requestId, rawBody: parsedBody })
    }


    if (isJson)
        return parsedBody as T

    const textBody = await response.text()
    return textBody as unknown as T
}
