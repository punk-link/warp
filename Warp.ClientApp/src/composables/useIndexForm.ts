import { ref } from 'vue'
import { fetchJson } from '../api/fetchHelper'
import type { ApiError } from '../types/api-error'

export function useIndexForm() {
    const pending = ref(false)
    const error = ref<string | null>(null)

    async function createEntry(payload: { text: string; files?: File[]; expiration?: string | null }) {
        pending.value = true
        error.value = null

        try {
            const form = new FormData()
            form.append('text', payload.text ?? '')
            if (payload.expiration)
                form.append('expiration', payload.expiration)

            if (payload.files && payload.files.length) {
                for (const f of payload.files)
                    form.append('files', f)
            }

            return await fetchJson('/api/entries', {
                method: 'POST',
                body: form
            })
        } catch (err) {
            const apiError = err as ApiError
            error.value = apiError.problem?.detail || apiError.message || 'Request failed'
            throw apiError
        } finally {
            pending.value = false
        }
    }

    async function updateEntry(id: string, payload: { text?: string; files?: File[]; expiration?: string | null }) {
        pending.value = true
        error.value = null

        try {
            const form = new FormData()
            if (payload.text !== undefined)
                form.append('text', payload.text)

            if (payload.expiration !== undefined && payload.expiration !== null)
                form.append('expiration', payload.expiration)

            if (payload.files && payload.files.length) {
                for (const f of payload.files)
                    form.append('files', f)
            }

            return await fetchJson(`/api/entries/${encodeURIComponent(id)}`, {
                method: 'PUT',
                body: form
            })
        } catch (err) {
            const apiError = err as ApiError
            error.value = apiError.problem?.detail || apiError.message || 'Request failed'
            throw apiError
        } finally {
            pending.value = false
        }

    }

    return { createEntry, updateEntry, pending, error }
}
