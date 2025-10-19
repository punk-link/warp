export function useAnalytics() {
  function track(eventName: string, payload?: Record<string, any>) {
    try {
      const globalAny = (window as any)
      if (globalAny && typeof globalAny.analytics === 'object' && typeof globalAny.analytics.track === 'function') {
        globalAny.analytics.track(eventName, payload)
      } else {
        console.debug('[analytics]', eventName, payload)
      }
    } catch (e) {
      // swallow analytics errors
    }
  }

  return { track }
}
