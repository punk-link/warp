const ENV_ORIGIN = (import.meta as any).env?.VITE_API_ORIGIN as string | undefined;

export const API_BASE = ENV_ORIGIN
	? `${ENV_ORIGIN.replace(/\/$/, '')}/api`
	: '/api';
