export enum ExpirationPeriod {
	FiveMinutes = 0,
	ThirtyMinutes = 1,
	OneHour = 2,
	EightHours = 3,
	OneDay = 4
}

export interface ExpirationOption { value: ExpirationPeriod; label: string }
