import { ErrorHandlingMode } from '../error-handling-mode'
import { NotifyLevel } from '../notify-level'


/** Telemetry event emitted when an API error occurs. */
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
