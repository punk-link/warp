import type { ProblemDetails } from './problem-details/problem-details'


/** Represents an error that occurs during an API call. */
export interface ApiError extends Error {
    status: number
    requestId?: string | null
    traceId?: string | null
    eventId?: number
    sentryId?: string | null
    retryAfter?: number
    method?: string
    endpoint?: string
    problem?: ProblemDetails
    rawBody?: unknown
}
