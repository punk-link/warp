export enum ExpirationPeriod {
	FiveMinutes = 'FiveMinutes',
	ThirtyMinutes = 'ThirtyMinutes',
	OneHour = 'OneHour',
	EightHours = 'EightHours',
	OneDay = 'OneDay'
}


export function parseExpirationPeriod(value: unknown): ExpirationPeriod {
	return (typeof value === 'string' && (Object.values(ExpirationPeriod) as string[]).includes(value))
		? (value as ExpirationPeriod)
		: ExpirationPeriod.FiveMinutes
}
