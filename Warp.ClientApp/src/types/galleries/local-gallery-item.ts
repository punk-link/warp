export interface LocalGalleryItem {
    kind: 'local'
    file: File
    url: string
    name: string
    type: string
    size: number
    addedAt: number
}