/**
 * Central error taxonomy for API responses.
 *
 * Defines which statuses should redirect to an error page vs surface a frontend notification,
 * and provides defaults for notification severity and deduplication.
 */
import { NotifyLevel } from '../types/notify-level'
import { ErrorClassification } from '../types/error-classification'


export function shouldRedirect(status: number | undefined | null): boolean {
    if (status == null)
        return false

    if (status === 404)
        return true

    return status >= 500 && status <= 599
}


export function isValidation(status: number | undefined | null): boolean {
    return status === 422
}


export function classify(status: number | undefined | null): ErrorClassification {
    return shouldRedirect(status)
        ? ErrorClassification.Redirect 
        : ErrorClassification.Notify
}


export function defaultNotifyLevel(status: number | undefined | null): NotifyLevel {
    if (status == null)
        return NotifyLevel.Error

    if (status === 404 || (status >= 500 && status <= 599))
        return NotifyLevel.Error

    if (status >= 400 && status <= 499)
        return NotifyLevel.Warn

    return NotifyLevel.Error
}


export function buildDedupeKey(method: string | undefined, url: string | undefined, status: number | undefined | null): string {
    const m = (method || 'GET').toUpperCase()
    const u = (url || '').replace(/\?.*$/, '') // strip query string for dedupe
    const s = status == null ? 'net' : String(status)

    return `${m} ${u} -> ${s}`
}


export function statusBucket(status: number | undefined | null): 'network' | '4xx' | '404' | '5xx' | 'ok' | 'other' {
    if (status == null)
        return 'network'

    if (status === 404)
        return '404'

    if (status >= 500 && status <= 599)
        return '5xx'

    if (status >= 400 && status <= 499)
        return '4xx'

    if (status >= 200 && status <= 299)
        return 'ok'

    return 'other'
}
