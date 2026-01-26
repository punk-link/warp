import { NotificationLevel } from './enums/notification-level'
import type { NotificationAction } from './notification-action'


/** Represents options for creating a notification. */
export interface NotificationOptions {
    level: NotificationLevel
    message?: string
    title: string
    details?: string
    dedupeKey?: string
    ttlMs?: number
    sticky?: boolean
    actions?: NotificationAction[]
}
