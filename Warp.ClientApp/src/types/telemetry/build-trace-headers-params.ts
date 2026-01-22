import { TraceContextInit } from "./trace-context-init"


/** Parameters for building trace-enriched request headers. */
export interface BuildTraceHeadersParams {
    headers?: HeadersInit
    traceContext?: TraceContextInit
    ensureJsonAccept?: boolean
}