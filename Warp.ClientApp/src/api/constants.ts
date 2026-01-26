const CONFIG_BASE = (typeof window !== 'undefined' ? (window as any)?.appConfig?.apiBaseUrl : undefined) as string | undefined


const ENV_ORIGIN = (import.meta as any).env?.VITE_API_ORIGIN as string | undefined


function trimTrailingSlash(value: string): string {
	return value.replace(/\/$/, '')
}


/** The base URL for API requests. */
export const API_BASE = CONFIG_BASE
	? trimTrailingSlash(CONFIG_BASE)
	: ENV_ORIGIN
		? `${trimTrailingSlash(ENV_ORIGIN)}/api`
		: '/api'
