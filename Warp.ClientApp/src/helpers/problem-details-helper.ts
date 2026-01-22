import { ProblemDetails } from "../types/apis/problem-details/problem-details"
import { ProblemDetailsParseError } from "../types/apis/problem-details/problem-details-parse-error"


/** Type guard to check if a value conforms to ProblemDetails shape. */
export function isProblemDetails(value: unknown): value is ProblemDetails {
    if (!value || typeof value !== 'object')
        return false

    const v: any = value
    return typeof v.type === 'string'
        && typeof v.title === 'string'
        && typeof v.status === 'number'
        && typeof v.detail === 'string'
        && typeof v.traceId === 'string'
}


/** Parses a raw object into a ProblemDetails instance, throwing ProblemDetailsParseError on failure. */
export function toProblemDetails(raw: any): ProblemDetails {
    if (!raw || typeof raw !== 'object') 
        throw new ProblemDetailsParseError('Problem details payload is not an object', raw)

    const type = asString(raw.type)
    const title = asString(raw.title)
    const status = asNumber(raw.status)
    const detail = asString(raw.detail)
    const traceId = asString(raw.traceId ?? raw['trace-id'])

    if (!type || !title || status == null || !detail || !traceId) {
        const missing: string[] = []
        if (!type) missing.push('type')
        if (!title) missing.push('title')
        if (status == null) missing.push('status')
        if (!detail) missing.push('detail')
        if (!traceId) missing.push('traceId')
        throw new ProblemDetailsParseError(`Problem details missing required field(s): ${missing.join(', ')}`, raw)
    }

    const eventId = asNumber(raw.eventId ?? raw['event-id'])
    const sentryId = asString(raw.sentryId ?? raw['sentry-id'])
    const stackTrace = asString(raw.stackTrace ?? raw['stack-trace'])
    const errors = normalizeErrors(raw.errors)

    const base: ProblemDetails = {
        type,
        title,
        status,
        detail,
        traceId,
        ...(eventId != null ? { eventId } : {}),
        ...(sentryId ? { sentryId } : {}),
        ...(stackTrace ? { stackTrace } : {}),
        ...(errors ? { errors } : {})
    }

    for (const [k, v] of Object.entries(raw)) {
        if (k in base) 
            continue

        if (['trace-id', 'event-id', 'sentry-id', 'stack-trace', 'traceId', 'eventId', 'sentryId', 'stackTrace', 'errors', 'type', 'title', 'status', 'detail'].includes(k)) 
            continue

        (base as any)[k] = v
    }

    return base
}


function asString(v: unknown): string | undefined {
    if (v == null) 
        return undefined

    if (typeof v === 'string') 
        return v

    if (typeof v === 'number' || typeof v === 'boolean') 
        return String(v)

    return undefined
}


function asNumber(v: unknown): number | undefined {
    if (typeof v === 'number' && Number.isFinite(v)) 
        return v

    if (typeof v === 'string' && v.trim() !== '') {
        const n = Number(v)
        return Number.isFinite(n) ? n : undefined
    }

    return undefined
}


function normalizeErrors(raw: any): Record<string, string[]> | undefined {
    if (!raw || typeof raw !== 'object' || Array.isArray(raw)) 
        return undefined

    const result: Record<string, string[]> = {}
    for (const [k, v] of Object.entries(raw)) {
        if (Array.isArray(v)) {
            const arr = v.map(asString).filter((s): s is string => !!s)
            if (arr.length) 
                result[k] = arr
        } else {
            const s = asString(v)
            if (s) result[k] = [s]
        }
    }

    return Object.keys(result).length ? result : undefined
}