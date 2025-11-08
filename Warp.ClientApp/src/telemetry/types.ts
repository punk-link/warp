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
