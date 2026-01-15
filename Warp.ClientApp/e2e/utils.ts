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
    // Global setup ensures the app is ready, so we only need minimal retry logic here
    // for cases where the page context is reset between tests
    let lastError: Error | null = null

    for (let attempt = 1; attempt <= 3; attempt++) {
        if (page.isClosed()) {
            throw new Error('Page was closed before navigation could complete')
        }

        try {
            await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 30000 })

            // Wait for Vue app to mount
            await page.waitForFunction(
                () => {
                    const app = document.querySelector('#app')
                    return app && app.children.length > 0
                },
                { timeout: 15000 }
            )

            // Wait for the mode toggle to be visible
            await expect(page.getByTestId('mode-simple')).toBeVisible({ timeout: 30000 })

            return
        } catch (error) {
            lastError = error as Error

            const errorMessage = lastError.message.toLowerCase()
            if (errorMessage.includes('closed') || errorMessage.includes('target page')) {
                throw lastError
            }

            if (attempt < 3 && !page.isClosed()) {
                await page.waitForTimeout(2000)
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
