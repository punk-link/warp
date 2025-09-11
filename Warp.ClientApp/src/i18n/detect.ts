// Locale detection & persistence utilities

export const LOCALE_STORAGE_KEY = 'warp.locale';

export const supportedLocales = ['en', 'es'] as const;
export type SupportedLocale = typeof supportedLocales[number];


function normalise(raw: string): string {
    return raw.trim().toLowerCase().replace('_', '-');
}


export function detectInitialLocale(): SupportedLocale {
    try {
        if (typeof window !== 'undefined') {
            const persisted = window.localStorage.getItem(LOCALE_STORAGE_KEY);
            if (persisted) {
                const normalized = normalise(persisted);
                if (supportedLocales.includes(normalized as SupportedLocale))
                    return normalized as SupportedLocale;
            }

            const navLangs: string[] = [];
            const { navigator } = window;
            
            if (navigator.languages && navigator.languages.length)
                navLangs.push(...navigator.languages);
            else if (navigator.language)
                navLangs.push(navigator.language);
            
            for (const candidate of navLangs) {
                const normalised = normalise(candidate);
                if (supportedLocales.includes(normalised as SupportedLocale))
                    return normalised as SupportedLocale;
            }
            
            // Try primary subtag match (e.g. 'en-GB' -> 'en')
            for (const candidate of navLangs) {
                const primary = normalise(candidate).split('-')[0];
                if (supportedLocales.includes(primary as SupportedLocale))
                    return primary as SupportedLocale;
            }
        }
    } catch {
        // Non-fatal: fall through to default
    }

    return 'en';
}
