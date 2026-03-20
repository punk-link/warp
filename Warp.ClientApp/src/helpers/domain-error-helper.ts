import { getCurrentLocale } from '../i18n'
import type { ProblemDetails } from '../types/apis/problem-details/problem-details'


const localeModules: Partial<Record<string, Record<number, string> | null>> = {}


async function loadLocale(locale: string): Promise<Record<number, string> | undefined> {
    if (locale in localeModules)
        return localeModules[locale] ?? undefined

    try {
        const mod = await import(`../i18n/generated/domain-errors.${locale}.ts`)
        localeModules[locale] = mod.default as Record<number, string>
    } catch {
        localeModules[locale] = null
    }

    return localeModules[locale] ?? undefined
}


function interpolate(template: string, params: string[]): string {
    return template.replace(/\{(\d+)\}/g, (_, index) => params[Number(index)] ?? '')
}


export async function getLocalizedDomainError(problem: ProblemDetails): Promise<string | undefined> {
    if (problem.eventId == null)
        return undefined

    const locale = getCurrentLocale()
    const messages = await loadLocale(locale) ?? (locale !== 'en' ? await loadLocale('en') : undefined)

    if (messages == null)
        return undefined

    const template = messages[problem.eventId]
    if (template == null)
        return undefined

    if (problem.errorParams && problem.errorParams.length > 0)
        return interpolate(template, problem.errorParams)

    return template
}
