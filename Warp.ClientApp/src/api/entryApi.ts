import { Entry } from '../types/entry';
import { fetchJson } from './fetchHelper'

export interface EntryCreateResponse { id: string; previewUrl?: string }

// Server pattern: GET /api/entries returns a new entry shell with id. Then POST /api/entries/{id}
// with JSON payload (AddOrUpdate) and optional separate image uploads.

export interface EntryAddOrUpdateRequest {
    editMode: number | string
    expirationPeriod: number | string
    textContent: string
    imageIds: string[]
}


export async function addOrUpdateEntry(id: string, payload: EntryAddOrUpdateRequest, files: File[]) {
    const form = new FormData()

    form.append('editMode', String(payload.editMode))
    form.append('expirationPeriod', String(payload.expirationPeriod))
    form.append('textContent', payload.textContent)

    for (const file of files) 
        form.append('images', file, file.name)

    return fetchJson<EntryCreateResponse>(`/api/entries/${encodeURIComponent(id)}`, {
        method: 'POST',
        body: form
    })
}


export async function getEntry(id?: string): Promise<Entry> {
    if (!id)
        return fetchJson<Entry>(`/api/entries`)

    return fetchJson<Entry>(`/api/entries/${encodeURIComponent(id!)}`)
}


export async function deleteEntry(id: string) {
    return fetchJson(`/api/entries/${encodeURIComponent(id)}`, { method: 'DELETE' })
}

export const entryApi = { addOrUpdateEntry, getEntry, deleteEntry }
