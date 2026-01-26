import { ProblemDetails } from "../types/apis/problem-details/problem-details"
import { ProblemDetailsParseError } from "../types/apis/problem-details/problem-details-parse-error"


const RESERVED_PROPERTY_NAMES = [ 
    'trace-id', 
    'event-id', 
    'sentry-id', 
    'stack-trace', 
    'traceId', 
    'eventId', 
    'sentryId', 
    'stackTrace', 
    'errors', 
    'type', 
    'title', 
    'status', 
    'detail' 
]


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

    const required = parseRequiredFields(raw)
    validateRequiredFields(required, raw)

    const optional = parseOptionalFields(raw)
    const base = buildBaseProblemDetails(required.type!, required.title!, required.status!, required.detail!, required.traceId!, optional)

    addExtraProperties(base, raw)

    return base
}


function addExtraProperties(base: ProblemDetails, pair: any) {
    for (const [key, value] of Object.entries(pair)) {
        if (key in base)
            continue

        if (RESERVED_PROPERTY_NAMES.includes(key))
            continue

        (base as any)[key] = value
    }
}


function asString(value: unknown): string | undefined {
    if (value == null) 
        return undefined

    if (typeof value === 'string') 
        return value

    if (typeof value === 'number' || typeof value === 'boolean') 
        return String(value)

    return undefined
}


function asNumber(value: unknown): number | undefined {
    if (typeof value === 'number' && Number.isFinite(value)) 
        return value

    if (typeof value === 'string' && value.trim() !== '') {
        const number = Number(value)
        return Number.isFinite(number) 
            ? number 
            : undefined
    }

    return undefined
}


function buildBaseProblemDetails(
    type: string,
    title: string,
    status: number,
    detail: string,
    traceId: string,
    optional: ReturnType<typeof parseOptionalFields>
): ProblemDetails {
    const { eventId, sentryId, stackTrace, errors } = optional

    return {
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
}


function collectMissingFields(
    type: string | undefined,
    title: string | undefined,
    status: number | undefined,
    detail: string | undefined,
    traceId: string | undefined
): string[] {
    const missing: string[] = []
    if (!type)
        missing.push('type')

    if (!title)
        missing.push('title')

    if (status == null)
        missing.push('status')

    if (!detail)
        missing.push('detail')
    
    if (!traceId)
        missing.push('traceId')

    return missing
}


function normalizeErrors(raw: any): Record<string, string[]> | undefined {
    if (!raw || typeof raw !== 'object' || Array.isArray(raw)) 
        return undefined

    const result: Record<string, string[]> = {}
    for (const [key, value] of Object.entries(raw)) {
        if (Array.isArray(value)) {
            const array = value.map(asString).filter((s): s is string => !!s)
            if (array.length) 
                result[key] = array
        } else {
            const str = asString(value)
            if (str) result[key] = [str]
        }
    }

    return Object.keys(result).length ? result : undefined
}


function parseOptionalFields(raw: any) {
    return {
        eventId: asNumber(raw.eventId ?? raw['event-id']),
        sentryId: asString(raw.sentryId ?? raw['sentry-id']),
        stackTrace: asString(raw.stackTrace ?? raw['stack-trace']),
        errors: normalizeErrors(raw.errors)
    }
}


function parseRequiredFields(raw: any) {
    return {
        type: asString(raw.type),
        title: asString(raw.title),
        status: asNumber(raw.status),
        detail: asString(raw.detail),
        traceId: asString(raw.traceId ?? raw['trace-id'])
    }
}


function validateRequiredFields(required: ReturnType<typeof parseRequiredFields>, raw: any): asserts required is Required<ReturnType<typeof parseRequiredFields>> {
    const missing = collectMissingFields(required.type, required.title, required.status, required.detail, required.traceId)

    if (missing.length > 0)
        throw new ProblemDetailsParseError(`Problem details missing required field(s): ${missing.join(', ')}`, raw)
}