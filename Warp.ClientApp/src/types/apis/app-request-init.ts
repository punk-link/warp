import { NotificationLevel } from '../notifications/enums/notification-level'
import { ErrorHandlingMode } from './enums/error-handling-mode'
import { TraceContextInit } from '../telemetry/trace-context-init'


/**
 * Application-specific request options extending the standard Fetch RequestInit.
 * Used by fetchJson to control global error handling and notification behavior.
 */
export interface AppRequestInit extends RequestInit {
    errorHandling?: ErrorHandlingMode
    notifyLevel?: NotificationLevel
    dedupeKey?: string
    context?: { feature?: string; i18nKey?: string; data?: Record<string, unknown> }
    traceContext?: TraceContextInit
}
