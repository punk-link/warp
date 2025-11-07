import { describe, expect, it } from 'vitest'
import { buildTraceHeaders, ensureTraceContext, extractTraceIdFromHeaders } from '../telemetry/traceContext'


describe('traceContext utilities', () => {
    it('generates valid identifiers when none provided', () => {
        const context = ensureTraceContext()

        expect(context.traceId).toMatch(/^[0-9a-f]{32}$/)
        expect(context.spanId).toMatch(/^[0-9a-f]{16}$/)
        expect(context.traceParent).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/)
    })


    it('normalizes provided identifiers and honours sampling flag', () => {
        const context = ensureTraceContext({ traceId: 'ABCDEFABCDEFABCDEFABCDEFABCDEFAB', spanId: 'FEDCBA9876543210', sampled: false })

        expect(context.traceId).toBe('abcdefabcdefabcdefabcdefabcdefab')
        expect(context.spanId).toBe('fedcba9876543210')
        expect(context.traceParent.endsWith('-00')).toBe(true)
    })


    it('buildTraceHeaders enriches headers with trace metadata and accept header', () => {
        const { headers, context } = buildTraceHeaders({ ensureJsonAccept: true })

        expect(headers.get('accept')).toBe('application/json')
        expect(headers.get('traceparent')).toBe(context.traceParent)
        expect(headers.get('x-trace-id')).toBe(context.traceId)
    })


    it('extractTraceIdFromHeaders prefers explicit header and falls back to traceparent', () => {
        const { headers } = buildTraceHeaders()
        const directTraceId = headers.get('x-trace-id')!

        const extractedDirect = extractTraceIdFromHeaders(headers)
        expect(extractedDirect).toBe(directTraceId)

        const fallbackHeaders = new Headers({ traceparent: headers.get('traceparent')! })
        expect(extractTraceIdFromHeaders(fallbackHeaders)).toBe(directTraceId)
    })
})
