const CONFIG_BASE = (typeof window !== 'undefined' ? (window as any)?.appConfig?.apiBaseUrl : undefined) as string | undefined


const ENV_ORIGIN = (import.meta as any).env?.VITE_API_ORIGIN as string | undefined


function trimTrailingSlash(value: string): string {
	return value.replace(/\/$/, '')
}


// Resolve API base in the following order:
// 1) Server-provided window.appConfig.apiBaseUrl (preferred in production)
// 2) VITE_API_ORIGIN from env (for local/dev overrides) + "/api"
// 3) Fallback to "/api" (same-origin API)
export const API_BASE = CONFIG_BASE
	? trimTrailingSlash(CONFIG_BASE)
	: ENV_ORIGIN
		? `${trimTrailingSlash(ENV_ORIGIN)}/api`
		: '/api'
