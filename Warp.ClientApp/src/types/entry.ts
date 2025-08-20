import { EditMode } from "./edit-mode";

export interface Entry { 
  id: string;
  editMode: EditMode;
  expirationPeriod: string;
  expiresAt: Date;
  images: string[] | [];
  textContent: string | '';
  viewCount: number | 0;
 }