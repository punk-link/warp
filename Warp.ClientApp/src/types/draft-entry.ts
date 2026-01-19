import { EditMode } from "./edit-modes";
import { ExpirationPeriod } from './expiration-periods'


export interface DraftEntry { 
    id: string;
    editMode: EditMode;
    expirationPeriod: ExpirationPeriod;
    images: string[] | [];
    textContent: string | '';
}