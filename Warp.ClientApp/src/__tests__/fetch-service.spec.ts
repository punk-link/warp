import { afterEach, beforeEach, describe, it, expect, vi } from 'vitest'
import { fetchJson } from '../api/fetch-service'


function makeOkTextResponse(): Response {
    return {
        ok: true,
        status: 200,
        statusText: 'OK',
        headers: new Headers({ 'content-type': 'text/plain' }),
        text: () => Promise.resolve(''),
        json: () => Promise.resolve({})
    } as unknown as Response
}


function setCsrfCookie(value: string) {
    document.cookie = `XSRF-TOKEN=${encodeURIComponent(value)}; path=/`
}


function clearCsrfCookie() {
    document.cookie = 'XSRF-TOKEN=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/'
}


describe('fetchJson CSRF header injection', () => {
    let fetchSpy: ReturnType<typeof vi.fn>

    beforeEach(() => {
        fetchSpy = vi.fn().mockResolvedValue(makeOkTextResponse())
        global.fetch = fetchSpy
        clearCsrfCookie()
    })

    afterEach(() => {
        vi.restoreAllMocks()
        clearCsrfCookie()
    })


    it('injects X-CSRF-TOKEN header on POST when cookie is present', async () => {
        setCsrfCookie('my-csrf-token')

        await fetchJson('/api/test', { method: 'POST' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBe('my-csrf-token')
    })


    it('injects X-CSRF-TOKEN header on PUT when cookie is present', async () => {
        setCsrfCookie('my-csrf-token')

        await fetchJson('/api/test', { method: 'PUT' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBe('my-csrf-token')
    })


    it('injects X-CSRF-TOKEN header on DELETE when cookie is present', async () => {
        setCsrfCookie('my-csrf-token')

        await fetchJson('/api/test', { method: 'DELETE' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBe('my-csrf-token')
    })


    it('does not inject X-CSRF-TOKEN header on GET', async () => {
        setCsrfCookie('my-csrf-token')

        await fetchJson('/api/test', { method: 'GET' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBeNull()
    })


    it('does not inject X-CSRF-TOKEN header on HEAD', async () => {
        setCsrfCookie('my-csrf-token')

        await fetchJson('/api/test', { method: 'HEAD' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBeNull()
    })


    it('does not inject X-CSRF-TOKEN header when cookie is absent', async () => {
        await fetchJson('/api/test', { method: 'POST' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBeNull()
    })


    it('decodes a URL-encoded token from the cookie', async () => {
        setCsrfCookie('hello world')

        await fetchJson('/api/test', { method: 'POST' })

        const [, options] = fetchSpy.mock.calls[0] as [string, RequestInit]
        expect((options.headers as Headers).get('X-CSRF-TOKEN')).toBe('hello world')
    })
})
