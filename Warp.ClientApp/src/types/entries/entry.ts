import { EditMode } from "./enums/edit-modes";
import { ExpirationPeriod } from "./enums/expiration-periods";


/** Represents a finalized entry with its properties. */
export interface Entry { 
    id: string;
    editMode: EditMode;
    expirationPeriod: ExpirationPeriod;
    expiresAt: Date;
    images: string[] | [];
    textContent: string | '';
    contentDelta?: string;
    viewCount: number | 0;
}