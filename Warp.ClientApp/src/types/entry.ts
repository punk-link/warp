import { EditMode } from "./edit-modes";
import { ExpirationPeriod } from './expiration-periods'

export interface Entry { 
  id: string;
  editMode: EditMode;
  expirationPeriod: ExpirationPeriod;
  expiresAt: Date;
  images: string[] | [];
  textContent: string | '';
  viewCount: number | 0;
 }