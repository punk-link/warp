export interface RemoteGalleryItem {
    kind: 'remote'
    url: string
    name: string
    type?: string
    size?: number
    addedAt: number
}