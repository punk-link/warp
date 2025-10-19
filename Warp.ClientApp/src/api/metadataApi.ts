import { fetchJson } from './fetchHelper'
import { API_BASE } from './constants'
import { routeApiError } from './errorRouting'


export async function getExpirationPeriods() {
    try {
        return await fetchJson<Record<string, string>>(`${API_BASE}/metadata/enums/expiration-periods`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


export async function getEditModes() {
    try {
        return await fetchJson<Record<string, string>>(`${API_BASE}/metadata/enums/edit-modes`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


export const metadataApi = { getExpirationPeriods, getEditModes }
