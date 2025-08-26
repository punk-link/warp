import { ref } from 'vue'
import { fetchJson } from '../api/fetchHelper'
import { EditMode } from '../types/edit-modes'
import { ExpirationPeriod } from '../types/expiration-periods'


async function convertToArray<TOption>(data: any): Promise<TOption[]> {
    if (Array.isArray(data))
        return data.map((v: any) => ({ value: v, label: v })) as TOption[]

    const mapped = Object.entries(data as Record<string, string>)
        .map(([k, v]) => ({ key: Number(k), label: v }))
        .sort((a, b) => a.key - b.key)
        .map(e => e.key as TOption)

    return mapped as TOption[]
}


export function useMetadata() {
    const editModes = ref<EditMode[]>([])
    const expirationOptions = ref<ExpirationPeriod[]>([])


    async function loadEditModes(): Promise<EditMode[]> {
        if (editModes.value.length)
            return editModes.value

        try {
            const json = await fetchJson('/api/metadata/enums/edit-modes')
            editModes.value = await convertToArray<EditMode>(json)
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
            expirationOptions.value = await convertToArray<ExpirationPeriod>(json)
        } catch {
            console.error('Failed to load expiration options')
        }

        return expirationOptions.value
    }

    return { editModes, expirationOptions, loadEditModes, loadExpirationOptions }
}
