import type { ModerationResult } from '../moderation/moderation-result'


/** Image metadata returned by the server, including viewer-specific blur state. */
export interface ImageInfoResponse {
    id: string;
    entryId: string;
    isBlurred: boolean;
    moderationResult?: ModerationResult;
    url: string;
}
