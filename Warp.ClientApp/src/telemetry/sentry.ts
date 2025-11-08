import * as Sentry from '@sentry/vue'
import '@sentry/tracing'
import type { SeverityLevel } from '@sentry/types'
import type { App } from 'vue'
import type { Router } from 'vue-router'
import { ErrorHandlingMode } from '../types/error-handling-mode'
import { NotifyLevel } from '../types/notify-level'
import type { ApiErrorTelemetry } from './types'


let sentryEnabled = false


export function setupSentry(app: App, router: Router): void {
    const dsn = window.appConfig?.sentryDsn?.trim()
    if (!dsn) {
        if (import.meta.env?.DEV)
            // eslint-disable-next-line no-console
            console.info('[sentry] DSN not provided; skipping client telemetry upload')

        return
    }

    if (sentryEnabled)
        return

    const environment = resolveEnvironment()
    const release = resolveRelease()
    const { tracesSampleRate, profilesSampleRate } = resolveSampleRates()

    Sentry.init({
        app,
        dsn,
        environment,
        release,
        integrations: [
            Sentry.browserTracingIntegration({
                router
            })
        ],
        tracesSampleRate,
        profilesSampleRate
    })

    Sentry.setTag('app.environment', environment)
    if (release)
        Sentry.setTag('app.version', release)

    sentryEnabled = true
}


export function reportApiErrorToSentry(payload: ApiErrorTelemetry): void {
    if (!sentryEnabled)
        return

    const severity = mapSeverity(payload)

    Sentry.addBreadcrumb({
        category: payload.category,
        level: severity,
        type: 'default',
        data: {
            method: payload.method,
            endpoint: payload.endpoint,
            status: payload.status,
            handledBy: resolveHandlingModeName(payload.handledBy),
            requestId: payload.requestId ?? undefined,
            traceId: payload.traceId ?? undefined,
            eventId: payload.eventId ?? undefined,
            serverSentryId: payload.sentryId ?? undefined,
            dedupeKey: payload.dedupeKey,
            notifyLevel: payload.notifyLevel
        }
    })

    if (!shouldCaptureEvent(payload))
        return

    Sentry.withScope(scope => {
        scope.setLevel(severity)

        if (payload.endpoint)
            scope.setTag('api.endpoint', payload.endpoint)

        if (payload.method)
            scope.setTag('api.method', payload.method)

        if (payload.status != null)
            scope.setTag('api.status', String(payload.status))

        scope.setContext('apiRequest', {
            method: payload.method ?? 'UNKNOWN',
            endpoint: payload.endpoint ?? 'UNKNOWN',
            status: payload.status ?? null,
            handledBy: resolveHandlingModeName(payload.handledBy),
            requestId: payload.requestId ?? null,
            traceId: payload.traceId ?? null,
            eventId: payload.eventId ?? null,
            serverSentryId: payload.sentryId ?? null,
            dedupeKey: payload.dedupeKey ?? null,
            notifyLevel: payload.notifyLevel ?? null
        })

        Sentry.captureMessage(buildApiErrorMessage(payload), severity)
    })
}


function resolveEnvironment(): string {
    const fromConfig = window.appConfig?.environment?.trim()
    if (fromConfig)
        return fromConfig

    const mode = (import.meta as any)?.env?.MODE

    return typeof mode === 'string' && mode ? mode : 'unknown'
}


function resolveRelease(): string | undefined {
    const version = (import.meta as any)?.env?.VITE_APP_VERSION
    if (typeof version !== 'string' || !version)
        return undefined

    return `warp-clientapp@${version}`
}


function resolveSampleRates(): { tracesSampleRate?: number; profilesSampleRate?: number } {
    const config = window.appConfig
    const tracesSampleRate = coerceSampleRate(config?.sentryTracesSampleRate)
    const profilesSampleRate = coerceSampleRate(config?.sentryProfilesSampleRate)

    if (tracesSampleRate == null && import.meta.env?.DEV)
        return { tracesSampleRate: 1, profilesSampleRate }

    return { tracesSampleRate, profilesSampleRate }
}


function coerceSampleRate(value: unknown): number | undefined {
    if (value == null)
        return undefined

    if (typeof value === 'number' && Number.isFinite(value))
        return clampSampleRate(value)

    if (typeof value === 'string') {
        const parsed = Number.parseFloat(value)
        if (Number.isFinite(parsed))
            return clampSampleRate(parsed)
    }

    return undefined
}


function clampSampleRate(value: number): number {
    if (value < 0)
        return 0

    if (value > 1)
        return 1

    return value
}


function mapSeverity(payload: ApiErrorTelemetry): SeverityLevel {
    const notifyLevel = payload.notifyLevel
    if (notifyLevel === NotifyLevel.Error)
        return 'error'

    if (notifyLevel === NotifyLevel.Warn)
        return 'warning'

    if (payload.status != null && payload.status >= 500)
        return 'error'

    if (payload.status != null && payload.status >= 400)
        return 'warning'

    return 'info'
}


function resolveHandlingModeName(mode: ErrorHandlingMode): string {
    switch (mode) {
        case ErrorHandlingMode.Global:
            return 'global'
        case ErrorHandlingMode.Component:
            return 'component'
        case ErrorHandlingMode.Silent:
            return 'silent'
        default:
            return 'unknown'
    }
}


function shouldCaptureEvent(payload: ApiErrorTelemetry): boolean {
    if (payload.notifyLevel === NotifyLevel.Info)
        return false

    if (payload.notifyLevel === NotifyLevel.Warn)
        return payload.status != null && payload.status >= 500

    if (payload.notifyLevel === NotifyLevel.Error)
        return true

    if (payload.status != null)
        return payload.status >= 500

    return false
}


function buildApiErrorMessage(payload: ApiErrorTelemetry): string {
    const method = payload.method ?? 'REQUEST'
    const endpoint = payload.endpoint ?? 'unknown endpoint'
    const status = payload.status != null ? payload.status : 'unknown status'

    return `API ${method.toUpperCase()} ${endpoint} failed with ${status}`
}
