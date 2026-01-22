import { TraceContext } from "../types/telemetry/trace-context"
import { TraceContextInit } from "../types/telemetry/trace-context-init"
import { BuildTraceHeadersParams } from "../types/telemetry/build-trace-headers-params"

const TRACE_VERSION = '00'
const DEFAULT_TRACE_FLAGS = '01'


/** Trace identifier response header. */
export const TRACE_ID_HEADER = 'x-trace-id'

/** W3C traceparent header name. */
export const TRACEPARENT_HEADER = 'traceparent'

/** W3C tracestate header name. */
export const TRACESTATE_HEADER = 'tracestate'


/** Applies trace headers to the provided headers instance. */
export function applyTraceHeaders(target: Headers, context: TraceContext): void {
    target.set(TRACEPARENT_HEADER, context.traceParent)
    target.set(TRACE_ID_HEADER, context.traceId)

    if (context.traceState)
        target.set(TRACESTATE_HEADER, context.traceState)
}


/** Builds request headers enriched with trace context and optional defaults. */
export function buildTraceHeaders(params: BuildTraceHeadersParams = {}): { headers: Headers; context: TraceContext } {
    const { headers: initHeaders, traceContext, ensureJsonAccept } = params
    const headers = new Headers(initHeaders ?? undefined)

    if (ensureJsonAccept && !headers.has('Accept'))
        headers.set('Accept', 'application/json')

    const context = ensureTraceContext(traceContext)
    applyTraceHeaders(headers, context)

    return { headers, context }
}


/** Ensures a valid trace context exists by reusing the provided identifiers when possible. */
export function ensureTraceContext(init?: TraceContextInit): TraceContext {
    const traceId = normalizeTraceId(init?.traceId) ?? generateTraceId()
    const spanId = normalizeSpanId(init?.spanId) ?? generateSpanId()
    const sampled = init?.sampled === false ? '00' : DEFAULT_TRACE_FLAGS
    const traceParent = `${TRACE_VERSION}-${traceId}-${spanId}-${sampled}`

    return {
        traceId,
        spanId,
        traceParent,
        traceState: init?.traceState?.trim() || undefined
    }
}


/** Extracts the trace identifier from a response headers collection. */
export function extractTraceIdFromHeaders(headers: Headers): string | null {
    const direct = headers.get(TRACE_ID_HEADER) ?? headers.get('trace-id')
    const normalizedDirect = normalizeTraceId(direct ?? undefined)
    if (normalizedDirect)
        return normalizedDirect

    const fromTraceParent = parseTraceParent(headers.get(TRACEPARENT_HEADER))
    return fromTraceParent
}


const HEX_LOOKUP: string[] = Array.from({ length: 256 }, (_, index) => index.toString(16).padStart(2, '0'))


function generateRandomBytes(length: number): Uint8Array {
    const bytes = new Uint8Array(length)
    if (typeof crypto !== 'undefined' && typeof crypto.getRandomValues === 'function') {
        crypto.getRandomValues(bytes)
        
        return bytes
    }

    for (let i = 0; i < length; i += 1)
        bytes[i] = Math.floor(Math.random() * 256)

    return bytes
}


function convertBytesToHex(bytes: Uint8Array): string {
    let result = ''
    for (let i = 0; i < bytes.length; i += 1)
        result += HEX_LOOKUP[bytes[i]]

    return result
}


function generateTraceId(): string {
    let traceId = convertBytesToHex(generateRandomBytes(16))
    if (/^0+$/.test(traceId))
        traceId = convertBytesToHex(generateRandomBytes(16))

    return traceId
}


function generateSpanId(): string {
    let spanId = convertBytesToHex(generateRandomBytes(8))
    if (/^0+$/.test(spanId))
        spanId = convertBytesToHex(generateRandomBytes(8))

    return spanId
}


function isValidHex(value: string | undefined, length: number): value is string {
    if (!value)
        return false

    return new RegExp(`^[0-9a-fA-F]{${length}}$`).test(value)
}


function normalizeTraceId(value?: string): string | undefined {
    const candidate = value?.trim()
    if (!isValidHex(candidate, 32))
        return undefined

    const normalized = candidate!.toLowerCase()
    return /^0+$/.test(normalized) ? undefined : normalized
}


function normalizeSpanId(value?: string): string | undefined {
    const candidate = value?.trim()
    if (!isValidHex(candidate, 16))
        return undefined

    const normalized = candidate!.toLowerCase()
    return /^0+$/.test(normalized) ? undefined : normalized
}


function parseTraceParent(value: string | null): string | null {
    if (!value)
        return null

    const trimmed = value.trim()
    if (!trimmed)
        return null

    const segments = trimmed.split('-')
    if (segments.length < 4)
        return null

    const traceId = segments[1]
    return normalizeTraceId(traceId) ?? null
}