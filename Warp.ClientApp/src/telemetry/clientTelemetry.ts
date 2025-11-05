import { ErrorHandlingMode } from '../types/error-handling-mode'
import { NotifyLevel } from '../types/notify-level'


export interface ApiErrorTelemetry {
    category: 'api-error'
    method?: string
    endpoint?: string
    status?: number
    handledBy: ErrorHandlingMode
    requestId?: string | null
    traceId?: string | null
    eventId?: number
    sentryId?: string | null
    dedupeKey?: string
    notifyLevel?: NotifyLevel
}


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

    try {
        window.dispatchEvent(new CustomEvent('telemetry', { detail: data }))
    } catch {
        // ignore
    }

    if (import.meta && import.meta.env && import.meta.env.DEV)
        // eslint-disable-next-line no-console
        console.debug('[telemetry]', data)
}
