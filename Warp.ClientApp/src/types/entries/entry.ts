import { EditMode } from './enums/edit-modes'
import { ExpirationPeriod } from './enums/expiration-periods'
import type { ImageInfoResponse } from '../images/image-info-response'


/** Represents a finalized entry with its properties. */
export interface Entry {
    id: string;
    editMode: EditMode;
    expirationPeriod: ExpirationPeriod;
    expiresAt: Date;
    images: ImageInfoResponse[];
    textContent: string;
    contentDelta?: string;
    viewCount: number;
    isTextBlurred: boolean;
}