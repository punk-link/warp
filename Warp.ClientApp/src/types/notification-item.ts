import type { NotificationAction } from './notification-action'
import { NotifyLevel } from './notify-level'


export interface NotificationItem extends Required<Pick<{ level: NotifyLevel; message: string }, 'level' | 'message'>> {
    id: string
    title?: string
    details?: string
    actions?: NotificationAction[]
    dedupeKey?: string
    sticky: boolean
    ttlMs: number
    createdAt: number
    expiresAt: number | null
    occurrences: number
}
