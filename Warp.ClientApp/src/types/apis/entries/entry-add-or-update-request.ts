export interface EntryAddOrUpdateRequest {
    editMode: number | string
    expirationPeriod: number | string
    textContent: string
    contentDelta?: string
    imageIds: string[]
}