/**
 * Central error taxonomy for API responses.
 *
 * Defines which statuses should redirect to an error page vs surface a frontend notification,
 * and provides defaults for notification severity and deduplication.
 */
import { NotificationLevel } from '../types/notifications/enums/notification-level'
import { RedirectAction } from '../types/apis/enums/redirect-action'


/** Builds a deduplication key for the specified request parameters. */
export function buildDedupeKey(method: string | undefined, url: string | undefined, status: number | undefined | null): string {
    const normalizedMethod = (method || 'GET').toUpperCase()
    const normalizedUrl = (url || '').replace(/\?.*$/, '') // strip query string for dedupe
    const normalizedStatus = status == null ? 'net' : String(status)

    return `${normalizedMethod} ${normalizedUrl} -> ${normalizedStatus}`
}


/** Classifies the specified status into a redirect action. */
export function classify(status: number | undefined | null): RedirectAction {
    return shouldRedirect(status)
        ? RedirectAction.Redirect 
        : RedirectAction.Notify
}


/** Determines the default notification level for the specified status. */
export function defaultNotifyLevel(status: number | undefined | null): NotificationLevel {
    if (status == null)
        return NotificationLevel.Error

    if (status === 404 || (status >= 500 && status <= 599))
        return NotificationLevel.Error

    if (status >= 400 && status <= 499)
        return NotificationLevel.Warn

    return NotificationLevel.Error
}


/** Determines whether the specified status represents a validation error. */
export function isValidation(status: number | undefined | null): boolean {
    return status === 422
}


/** Determines whether the specified status should trigger a redirect. */
export function shouldRedirect(status: number | undefined | null): boolean {
    if (status == null)
        return false

    if (status === 404)
        return true

    return status >= 500 && status <= 599
}


/** Buckets the specified status into a high-level category. */
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
