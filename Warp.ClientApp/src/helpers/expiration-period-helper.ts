import { ExpirationPeriod } from "../types/entries/enums/expiration-periods";


/** Parses a value into an ExpirationPeriod, defaulting to ExpirationPeriod.FiveMinutes if invalid. */
export function parseExpirationPeriod(value: unknown): ExpirationPeriod {
	return (typeof value === 'string' && (Object.values(ExpirationPeriod) as string[]).includes(value))
		? (value as ExpirationPeriod)
		: ExpirationPeriod.FiveMinutes
}