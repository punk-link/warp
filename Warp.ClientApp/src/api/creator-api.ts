import { fetchJson } from './fetch-service'
import { API_BASE } from './constants'
import { routeApiError } from './error-routing'


/** Gets the creator info, or sets it if not present. */
export async function getOrSetCreator() {
    try {
        return await fetchJson(`${API_BASE}/creators`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


/** API methods related to creators. */
export const creatorApi = { getOrSetCreator }
