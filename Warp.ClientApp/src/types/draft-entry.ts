import { EditMode } from "./edit-mode";

export interface DraftEntry { 
  id: string;
  editMode: EditMode;
  expirationPeriod: string;
  images: string[] | [];
  textContent: string | '';
 }