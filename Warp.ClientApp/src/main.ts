import { createApp } from 'vue'
import { Fancybox } from '@fancyapps/ui'
import '@fancyapps/ui/dist/fancybox/fancybox.css'
import App from './App.vue'
import router from './router'
import './styles/tailwind.css'

async function ensureCsrf() {
    try {
        const hasCookie = /(?:^|; )XSRF-TOKEN=/.test(document.cookie)
        if (!hasCookie) {
            await fetch('/api/security/csrf', { method: 'GET', credentials: 'include', cache: 'no-store' })
        }
    } catch {
        // Non-blocking: continue even if CSRF bootstrap fails; server will reject unsafe requests without it
    }
}

await ensureCsrf()

createApp(App)
    .use(router)
    .mount('#app')

    
declare global {
    interface Window { Fancybox?: typeof Fancybox }
}


window.Fancybox = Fancybox
