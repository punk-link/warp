import { ref } from 'vue'

export function useIndexUI(initial: 'simple' | 'advanced' = 'simple') {
  const mode = ref<'simple' | 'advanced'>(initial)

  function setMode(m: 'simple' | 'advanced') {
    mode.value = m
  }

  return { mode, setMode }
}
