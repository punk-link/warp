import { createApp } from 'vue'
import { Fancybox } from '@fancyapps/ui'
import '@fancyapps/ui/dist/fancybox/fancybox.css'
import './styles/fancybox-overrides.css'
import './styles/tailwind.css'
import { createI18nInstance } from './i18n'
import { setupDefaultErrorBridge } from './api/errorBridge'
import { buildTraceHeaders } from './telemetry/traceContext'
import { setupSentry } from './telemetry/sentry'
import App from './App.vue'
import router from './router'


async function ensureCsrf() {
    try {
        const hasCookie = /(?:^|; )XSRF-TOKEN=/.test(document.cookie)
        if (!hasCookie) {
            const { headers } = buildTraceHeaders()
            // TODO: use a centralized fetch service
            await fetch('/api/security/csrf', { method: 'GET', credentials: 'include', cache: 'no-store', headers })
        }
    } catch {
        // Non-blocking: continue even if CSRF bootstrap fails; server will reject unsafe requests without it
    }
}


async function bootstrap(): Promise<void> {
    await ensureCsrf()

    const i18n = await createI18nInstance()
    const app = createApp(App)

    setupDefaultErrorBridge(router)

    app.use(router)
    app.use(i18n)

    setupSentry(app, router)

    app.mount('#app')
}


void bootstrap()


declare global {
    interface Window { Fancybox?: typeof Fancybox }
}


window.Fancybox = Fancybox
