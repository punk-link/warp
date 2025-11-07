import type { TraceContextInit } from '../telemetry/traceContext'
import { NotifyLevel } from './notify-level'
import { ErrorHandlingMode } from './error-handling-mode'


/**
 * Application-specific request options extending the standard Fetch RequestInit.
 * Used by fetchJson to control global error handling and notification behavior.
 */
export interface AppRequestInit extends RequestInit {
    errorHandling?: ErrorHandlingMode
    notifyLevel?: NotifyLevel
    dedupeKey?: string
    context?: { feature?: string; i18nKey?: string; data?: Record<string, unknown> }
    traceContext?: TraceContextInit
}
