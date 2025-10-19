import { fetchJson } from './fetchHelper'
import { API_BASE } from './constants'
import { routeApiError } from './errorRouting'


export async function getOrSetCreator() {
    try {
        return await fetchJson(`${API_BASE}/creators`)
    } catch (e) {
        routeApiError(e)
        throw e
    }
}


export const creatorApi = { getOrSetCreator }
