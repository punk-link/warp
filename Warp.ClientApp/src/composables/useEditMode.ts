import { ref, watch } from 'vue'

const STORAGE_KEY = 'warp.editMode'

export function useEditMode() {
  const stored = (typeof localStorage !== 'undefined' && localStorage.getItem(STORAGE_KEY)) as ('simple' | 'advanced') | null
  const mode = ref<'simple' | 'advanced'>(stored ?? 'simple')

  watch(mode, (v) => {
    try {
      localStorage.setItem(STORAGE_KEY, v)
    } catch {
      // ignore storage errors in private mode
    }
  })

  return { mode }
}
