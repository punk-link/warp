import type { ProblemDetails } from '../types/apis/problem-details/problem-details'
import { ProblemDetailsParseError } from '../types/apis/problem-details/problem-details-parse-error'
import type { ApiError } from '../types/apis/api-error'
import type { AppRequestInit } from '../types/apis/app-request-init'
import { toProblemDetails } from '../helpers/problem-details-helper'
import { ErrorHandlingMode } from '../types/apis/enums/error-handling-mode'
import { buildTraceHeaders, extractTraceIdFromHeaders } from '../telemetry/traceContext'


/** Performs a fetch request and parses the JSON response. */
export async function fetchJson<T = any>(url: string, opts: AppRequestInit = {}): Promise<T> {
    const method = (opts.method || 'GET').toString()
    const { headers, context: traceContext } = buildTraceHeaders({
        headers: opts.headers,
        traceContext: opts.traceContext,
        ensureJsonAccept: true
    })

    const fetchOptions = buildFetchOptions(opts, headers)
    const response = await performFetch(url, fetchOptions, method, traceContext, opts)
    const metadata = extractResponseMetadata(response, traceContext)
    const { parsedBody, isJson } = await parseResponseBody(response, metadata, method, url, opts)

    if (!response.ok)
        handleErrorResponse(response, parsedBody, isJson, metadata, method, url, opts)

    return isJson 
        ? parsedBody 
        : await response.text() as unknown as T
}


/** Registers a global error bridge handler. */
export function registerErrorBridge(handler: ErrorBridge | null): void {
    globalErrorBridge = handler
}


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


function buildFetchOptions(opts: AppRequestInit, headers: Headers): RequestInit {
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

    return fetchOptions
}


function extractResponseMetadata(response: Response, traceContext: { traceId: string }) {
    return {
        contentType: response.headers.get('content-type') || '',
        requestId: response.headers.get('x-request-id'),
        traceId: extractTraceIdFromHeaders(response.headers) ?? traceContext.traceId
    }
}


let globalErrorBridge: ErrorBridge | null = null


function handleErrorResponse(response: Response, parsedBody: any, isJson: boolean, metadata: { requestId: string | null; traceId: string }, method: string, url: string, opts: AppRequestInit): never {
    if (isJson && parsedBody && typeof parsedBody === 'object')
        handleJsonErrorResponse(response, parsedBody, metadata, method, url, opts)

    const msg = typeof parsedBody === 'string' && parsedBody.trim() 
        ? parsedBody.substring(0, 300) 
        : `Request failed: ${response.status}`
    
    const err = buildApiError({
        message: msg,
        status: response.status,
        requestId: metadata.requestId,
        traceId: metadata.traceId,
        method,
        endpoint: url,
        rawBody: parsedBody
    })

    maybeHandleGlobally(err, opts)
    throw err
}


function handleJsonErrorResponse(response: Response, parsedBody: any, metadata: { requestId: string | null; traceId: string }, method: string, url: string, opts: AppRequestInit): never {
    try {
        const problem = toProblemDetails(parsedBody)
        const retryAfter = parseRetryAfter(response.headers.get('retry-after'))

        const err = buildApiError({
            message: problem.title || `Request failed: ${response.status}`,
            status: response.status,
            requestId: metadata.requestId,
            traceId: metadata.traceId ?? problem.traceId,
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
                requestId: metadata.requestId,
                traceId: metadata.traceId,
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


function maybeHandleGlobally(error: ApiError, req: AppRequestInit): void {
    const mode = req.errorHandling ?? ErrorHandlingMode.Global
    if (mode === ErrorHandlingMode.Global && globalErrorBridge)
        globalErrorBridge(error, req)
}


async function parseResponseBody(response: Response, metadata: { contentType: string; requestId: string | null; traceId: string }, method: string, url: string, opts: AppRequestInit): Promise<{ parsedBody: any; isJson: boolean }> {
    const isJson = /json/i.test(metadata.contentType)
    let parsedBody: any = undefined

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
                requestId: metadata.requestId,
                traceId: metadata.traceId,
                method,
                endpoint: url,
                rawBody: parsedBody,
                cause: e
            })
            
            maybeHandleGlobally(err, opts)
            throw err
        }
    }

    return { parsedBody, isJson }
}


function parseRetryAfter(headerValue: string | null): number | undefined {
    if (!headerValue)
        return undefined

    const trimmed = headerValue.trim()
    return /^\d+(?:\.\d+)?$/.test(trimmed) ? Number(trimmed) : undefined
}


async function performFetch(url: string, fetchOptions: RequestInit, method: string, traceContext: { traceId: string }, opts: AppRequestInit): Promise<Response> {
    try {
        return await fetch(url, fetchOptions)
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
}


type ErrorBridge = (error: ApiError, req: AppRequestInit) => void
