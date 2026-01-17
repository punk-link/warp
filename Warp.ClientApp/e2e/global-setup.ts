import { chromium, FullConfig } from '@playwright/test'


/**
 * Global setup that ensures both frontend dev server and backend API are ready before tests run.
 * This prevents flaky tests in CI where the backend may take longer to become fully responsive.
 */
async function globalSetup(config: FullConfig): Promise<void> {
    const baseURL = config.projects[0]?.use?.baseURL
    const maxWaitTime = 120_000
    const startTime = Date.now()

    console.log(`[global-setup] Waiting for app at ${baseURL} to be fully ready...`)

    await waitForDevServer(baseURL, maxWaitTime, startTime)
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
    const context = await browser.newContext({
        ignoreHTTPSErrors: true
    })
    const page = await context.newPage()

    try {
        while (Date.now() - startTime < maxWaitTime) {
            try {
                await context.clearCookies()

                const response = await page.goto(baseURL, { waitUntil: 'domcontentloaded', timeout: 30000 })
                console.log(`[global-setup] Page loaded with status: ${response?.status()}`)

                await page.waitForFunction(
                    () => {
                        const app = document.querySelector('#app')
                        return app && app.children.length > 0
                    },
                    { timeout: 20000 }
                )

                const currentUrl = page.url()
                console.log(`[global-setup] Current URL after load: ${currentUrl}`)

                if (currentUrl.includes('/error')) {
                    const errorText = await page.locator('body').textContent()
                    console.log(`[global-setup] Redirected to error page. Content: ${errorText?.substring(0, 500)}`)
                    throw new Error(`App redirected to error page: ${currentUrl}`)
                }

                const modeToggle = page.getByTestId('mode-simple')
                await modeToggle.waitFor({ state: 'visible', timeout: 20000 })

                console.log(`[global-setup] Vue app initialized successfully`)
                return
            } catch (error) {
                const errorMsg = error instanceof Error ? error.message : String(error)
                console.log(`[global-setup] App not ready yet: ${errorMsg}`)

                try {
                    const bodyText = await page.locator('body').textContent()
                    console.log(`[global-setup] Page content: ${bodyText?.substring(0, 300)}...`)
                } catch {
                    // Ignore if we can't get content
                }

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
