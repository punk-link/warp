import { ref } from 'vue'
import { fetchJson } from '../api/fetchHelper'
import { EditMode, parseEditMode } from '../types/edit-modes'
import { ExpirationPeriod, parseExpirationPeriod } from '../types/expiration-periods'


/** Composable for loading metadata such as enum values from the API. */
export function useMetadata() {
    return { editModes, expirationOptions, loadEditModes, loadExpirationOptions }
}


async function convertEnumDictionaryToValues<TOption>(data: any): Promise<TOption[]> {
    if (!data)
        return []

    if (Array.isArray(data))
        return data as TOption[]

    const mapped = Object.entries(data as Record<string, string>)
        .map(([, v]) => v) 
        .filter(v => typeof v === 'string')
        .map(v => parseEditMode(v) as unknown as TOption)

    return mapped as TOption[]
}


async function loadEditModes(): Promise<EditMode[]> {
    if (editModes.value.length)
        return editModes.value

    try {
        const json = await fetchJson('/api/metadata/enums/edit-modes')
        editModes.value = await convertEnumDictionaryToValues<EditMode>(json)
    } catch {
        console.error('Failed to load edit modes')
    }

    return editModes.value
}


async function loadExpirationOptions(): Promise<ExpirationPeriod[]> {
    if (expirationOptions.value.length)
        return expirationOptions.value

    try {
        const json = await fetchJson('/api/metadata/enums/expiration-periods')
        if (Array.isArray(json)) {
            expirationOptions.value = json.map(v => parseExpirationPeriod(v)) as ExpirationPeriod[]
        } else {
            const mapped = Object.entries(json as Record<string, string>)
                .map(([, v]) => parseExpirationPeriod(v))
            expirationOptions.value = mapped
        }
    } catch {
        console.error('Failed to load expiration options')
    }

    return expirationOptions.value
}


const editModes = ref<EditMode[]>([])
const expirationOptions = ref<ExpirationPeriod[]>([])
