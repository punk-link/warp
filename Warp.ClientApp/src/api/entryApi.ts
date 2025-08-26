import { Entry } from '../types/entry';
import { fetchJson } from './fetchHelper'

export interface EntryCreateResponse { id: string; previewUrl?: string }

export async function createEntry(form: FormData) {
  return fetchJson<EntryCreateResponse>('/api/entries', { method: 'POST', body: form })
}

export async function getEntry(id?: string): Promise<Entry> {
  if (!id)
    return fetchJson<Entry>(`/api/entries`)

  return fetchJson<Entry>(`/api/entries/${encodeURIComponent(id!)}`)
}

export async function updateEntry(id: string, form: FormData) {
  return fetchJson(`/api/entries/${encodeURIComponent(id)}`, { method: 'PUT', body: form })
}

export async function deleteEntry(id: string) {
  return fetchJson(`/api/entries/${encodeURIComponent(id)}`, { method: 'DELETE' })
}

export const entryApi = { createEntry, getEntry, updateEntry, deleteEntry }
