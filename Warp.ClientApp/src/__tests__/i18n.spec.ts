import { describe, it, expect } from 'vitest'
import { createI18nInstance, setLocale, currentLocale } from '../i18n'
import type { Composer } from 'vue-i18n'

describe('i18n bootstrap & switching', () => {
    it('initializes with base locale and resolves a key', async () => {
        const i18n = await createI18nInstance()
        const t = (i18n.global as unknown as Composer).t
        const title = t('app.title')
        expect(title).toBe('Warp')
    })

    it('switches locale at runtime', async () => {
        await createI18nInstance()
        await setLocale('es')
        expect(currentLocale()).toBe('es')
        // Ensure a translated key changes
        // (Key exists in both locales but with different value)
        const comp = (await createI18nInstance()).global as unknown as Composer
        const taglineEs = comp.t('app.tagline')
        expect(taglineEs.toLowerCase()).toContain('efímero')
    })
})
