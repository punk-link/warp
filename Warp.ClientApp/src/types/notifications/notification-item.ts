import type { NotificationAction } from './notification-action'
import { NotificationLevel } from './enums/notification-level'


/** Represents a notification item with its properties. */
export interface NotificationItem extends Required<Pick<{ level: NotificationLevel; message: string }, 'level' | 'message'>> {
    id: string
    title?: string
    details?: string
    traceId?: string
    diagnostics?: string
    actions?: NotificationAction[]
    dedupeKey?: string
    sticky: boolean
    ttlMs: number
    createdAt: number
    expiresAt: number | null
    occurrences: number
    onRetry?: () => void | Promise<void>
    showAsOverlay: boolean
}
