import { expect } from '@playwright/test'
import type { Locator, Page } from '@playwright/test'


async function waitForSpaUrl(page: Page, matcher: RegExp | string, timeout = 30000): Promise<void> {
    await page.waitForURL(matcher, { waitUntil: 'commit', timeout })
}


export async function clickElement(locator: Locator, timeout = 15000): Promise<void> {
    await locator.waitFor({ state: 'visible', timeout })
    await locator.waitFor({ state: 'attached', timeout })
    await expect(locator).toBeEnabled({ timeout })

    await locator.click()
}


export async function setupClipboardSpy(page: Page): Promise<void> {
    await page.context().addInitScript(() => {
        window.__copiedLink = ''

        if (navigator.clipboard) {
            const clipboard = navigator.clipboard
            clipboard.writeText = async (text: string) => {
                window.__copiedLink = text
                return Promise.resolve()
            }
        }
    })
}


export async function getCopiedLink(page: Page): Promise<string> {
    return page.evaluate(() => window.__copiedLink || '')
}


export async function gotoHome(page: Page): Promise<void> {
    // Retry navigation up to 5 times for cold start scenarios (CI needs more time)
    let lastError: Error | null = null

    for (let attempt = 1; attempt <= 5; attempt++) {
        try {
            await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30000 })

            // Wait for Vue app to mount - check for #app having content
            await page.waitForFunction(
                () => {
                    const app = document.querySelector('#app')
                    return app && app.children.length > 0
                },
                { timeout: 20000 }
            )

            // Wait for Vue router to be ready
            await page.waitForFunction(
                () => (window as any).__vue_app_ready !== false,
                { timeout: 5000 }
            ).catch(() => {}) // Ignore if __vue_app_ready isn't set

            // Now wait for the mode toggle
            await expect(page.getByTestId('mode-simple')).toBeVisible({ timeout: 20000 })

            return // Success
        } catch (error) {
            lastError = error as Error

            // Don't retry if the page/browser was closed (test timeout or abort)
            const errorMessage = lastError.message.toLowerCase()
            if (errorMessage.includes('closed') || errorMessage.includes('target page')) {
                throw lastError
            }

            if (attempt < 5 && !page.isClosed()) {
                // Exponential backoff: 1s, 2s, 3s, 4s
                await page.waitForTimeout(attempt * 1000)
            }
        }
    }

    throw lastError
}


export async function expectOnHome(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/?(\?|$)/i)
    await expect(page.getByRole('main').getByRole('heading', { level: 1 }).first()).toBeVisible()
}


export async function expectOnPreview(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/preview(\?|$)/i)
    await expect(page.locator('article').first()).toBeVisible()
}


export async function expectOnEntry(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/entry\//i)
    await expect(page.locator('article').first()).toBeVisible()
}


export async function expectOnDeleted(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/deleted(\?|$)/i)
    await expect(page.getByText(/the entry was deleted/i)).toBeVisible()
}
