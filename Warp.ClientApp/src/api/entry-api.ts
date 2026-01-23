import { Entry } from '../types/entries/entry';
import { fetchJson } from './fetch-service'
import { API_BASE } from './constants'
import { routeApiError } from './error-routing'
import { EntryAddOrUpdateRequest } from '../types/apis/entries/entry-add-or-update-request';
import { EntryCreateResponse } from '../types/apis/entries/entry-create-response';
import { EntryCopyResponse } from '../types/apis/entries/entry-copy-response';

// Server pattern: GET /api/entries returns a new entry shell with id. Then POST /api/entries/{id}
// with JSON payload (AddOrUpdate) and optional separate image uploads.

/** Adds or updates an entry with the specified ID, payload, and image files. */
export async function addOrUpdateEntry(id: string, payload: EntryAddOrUpdateRequest, files: File[]) {
    const form = new FormData()

    form.append('editMode', String(payload.editMode))
    form.append('expirationPeriod', String(payload.expirationPeriod))
    form.append('textContent', payload.textContent)

    for (const file of files) 
        form.append('images', file, file.name)

    try {
        return await fetchJson<EntryCreateResponse>(`${API_BASE}/entries/${encodeURIComponent(id)}`, {
            method: 'POST',
            body: form
        })
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** Copies an entry by ID. */
export async function copyEntry(id: string): Promise<EntryCopyResponse> {
    try {
        return await fetchJson<EntryCopyResponse>(`${API_BASE}/entries/${encodeURIComponent(id)}/copy`, {
            method: 'POST'
        })
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** Deletes an entry by ID. */
export async function deleteEntry(id: string): Promise<void> {
    try {
        return await fetchJson(`${API_BASE}/entries/${encodeURIComponent(id)}`, { method: 'DELETE' })
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** Gets an entry by ID, or a new entry shell if no ID is provided. */
export async function getEntry(id?: string): Promise<Entry> {
    try {
        if (!id)
            return await fetchJson<Entry>(`${API_BASE}/entries`)

        return await fetchJson<Entry>(`${API_BASE}/entries/${encodeURIComponent(id!)}`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** Reports an entry for inappropriate content or other issues. */
export async function reportEntry(id: string): Promise<void> {
    try {
        return await fetchJson(`${API_BASE}/entries/${encodeURIComponent(id)}/report`, { method: 'POST' })
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** API methods related to entries. */
export const entryApi = { addOrUpdateEntry, copyEntry, getEntry, deleteEntry, reportEntry }
