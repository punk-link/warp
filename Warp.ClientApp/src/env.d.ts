/// <reference types="vite/client" />

declare module '*.vue' {
    import type { DefineComponent } from 'vue'
    const component: DefineComponent<{}, {}, any>
    export default component
}


declare global {
    interface Window {
        appConfig?: {
            apiBaseUrl?: string
            environment?: string
            maxContentDeltaSize?: number
            maxHtmlContentSize?: number
            maxPlainTextContentSize?: number
            sentryDsn?: string
            sentryProfilesSampleRate?: number | null
            sentryTracesSampleRate?: number | null
        }
    }
}


export { }
