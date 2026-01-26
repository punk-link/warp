import type { Router } from 'vue-router'
import { registerErrorBridge } from './fetch-service'
import { buildDedupeKey, defaultNotifyLevel, isValidation } from './error-policy'
import type { ApiError } from '../types/apis/api-error'
import type { AppRequestInit } from '../types/apis/app-request-init'
import { useNotifications } from '../composables/use-notifications'
import { ErrorHandlingMode } from '../types/apis/enums/error-handling-mode'
import { NotificationLevel } from '../types/notifications/enums/notification-level'
import { emitApiErrorTelemetry } from '../telemetry/clientTelemetry'
import { tOr } from '../i18n'
import { ViewNames } from '../router/enums/view-names'


/** Sets up the default error bridge with the specified router. */
export function setupDefaultErrorBridge(router: Router): void {
    registerErrorBridge((error: ApiError, req: AppRequestInit) => {
        const mode = req.errorHandling ?? ErrorHandlingMode.Global
        if (mode !== ErrorHandlingMode.Global)
            return

        const dedupeKey = req.dedupeKey || buildDedupeKey(error.method, error.endpoint, error.status)

        emitApiErrorTelemetry({
            handledBy: mode,
            method: error.method,
            endpoint: error.endpoint,
            status: error.status,
            requestId: error.requestId,
            traceId: error.traceId,
            eventId: error.eventId,
            sentryId: error.sentryId,
            dedupeKey,
            notifyLevel: req.notifyLevel ?? defaultNotifyLevel(error.status)
        })

        const redirected = handleRedirect(router, error.status)
        if (!redirected)
            pushNotification(error, req, dedupeKey)
    })
}


function buildActions(error: ApiError) {
    return [{
        label: tOr('components.notifications.copyDetails', 'Copy details'),
        title: tOr('components.notifications.copyDiagnostics', 'Copy diagnostic identifiers'),
        onClick: () => copyDiagnostics(error)
    }]
}


function buildDetails(error: ApiError): string | undefined {
    const problem = error.problem
    if (!problem?.detail)
        return undefined

    const lines: string[] = []
    
    if (problem.eventId != null)
        lines.push(`Event ID: ${problem.eventId}`)

    if (problem.traceId)
        lines.push(`Trace ID: ${problem.traceId}`)

    return lines.length > 0 ? lines.join('\n') : undefined
}


function buildMessage(error: ApiError): string | undefined {
    if (error.problem)
        return error.problem.detail || undefined

    const errMsg = error.message?.trim()
    const fallbackTitle = tOr(statusToKey(error.status), 'Request failed')
    
    return errMsg && errMsg !== fallbackTitle ? errMsg : undefined
}


function buildTitle(error: ApiError): string {
    const problem = error.problem
    if (!problem)
        return tOr(statusToKey(error.status), error.message || 'Request failed')

    const baseTitle = problem.title || tOr(statusToKey(error.status), 'Request failed')
    
    if (problem.status != null)
        return `${problem.status}: ${baseTitle}`

    return baseTitle
}


function copyDiagnostics(error: ApiError): void {
    const parts: string[] = []
    if (error.traceId)
        parts.push(`traceId=${error.traceId}`)

    if (error.requestId)
        parts.push(`requestId=${error.requestId}`)

    if (error.eventId != null)
        parts.push(`eventId=${error.eventId}`)

    if (error.sentryId)
        parts.push(`sentryId=${error.sentryId}`)

    const text = parts.join(' ')
    if (text)
        void navigator.clipboard?.writeText(text)
}


function handleRedirect(router: Router, status: number | undefined | null): boolean {
    if (status == null)
        return false

    const current = router.currentRoute.value
    if (status === 404) {
        if (current.name !== ViewNames.NotFound)
            router.replace({ name: ViewNames.NotFound })

        return true
    }

    if (status >= 500 && status <= 599) {
        if (current.name !== ViewNames.Error)
            router.replace({ name: ViewNames.Error })

        return true
    }

    return false
}


function pushNotification(error: ApiError, req: AppRequestInit, dedupeKey: string): void {
    const { push } = useNotifications()
    const level = req.notifyLevel ?? defaultNotifyLevel(error.status)

    push({
        level,
        title: buildTitle(error),
        message: buildMessage(error),
        details: buildDetails(error),
        dedupeKey,
        actions: level >= NotificationLevel.Warn 
            ? buildActions(error) 
            : undefined
    })
}


function statusToKey(status?: number | null): string {
    if (status == null || status === 0)
        return 'api.error.network'

    switch (status) {
        case 400: return 'api.error._400'
        case 401: return 'api.error._401'
        case 403: return 'api.error._403'
        case 404: return 'api.error._404'
        case 409: return 'api.error._409'
        case 422: return 'api.error._422'
        case 429: return 'api.error._429'
        default: return 'api.error.generic'
    }
}
