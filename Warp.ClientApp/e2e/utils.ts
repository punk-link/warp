import { expect } from '@playwright/test'
import type { Locator, Page } from '@playwright/test'


export async function clickElement(locator: Locator, timeout = 15000): Promise<void> {
    await locator.waitFor({ state: 'visible', timeout })
    await locator.waitFor({ state: 'attached', timeout })

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


export async function expectOnHome(page: Page): Promise<void> {
    await page.waitForURL(/\/?(\?|$)/i)
    await expect(page.getByRole('main').getByRole('heading', { level: 1 }).first()).toBeVisible()
}


export async function expectOnPreview(page: Page): Promise<void> {
    await page.waitForURL(/\/preview(\?|$)/i)
    await expect(page.locator('article').first()).toBeVisible()
}


export async function expectOnEntry(page: Page): Promise<void> {
    await page.waitForURL(/\/entry\//i)
    await expect(page.locator('article').first()).toBeVisible()
}


export async function expectOnDeleted(page: Page): Promise<void> {
    await page.waitForURL(/\/deleted(\?|$)/i)
    await expect(page.getByText(/the entry was deleted/i)).toBeVisible()
}
