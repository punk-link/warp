import { fetchJson } from './fetchHelper'
import { API_BASE } from './constants'


export async function getOrSetCreator() {
    return fetchJson(`${API_BASE}/creators`)
}


export const creatorApi = { getOrSetCreator }
