export interface EntryAddOrUpdateRequest {
    editMode: number | string
    expirationPeriod: number | string
    textContent: string
    imageIds: string[]
}