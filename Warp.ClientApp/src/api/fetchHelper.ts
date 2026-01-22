import { toProblemDetails, ProblemDetailsParseError, ProblemDetails } from '../types/problem-details'
import type { ApiError } from '../types/apis/api-error'
import type { AppRequestInit } from '../types/apis/app-request-init'
import { ErrorHandlingMode } from '../types/error-handling-mode'
import { buildTraceHeaders, extractTraceIdFromHeaders } from '../telemetry/traceContext'


function buildApiError(params: { message: string; status: number; requestId?: string | null; traceId?: string | null; eventId?: number; sentryId?: string | null; retryAfter?: number; method?: string; endpoint?: string; problem?: ProblemDetails; rawBody?: unknown; cause?: unknown }): ApiError {
    const err = new Error(params.message) as ApiError
    err.status = params.status
    err.requestId = params.requestId
    if (params.traceId !== undefined)
        err.traceId = params.traceId

    if (params.eventId !== undefined)
        err.eventId = params.eventId

    if (params.sentryId !== undefined)
        err.sentryId = params.sentryId

    if (params.retryAfter !== undefined)
        err.retryAfter = params.retryAfter

    if (params.method !== undefined)
        err.method = params.method

    if (params.endpoint !== undefined)
        err.endpoint = params.endpoint

    if (params.problem)
        err.problem = params.problem

    if (params.rawBody !== undefined)
        err.rawBody = params.rawBody

    if (params.cause)
        (err as any).cause = params.cause

    return err
}


type ErrorBridge = (error: ApiError, req: AppRequestInit) => void


let globalErrorBridge: ErrorBridge | null = null


export function registerErrorBridge(handler: ErrorBridge | null): void {
    globalErrorBridge = handler
}


function maybeHandleGlobally(error: ApiError, req: AppRequestInit): void {
    const mode = req.errorHandling ?? ErrorHandlingMode.Global
    if (mode === ErrorHandlingMode.Global && globalErrorBridge)
        globalErrorBridge(error, req)
}


export async function fetchJson<T = any>(url: string, opts: AppRequestInit = {}): Promise<T> {
    const method = (opts.method || 'GET').toString()
    const { headers, context: traceContext } = buildTraceHeaders({
        headers: opts.headers,
        traceContext: opts.traceContext,
        ensureJsonAccept: true
    })

    const fetchOptions: RequestInit = {
        ...opts,
        credentials: 'include',
        headers
    }

    delete (fetchOptions as any).traceContext
    delete (fetchOptions as any).errorHandling
    delete (fetchOptions as any).notifyLevel
    delete (fetchOptions as any).dedupeKey
    delete (fetchOptions as any).context

    let response: Response
    try {
        response = await fetch(url, fetchOptions)
    } catch (e: any) {
        const err = buildApiError({
            message: e?.message || 'Network error',
            status: 0,
            requestId: null,
            traceId: traceContext.traceId,
            method,
            endpoint: url,
            rawBody: undefined,
            cause: e
        })

        maybeHandleGlobally(err, opts)
        throw err
    }

    const contentType = response.headers.get('content-type') || ''
    const requestId = response.headers.get('x-request-id')
    const traceId = extractTraceIdFromHeaders(response.headers) ?? traceContext.traceId

    let parsedBody: any = undefined
    let isJson = /json/i.test(contentType)

    try {
        if (isJson)
            parsedBody = await response.json()
        else if (!response.ok)
            parsedBody = await response.text()
    } catch (e) {
        if (!response.ok) {
            const err = buildApiError({
                message: `Request failed: ${response.status}`,
                status: response.status,
                requestId,
                traceId,
                method,
                endpoint: url,
                rawBody: parsedBody,
                cause: e
            })
            maybeHandleGlobally(err, opts)
            throw err
        }
    }

    if (!response.ok) {
        // Try to interpret ProblemDetails when JSON
        if (isJson && parsedBody && typeof parsedBody === 'object') {
            try {
                const problem = toProblemDetails(parsedBody)

                const retryAfterHeader = response.headers.get('retry-after')
                const retryAfter = retryAfterHeader && /^\d+(?:\.\d+)?$/.test(retryAfterHeader.trim()) ? Number(retryAfterHeader.trim()) : undefined

                const err = buildApiError({
                    message: problem.title || `Request failed: ${response.status}`,
                    status: response.status,
                    requestId: requestId,
                    traceId: traceId ?? problem.traceId,
                    eventId: problem.eventId,
                    sentryId: problem.sentryId,
                    retryAfter,
                    method,
                    endpoint: url,
                    problem,
                    rawBody: parsedBody
                })

                maybeHandleGlobally(err, opts)
                throw err
            } catch (err) {
                if (err instanceof ProblemDetailsParseError) {
                    const built = buildApiError({
                        message: parsedBody?.message || `Request failed: ${response.status}`,
                        status: response.status,
                        requestId,
                        traceId,
                        method,
                        endpoint: url,
                        rawBody: parsedBody,
                        cause: err
                    })

                    maybeHandleGlobally(built, opts)
                    throw built
                }

                throw err
            }
        }

        const msg = typeof parsedBody === 'string' && parsedBody.trim() ? parsedBody.substring(0, 300) : `Request failed: ${response.status}`
        const err = buildApiError({
            message: msg,
            status: response.status,
            requestId,
            traceId,
            method,
            endpoint: url,
            rawBody: parsedBody
        })

        maybeHandleGlobally(err, opts)

        throw err
    }

    if (isJson)
        return parsedBody as T

    const textBody = await response.text()
    return textBody as unknown as T
}
