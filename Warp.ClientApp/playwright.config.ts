// E2E tests with Playwright. To run: yarn e2e
import { defineConfig, devices } from '@playwright/test'


const devServerPort = Number(process.env.E2E_DEV_SERVER_PORT ?? '5173')
const resolvedBaseUrl = process.env.BASE_URL ?? `http://127.0.0.1:${devServerPort}/`
const devServerCommand = process.env.E2E_DEV_SERVER_COMMAND ?? `yarn dev --host 127.0.0.1 --port ${devServerPort}`


export default defineConfig({
    testDir: './e2e',
    timeout: 60_000,
    expect: { timeout: 5_000 },
    retries: process.env.CI ? 2 : 0,
    fullyParallel: true,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL: resolvedBaseUrl,
        trace: 'on-first-retry',
        ignoreHTTPSErrors: true
    },
    projects: [
        { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
        { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
        { name: 'webkit', use: { ...devices['Desktop Safari'] } }
    ],
    webServer: {
        command: devServerCommand,
        url: resolvedBaseUrl,
        reuseExistingServer: !process.env.CI,
        stdout: 'pipe',
        ignoreHTTPSErrors: true
    }
})
