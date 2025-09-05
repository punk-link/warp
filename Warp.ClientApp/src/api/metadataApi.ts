import { fetchJson } from './fetchHelper'
import { API_BASE } from './constants'


export async function getExpirationPeriods() {
    return fetchJson<Record<string, string>>(`${API_BASE}/metadata/enums/expiration-periods`)
}


export async function getEditModes() {
    return fetchJson<Record<string, string>>(`${API_BASE}/metadata/enums/edit-modes`)
}


export const metadataApi = { getExpirationPeriods, getEditModes }
