import { createI18n } from 'vue-i18n';
import type { I18n, Composer } from 'vue-i18n';
import { detectInitialLocale, LOCALE_STORAGE_KEY, supportedLocales, type SupportedLocale } from './detect';

// Type schema derived from base (en) without eagerly importing runtime data
// (Only used for type inference; messages loaded lazily.)
export type MessageSchema = typeof import('./locales/en').default;

let i18nSingleton: I18n | null = null;
let creatingPromise: Promise<I18n> | null = null;
const loaded = new Set<string>();


async function loadMessages(locale: SupportedLocale): Promise<Record<string, unknown>> {
    switch (locale) {
        case 'en': {
            const mod = await import('./locales/en');
            return mod.default as Record<string, unknown>;
        }
        case 'es': {
            const mod = await import('./locales/es');
            return mod.default as Record<string, unknown>;
        }
        default:
            return {};
    }
}


async function ensureMessages(locale: SupportedLocale, i18n: I18n) {
    if (loaded.has(locale))
        return;

    const messages = await loadMessages(locale);
    (i18n.global as unknown as Composer).setLocaleMessage(locale, messages);
    loaded.add(locale);
}


export async function createI18nInstance(): Promise<I18n> {
    if (i18nSingleton)
        return i18nSingleton;

    if (creatingPromise)
        return creatingPromise;

    creatingPromise = (async (): Promise<I18n> => {
        const initial = detectInitialLocale();
        const messages = await loadMessages(initial);

        // Use non‑strict generic to avoid heavy typing friction; runtime enforcement via our loaders
        const i18n = createI18n({
            legacy: false,
            locale: initial,
            fallbackLocale: 'en',
            messages: { [initial]: messages } as any
        }) as unknown as I18n; // Cast acceptable: internal usage constrained

        loaded.add(initial);
        document.documentElement.lang = initial;
        i18nSingleton = i18n;

        return i18n;
    })();

    return creatingPromise as Promise<I18n>;
}


export function currentLocale(): SupportedLocale {
    if (!i18nSingleton)
        return detectInitialLocale();

    const comp = i18nSingleton.global as unknown as Composer;
    return comp.locale.value as SupportedLocale;
}


export async function setLocale(next: SupportedLocale): Promise<void> {
    if (!supportedLocales.includes(next))
        return;

    const i18n = await createI18nInstance();
    const comp = i18n.global as unknown as Composer;
    const current = comp.locale.value;
    if (current === next)
        return;

    await ensureMessages(next, i18n);
    comp.locale.value = next;
    try {
        if (typeof window !== 'undefined')
            window.localStorage.setItem(LOCALE_STORAGE_KEY, next);
    } catch {
        // Non-blocking persistence failure
    }

    document.documentElement.lang = next;
}


export { supportedLocales };


// Lightweight helper to translate outside components with a fallback
export function tOr(key: string, fallback: string, params?: Record<string, unknown>): string {
    if (!i18nSingleton)
        return fallback

    const comp = i18nSingleton.global as unknown as Composer
    if (typeof (comp as any).te === 'function' && (comp as any).te(key))
        return comp.t(key as any, params as any) as unknown as string

    try {
        const translated = comp.t(key as any, params as any) as unknown as string
        return translated || fallback
    } catch {
        return fallback
    }
}
