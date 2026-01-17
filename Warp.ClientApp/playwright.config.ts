// E2E tests with Playwright. To run: yarn e2e
import { defineConfig, devices } from '@playwright/test'


const devServerPort = Number(process.env.E2E_DEV_SERVER_PORT ?? '5173')
const devServerHost = process.env.E2E_DEV_SERVER_HOST ?? 'localhost'
const resolvedBaseUrl = process.env.BASE_URL ?? `http://${devServerHost}:${devServerPort}/`
const devServerCommand = process.env.E2E_DEV_SERVER_COMMAND ?? `yarn dev --host ${devServerHost} --port ${devServerPort}`


export default defineConfig({
    testDir: './e2e',
    globalSetup: './e2e/global-setup.ts',
    timeout: process.env.CI ? 120_000 : 60_000,
    expect: { timeout: 5_000 },
    retries: process.env.CI ? 2 : 0,
    fullyParallel: true,
    workers: process.env.CI ? 2 : undefined,
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
        ignoreHTTPSErrors: true,
        timeout: 120_000,
        env: {
            ...process.env,
            ENV_ORIGIN: process.env.ENV_ORIGIN || 'https://localhost:8001'
        }
    }
})
