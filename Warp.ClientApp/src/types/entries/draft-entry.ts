import { EditMode } from "./enums/edit-modes";
import { ExpirationPeriod } from "./enums/expiration-periods";


/** * Represents a draft entry with its properties. */
export interface DraftEntry { 
    id: string;
    editMode: EditMode;
    expirationPeriod: ExpirationPeriod;
    images: string[] | [];
    textContent: string | '';
    contentDelta?: string;
}