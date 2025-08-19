import { fetchJson } from './fetchHelper'

export async function getOrSetCreator() {
  return fetchJson('/api/creators')
}

export const creatorApi = { getOrSetCreator }
