import { describe, it, expect, beforeAll } from 'vitest'
import { supportedLocales } from '../i18n/detect'


type NestedObject = { [key: string]: string | NestedObject }

const nonBaseLocales = supportedLocales.filter(l => l !== 'en')


function collectKeys(obj: NestedObject, prefix = ''): Set<string> {
    const keys = new Set<string>()

    for (const key of Object.keys(obj)) {
        const path = prefix ? `${prefix}.${key}` : key
        const value = obj[key]

        if (value !== null && typeof value === 'object')
            for (const nested of collectKeys(value as NestedObject, path))
                keys.add(nested)
        else
            keys.add(path)
    }

    return keys
}


describe('UI locale completeness', () => {
    let baseKeys: Set<string>
    const localeMessages: Record<string, NestedObject> = {}

    beforeAll(async () => {
        const base = await import('../i18n/locales/en')
        baseKeys = collectKeys(base.default as unknown as NestedObject)

        await Promise.all(
            nonBaseLocales.map(async locale => {
                const mod = await import(`../i18n/locales/${locale}.ts`)
                localeMessages[locale] = mod.default as unknown as NestedObject
            })
        )
    })

    for (const locale of nonBaseLocales) {
        it(`'${locale}' has all keys from the base 'en' locale`, () => {
            const localeKeys = collectKeys(localeMessages[locale])

            const missing = [...baseKeys].filter(key => !localeKeys.has(key))
            const extra = [...localeKeys].filter(key => !baseKeys.has(key))

            expect(missing, `'${locale}': missing keys:\n  ${missing.join('\n  ')}`).toHaveLength(0)
            expect(extra, `'${locale}': extra keys not in base 'en':\n  ${extra.join('\n  ')}`).toHaveLength(0)
        })
    }
})


describe('Domain error locale completeness', () => {
    let baseIds: Set<number>
    const localeErrors: Record<string, Record<number, string>> = {}

    beforeAll(async () => {
        const base = await import('../i18n/generated/domain-errors.en')
        baseIds = new Set(Object.keys(base.default).map(Number))

        await Promise.all(
            nonBaseLocales.map(async locale => {
                const mod = await import(`../i18n/generated/domain-errors.${locale}.ts`)
                localeErrors[locale] = mod.default as Record<number, string>
            })
        )
    })

    for (const locale of nonBaseLocales) {
        it(`'${locale}' has all event IDs from the base 'en' domain errors`, () => {
            const localeIds = new Set(Object.keys(localeErrors[locale]).map(Number))

            const missing = [...baseIds].filter(id => !localeIds.has(id))
            const extra = [...localeIds].filter(id => !baseIds.has(id))

            expect(missing, `'${locale}': missing event IDs:\n  ${missing.join('\n  ')}`).toHaveLength(0)
            expect(extra, `'${locale}': extra event IDs not in base 'en':\n  ${extra.join('\n  ')}`).toHaveLength(0)
        })
    }
})
