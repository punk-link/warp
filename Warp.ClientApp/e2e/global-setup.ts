import { chromium, FullConfig } from '@playwright/test'


/**
 * Global setup that ensures both frontend dev server and backend API are ready before tests run.
 * This prevents flaky tests in CI where the backend may take longer to become fully responsive.
 */
async function globalSetup(config: FullConfig): Promise<void> {
    const baseURL = config.projects[0]?.use?.baseURL || 'http://localhost:5173'
    const maxWaitTime = 120_000
    const startTime = Date.now()

    console.log(`[global-setup] Waiting for app at ${baseURL} to be fully ready...`)

    // First, wait for the dev server to be serving the index page
    await waitForDevServer(baseURL, maxWaitTime, startTime)

    // Then, verify the Vue app can actually initialize by loading it in a real browser
    await verifyAppInitialization(baseURL, maxWaitTime, startTime)

    console.log(`[global-setup] App is ready after ${Date.now() - startTime}ms`)
}


async function waitForDevServer(baseURL: string, maxWaitTime: number, startTime: number): Promise<void> {
    while (Date.now() - startTime < maxWaitTime) {
        try {
            const response = await fetch(baseURL, { signal: AbortSignal.timeout(5000) })
            if (response.ok) {
                console.log(`[global-setup] Dev server is responding`)
                return
            }
        } catch {
            // Ignore and retry
        }
        await sleep(1000)
    }
    throw new Error(`Dev server at ${baseURL} did not become ready within ${maxWaitTime}ms`)
}


async function verifyAppInitialization(baseURL: string, maxWaitTime: number, startTime: number): Promise<void> {
    const browser = await chromium.launch()
    const context = await browser.newContext()
    const page = await context.newPage()

    try {
        while (Date.now() - startTime < maxWaitTime) {
            try {
                await page.goto(baseURL, { waitUntil: 'domcontentloaded', timeout: 30000 })

                // Wait for Vue app to mount
                await page.waitForFunction(
                    () => {
                        const app = document.querySelector('#app')
                        return app && app.children.length > 0
                    },
                    { timeout: 20000 }
                )

                // Verify the mode toggle is visible (indicates app is fully initialized)
                const modeToggle = page.getByTestId('mode-simple')
                await modeToggle.waitFor({ state: 'visible', timeout: 20000 })

                console.log(`[global-setup] Vue app initialized successfully`)
                return
            } catch (error) {
                console.log(`[global-setup] App not ready yet, retrying... (${error instanceof Error ? error.message : error})`)
                await sleep(2000)
            }
        }
        throw new Error(`Vue app did not initialize within ${maxWaitTime}ms`)
    } finally {
        await browser.close()
    }
}


function sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms))
}


export default globalSetup
