import { ref } from 'vue'
import { fetchJson } from '../api/fetchHelper'
import type { ExpirationOption } from '../types/expiration'

async function convertToArray<T>(data: any): Promise<T[]> {
  if (Array.isArray(data)) 
    return data.map((v: T) => ({ value: v, label: v })) as T[]

  return Object.entries(data as Record<string, string>)
    .map(([k, v]) => ({ key: Number(k), label: v, rawKey: k }))
    .sort((a, b) => a.key - b.key)
    .map(e => ({ value: String(e.key), label: e.label })) as T[]
}


export interface EditModes { value: string; label: string }


export function useMetadata() {
  const editModes = ref<EditModes[]>([])
  const expirationOptions = ref<ExpirationOption[]>([])

  
  async function loadEditModes() {
    if (editModes.value.length) 
      return editModes.value

    try {
      const json = await fetchJson('/api/metadata/enums/edit-modes')
      editModes.value = await convertToArray<EditModes>(json)
    } catch {
      console.error('Failed to load edit modes')
    }

    return editModes.value
  }


  async function loadExpirationOptions() {
    if (expirationOptions.value.length) 
      return expirationOptions.value

    try {
      const json = await fetchJson('/api/metadata/enums/expiration-periods')
      expirationOptions.value = await convertToArray<ExpirationOption>(json)
    } catch {
      console.error('Failed to load expiration options')
    }

    return expirationOptions.value
  }

  return { editModes, expirationOptions, loadEditModes, loadExpirationOptions }
}
