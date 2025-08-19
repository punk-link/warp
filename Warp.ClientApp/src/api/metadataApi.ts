import { fetchJson } from './fetchHelper'

export async function getExpirationPeriods() {
  return fetchJson<Record<string, string>>('/api/metadata/enums/expiration-periods')
}

export async function getEditModes() {
  return fetchJson<Record<string, string>>('/api/metadata/enums/edit-modes')
}

export const metadataApi = { getExpirationPeriods, getEditModes }
