/** Detects the initial locale for the application. */
export function detectInitialLocale(): SupportedLocale {
    try {
        if (typeof window === 'undefined')
            return 'en';

        const persisted = getPersistedLocale();
        if (persisted)
            return persisted;

        const browserLanguages = getBrowserLanguages();
        
        const exactMatch = findExactMatch(browserLanguages);
        if (exactMatch)
            return exactMatch;
        
        const primaryMatch = findPrimaryMatch(browserLanguages);
        if (primaryMatch)
            return primaryMatch;
    } catch {
        // Non-fatal: fall through to default
    }

    return 'en';
}


/** Key used to persist the selected locale in local storage. */
export const LOCALE_STORAGE_KEY = 'warp.locale';

/** Supported application locales. */
export const supportedLocales = ['en', 'es'] as const;

/** Supported application locale type. */
export type SupportedLocale = typeof supportedLocales[number];


function normalise(raw: string): string {
    return raw.trim().toLowerCase().replace('_', '-');
}


function getPersistedLocale(): SupportedLocale | null {
    try {
        const persisted = window.localStorage.getItem(LOCALE_STORAGE_KEY);
        if (!persisted)
            return null;

        const normalized = normalise(persisted);
        if (supportedLocales.includes(normalized as SupportedLocale))
            return normalized as SupportedLocale;
    } catch {
        // Storage access may fail
    }
    
    return null;
}


function getBrowserLanguages(): string[] {
    const { navigator } = window;
    
    if (navigator.languages && navigator.languages.length)
        return Array.from(navigator.languages);
    
    if (navigator.language)
        return [navigator.language];
    
    return [];
}


function findExactMatch(candidates: string[]): SupportedLocale | null {
    for (const candidate of candidates) {
        const normalised = normalise(candidate);
        if (supportedLocales.includes(normalised as SupportedLocale))
            return normalised as SupportedLocale;
    }
    
    return null;
}


function findPrimaryMatch(candidates: string[]): SupportedLocale | null {
    for (const candidate of candidates) {
        const primary = normalise(candidate).split('-')[0];
        if (supportedLocales.includes(primary as SupportedLocale))
            return primary as SupportedLocale;
    }
    
    return null;
}
