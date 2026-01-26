import { fetchJson } from './fetch-service'
import { API_BASE } from './constants'
import { routeApiError } from './error-routing'


/** Fetches the expiration periods enum from the API. */
export async function getExpirationPeriods() {
    try {
        return await fetchJson<Record<string, string>>(`${API_BASE}/metadata/enums/expiration-periods`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** Fetches the edit modes enum from the API. */
export async function getEditModes() {
    try {
        return await fetchJson<Record<string, string>>(`${API_BASE}/metadata/enums/edit-modes`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** Exports metadata API functions. */
export const metadataApi = { getExpirationPeriods, getEditModes }
