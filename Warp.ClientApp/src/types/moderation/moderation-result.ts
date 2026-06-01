import type { ModerationStatus } from './enums/moderation-status'


/** Mirrors the server-side ModerationResult record struct. */
export interface ModerationResult {
    completedAt?: string;
    isFlagged: boolean;
    status: ModerationStatus;
}
