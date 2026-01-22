/** Describes optional overrides when generating a trace context. */
export interface TraceContextInit {
    traceId?: string
    spanId?: string
    traceState?: string
    sampled?: boolean
}