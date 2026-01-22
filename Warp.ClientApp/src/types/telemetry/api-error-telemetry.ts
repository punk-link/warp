import { ErrorHandlingMode } from '../apis/enums/error-handling-mode'
import { NotificationLevel } from '../notifications/enums/notification-level'


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
    notifyLevel?: NotificationLevel
}
