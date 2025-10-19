import { createApp } from 'vue'
import { Fancybox } from '@fancyapps/ui'
import '@fancyapps/ui/dist/fancybox/fancybox.css'
import './styles/fancybox-overrides.css'
import App from './App.vue'
import router from './router'
import './styles/tailwind.css'
import { createI18nInstance } from './i18n'


async function ensureCsrf() {
    try {
        const hasCookie = /(?:^|; )XSRF-TOKEN=/.test(document.cookie)
        if (!hasCookie) 
            await fetch('/api/security/csrf', { method: 'GET', credentials: 'include', cache: 'no-store' })
    } catch {
        // Non-blocking: continue even if CSRF bootstrap fails; server will reject unsafe requests without it
    }
}


;(async () => {
    await ensureCsrf()

    const i18n = await createI18nInstance()

    createApp(App)
        .use(router)
        .use(i18n)
        .mount('#app')
})()

    
declare global {
    interface Window { Fancybox?: typeof Fancybox }
}


window.Fancybox = Fancybox
