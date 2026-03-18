import { ApiErrorTelemetry } from '../types/telemetry/api-error-telemetry'
import { reportApiErrorToSentry } from './sentry'


export type { ApiErrorTelemetry } from '../types/telemetry/api-error-telemetry'


/** Emits an API error telemetry event. */
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


function sanitizeEndpoint(url: string | undefined): string | undefined {
    if (!url)
        return url

    try {
        const newUrl = new URL(url, window.location.origin)
        return newUrl.pathname
    } catch {
        return url.replace(/\?.*$/, '')
    }
}
