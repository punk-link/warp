/** Represents a fully materialized trace context for an outgoing request. */
export interface TraceContext {
    traceId: string
    spanId: string
    traceParent: string
    traceState?: string
}