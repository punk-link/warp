import type { Router } from 'vue-router'
import { registerErrorBridge } from './fetchHelper'
import { buildDedupeKey, defaultNotifyLevel, isValidation } from './errorPolicy'
import type { ApiError } from '../types/api-error'
import type { AppRequestInit } from '../types/app-request-init'
import { useNotifications } from '../composables/useNotifications'
import { ErrorHandlingMode } from '../types/error-handling-mode'
import { NotifyLevel } from '../types/notify-level'
import { emitApiErrorTelemetry } from '../telemetry/clientTelemetry'
import { tOr } from '../i18n'


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


function pushNotification(error: ApiError, req: AppRequestInit, dedupeKey: string): void {
    const { push } = useNotifications()

    const level = req.notifyLevel ?? defaultNotifyLevel(error.status)
    const serverMessage = error.problem?.detail || error.problem?.title || error.message || 'Request failed'

    const actions = [
        {
            label: tOr('components.notifications.copyDetails', 'Copy details'),
            title: tOr('components.notifications.copyDiagnostics', 'Copy diagnostic identifiers'),
            onClick: () => copyDiagnostics(error)
        }
    ]

    const briefMessage = isValidation(error.status) && error.problem?.title 
        ? error.problem.title 
        : serverMessage

    let title = tOr(statusToKey(error.status), briefMessage)
    let message: string | undefined
    let details: string | undefined
    
    if (error.problem) {
        const p = error.problem

        if (p.status != null && p.title)
            title = `${p.status}: ${p.title}`
        else if (p.status != null)
            title = `${p.status}: ${title}`

        message = p.detail || undefined

        if (p.detail) {
            const lines: string[] = []
            if (p.eventId != null)
                lines.push(`Event ID: ${p.eventId}`)

            if (p.traceId)
                lines.push(`Trace ID: ${p.traceId}`)
            
            details = lines.join('\n').trim() || undefined
        }
    } else {
        const errMsg = error.message?.trim()
        message = errMsg && errMsg !== title ? errMsg : ''
    }

    push({
        level,
        message: message,
        title: title,
        details: details || undefined,
        dedupeKey,
        actions: level >= NotifyLevel.Warn ? actions : undefined
    })
}


function handleRedirect(router: Router, status: number | undefined | null): boolean {
    if (status == null)
        return false

    const current = router.currentRoute.value
    if (status === 404) {
        if (current.name !== 'NotFound')
            router.replace({ name: 'NotFound' })

        return true
    }

    if (status >= 500 && status <= 599) {
        if (current.name !== 'Error')
            router.replace({ name: 'Error' })

        return true
    }

    return false
}


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
