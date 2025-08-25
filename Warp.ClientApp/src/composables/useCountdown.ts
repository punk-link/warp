import { ref, onMounted, onBeforeUnmount, watch, Ref } from 'vue'

/**
 * Simple countdown composable. Pass a ref to a target date (UTC) as Date|string.
 * Returns a reactive formatted remaining string (HH:MM:SS) and expired flag.
 */
export function useCountdown(target: Ref<Date | string | null | undefined>) {
  const remaining = ref<string>('')
  const expired = ref<boolean>(false)
  let timer: any = null

  function format(ms: number) {
    if (ms <= 0) return '00:00'
    const totalSeconds = Math.floor(ms / 1000)
    const hours = Math.floor(totalSeconds / 3600)
    const minutes = Math.floor((totalSeconds % 3600) / 60)
    const seconds = totalSeconds % 60
    const pad = (n: number) => n.toString().padStart(2, '0')
    if (hours > 0) return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`
    return `${pad(minutes)}:${pad(seconds)}`
  }

  function tick() {
    try {
      const t = target.value
      if (!t) {
        remaining.value = ''
        expired.value = false
        return
      }
      const date = (t instanceof Date) ? t : new Date(t)
      const diff = date.getTime() - Date.now()
      expired.value = diff <= 0
      remaining.value = format(diff)
    } catch {
      remaining.value = ''
    }
  }

  function start() {
    stop()
    tick()
    timer = setInterval(tick, 1000)
  }

  function stop() {
    if (timer) {
      clearInterval(timer)
      timer = null
    }
  }

  onMounted(start)
  onBeforeUnmount(stop)
  watch(target, () => start(), { immediate: true })

  return { remaining, expired }
}
