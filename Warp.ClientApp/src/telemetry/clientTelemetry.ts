import { reportApiErrorToSentry } from './sentry'
import type { ApiErrorTelemetry } from './types'


export type { ApiErrorTelemetry } from './types'


function sanitizeEndpoint(url: string | undefined): string | undefined {
    if (!url)
        return url

    try {
        const u = new URL(url, window.location.origin)
        return u.pathname
    } catch {
        return url.replace(/\?.*$/, '')
    }
}


export function emitApiErrorTelemetry(payload: Omit<ApiErrorTelemetry, 'category'>): void {
    const data: ApiErrorTelemetry = {
        category: 'api-error',
        ...payload,
        endpoint: sanitizeEndpoint(payload.endpoint)
    }

    reportApiErrorToSentry(data)

    try {
        window.dispatchEvent(new CustomEvent('telemetry', { detail: data }))
    } catch {
        // ignore
    }

    if (import.meta && import.meta.env && import.meta.env.DEV)
        // eslint-disable-next-line no-console
        console.debug('[telemetry]', data)
}
