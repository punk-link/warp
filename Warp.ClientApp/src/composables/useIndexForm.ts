import { ref } from 'vue'

export function useIndexForm() {
  const pending = ref(false)
  const error = ref<string | null>(null)

  async function createEntry(payload: { text: string; files?: File[]; expiration?: string | null }) {
    pending.value = true
    error.value = null

    try {
      const form = new FormData()
      form.append('text', payload.text ?? '')
      if (payload.expiration) form.append('expiration', payload.expiration)

      if (payload.files && payload.files.length) {
        for (const f of payload.files) {
          form.append('files', f)
        }
      }

      const res = await fetch('/api/entries', {
        method: 'POST',
        body: form,
        credentials: 'include'
      })

      if (!res.ok) {
        const body = await res.json().catch(() => null)
        error.value = body?.message ?? `Request failed: ${res.status}`
        throw new Error(error.value)
      }

      return await res.json()
    } finally {
      pending.value = false
    }
  }

  async function updateEntry(id: string, payload: { text?: string; files?: File[]; expiration?: string | null }) {
    pending.value = true
    error.value = null

    try {
      const form = new FormData()
      if (payload.text !== undefined) form.append('text', payload.text)
      if (payload.expiration !== undefined && payload.expiration !== null) form.append('expiration', payload.expiration)
      if (payload.files && payload.files.length) {
        for (const f of payload.files) form.append('files', f)
      }

      const res = await fetch(`/api/entries/${encodeURIComponent(id)}`, {
        method: 'PUT',
        body: form,
        credentials: 'include'
      })

      if (!res.ok) {
        const body = await res.json().catch(() => null)
        error.value = body?.message ?? `Request failed: ${res.status}`
        throw new Error(error.value)
      }

      return await res.json()
    } finally {
      pending.value = false
    }
  }

  return { createEntry, updateEntry, pending, error }
}
