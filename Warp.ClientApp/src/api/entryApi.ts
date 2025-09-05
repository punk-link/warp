import { Entry } from '../types/entry';
import { fetchJson } from './fetchHelper'
import { API_BASE } from './constants'

// Server pattern: GET /api/entries returns a new entry shell with id. Then POST /api/entries/{id}
// with JSON payload (AddOrUpdate) and optional separate image uploads.

export interface EntryAddOrUpdateRequest {
    editMode: number | string
    expirationPeriod: number | string
    textContent: string
    imageIds: string[]
}


export interface EntryCreateResponse { id: string; previewUrl?: string }


export interface EntryCopyResponse { id: string; }


export async function addOrUpdateEntry(id: string, payload: EntryAddOrUpdateRequest, files: File[]) {
    const form = new FormData()

    form.append('editMode', String(payload.editMode))
    form.append('expirationPeriod', String(payload.expirationPeriod))
    form.append('textContent', payload.textContent)

    for (const file of files) 
        form.append('images', file, file.name)

    return await fetchJson<EntryCreateResponse>(`${API_BASE}/entries/${encodeURIComponent(id)}`, {
        method: 'POST',
        body: form
    })
}


export async function copyEntry(id: string): Promise<EntryCopyResponse> {
    return await fetchJson<EntryCopyResponse>(`${API_BASE}/entries/${encodeURIComponent(id)}/copy`, {
        method: 'POST'
    })
}


export async function getEntry(id?: string): Promise<Entry> {
    if (!id)
        return await fetchJson<Entry>(`${API_BASE}/entries`)

    return await fetchJson<Entry>(`${API_BASE}/entries/${encodeURIComponent(id!)}`)
}


export async function deleteEntry(id: string): Promise<void> {
    return await fetchJson(`${API_BASE}/entries/${encodeURIComponent(id)}`, { method: 'DELETE' })
}


export async function reportEntry(id: string): Promise<void> {
    return await fetchJson(`${API_BASE}/entries/${encodeURIComponent(id)}/report`, { method: 'POST' })
}


export const entryApi = { addOrUpdateEntry, copyEntry, getEntry, deleteEntry, reportEntry }
