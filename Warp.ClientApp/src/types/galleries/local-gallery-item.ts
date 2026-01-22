/** Represents a local gallery item (image file). */
export interface LocalGalleryItem {
    kind: 'local'
    file: File
    url: string
    name: string
    type: string
    size: number
    addedAt: number
}