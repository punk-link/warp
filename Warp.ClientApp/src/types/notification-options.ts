import { NotifyLevel } from './notify-level'
import type { NotificationAction } from './notification-action'


export interface NotificationOptions {
    level: NotifyLevel
    message: string
    title?: string
    details?: string
    dedupeKey?: string
    ttlMs?: number
    sticky?: boolean
    actions?: NotificationAction[]
}
