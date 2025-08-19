import { ref } from 'vue'

export interface ExpirationOption { value: string; label: string }

export function useMetadata() {
  const expirationOptions = ref<ExpirationOption[]>([])

  async function loadExpirationOptions() {
    if (expirationOptions.value.length) 
        return expirationOptions.value

    try {
      const res = await fetch('/api/metadata/expirationPeriods', { credentials: 'include' })
      if (!res.ok) return expirationOptions.value
      const data = await res.json()
      expirationOptions.value = Array.isArray(data) ? data : []
    } catch {
      // noop - keep empty
    }

    return expirationOptions.value
  }

  return { expirationOptions, loadExpirationOptions }
}
