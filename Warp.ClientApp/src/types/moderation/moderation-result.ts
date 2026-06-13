import type { ModerationStatus } from './enums/moderation-status'


/** Mirrors the server-side ModerationResult record struct except for the actual list of scores, which is not needed on the client. */
export interface ModerationResult {
    completedAt?: string;
    isFlagged: boolean;
    status: ModerationStatus;
}
