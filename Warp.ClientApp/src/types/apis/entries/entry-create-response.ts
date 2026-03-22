import type { ImageInfoResponse } from '../../images/image-info-response'

export interface EntryCreateResponse { 
    id: string; 
    previewUrl?: string;
    excludedImages?: ImageInfoResponse[];
    rejectedFiles?: string[];
}
