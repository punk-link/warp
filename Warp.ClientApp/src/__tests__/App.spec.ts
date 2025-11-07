import { render, screen } from '@testing-library/vue'
import { describe, it, expect } from 'vitest'
import { createI18nInstance } from '../i18n'
import App from '../App.vue'
import { createMemoryHistory, createRouter } from 'vue-router'


function createTestRouter() {
    return createRouter({
        history: createMemoryHistory(),
        routes: [
            {
                path: '/',
                meta: { pageBg: '' },
                component: {
                    template: '<h1>{{ $t("components.logo.title") }}</h1>'
                }
            },
            {
                path: '/privacy',
                component: { template: '<div>privacy</div>' }
            },
            {
                path: '/data-request',
                component: { template: '<div>data request</div>' }
            }
        ]
    })
}

// Minimal smoke test to ensure the app renders base layout
describe('App.vue', () => {
    it('renders heading', async () => {
        const router = createTestRouter()
        
        await router.push('/')
        await router.isReady()

        const i18n = await createI18nInstance()

        render(App, {
            global: {
                plugins: [router, i18n]
            }
        })

        const headings = await screen.findAllByRole('heading', { level: 1 })
        const normalized = headings.map(h => h.textContent?.toLowerCase() ?? '')

        expect(normalized.some(text => text.includes('warp'))).toBe(true)
    })
})
