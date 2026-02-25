import { expect } from '@playwright/test'
import type { Locator, Page } from '@playwright/test'
import path from 'path'
import { fileURLToPath } from 'url'
import {
    getAdvancedModeToggle,
    getPreviewButton,
    getSimpleModeToggle,
    getTextArea
} from './locators'


async function waitForSpaUrl(page: Page, matcher: RegExp | string, timeout = 30000): Promise<void> {
    await page.waitForURL(matcher, { waitUntil: 'commit', timeout })
}


export async function clickElement(locator: Locator, timeout = 15000): Promise<void> {
    await locator.waitFor({ state: 'visible', timeout })
    await locator.waitFor({ state: 'attached', timeout })
    await expect(locator).toBeEnabled({ timeout })

    await locator.click()
}


export async function saveAndAwaitPostSaveButtons(page: Page, timeout = 30000): Promise<void> {
    const saveButton = page.getByRole('button', { name: /^Save$/i })
    await clickElement(saveButton)

    const copyLinkButton = page.getByRole('button', { name: /copy link/i })
    await expect(copyLinkButton).toBeVisible({ timeout })
}


type PasteFilePayload = {
    bytes: number[]
    mimeType: string
    name: string
}


export async function dispatchPasteWithFiles(page: Page, files: PasteFilePayload[]): Promise<void> {
    await page.evaluate((payloads) => {
        const files = payloads.map((payload) => {
            const data = new Uint8Array(payload.bytes)
            return new File([data], payload.name, { type: payload.mimeType })
        })

        const event = new CustomEvent('paste', {
            bubbles: true,
            detail: { files }
        })

        window.dispatchEvent(event)
    }, files)
}


export async function expectOnDeleted(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/deleted(\?|$)/i)
    await expect(page.getByText(/the entry was deleted/i)).toBeVisible()
}


export async function expectOnEntry(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/entry\//i)
    await expect(page.locator('article').first()).toBeVisible()
}


export async function expectOnHome(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/?(\?|$)/i)
    await expect(page.getByRole('main').getByRole('heading', { level: 1 }).first()).toBeVisible()
}


export async function expectOnPreview(page: Page): Promise<void> {
    await waitForSpaUrl(page, /\/preview(\?|$)/i)
    await expect(page.locator('article').first()).toBeVisible()
}


export async function fillTextAndVerify(page: Page, text: string): Promise<void> {
    const textArea = getTextArea(page)
    await expect(textArea).toBeVisible({ timeout: 10000 })

    const previewButton = getPreviewButton(page)
    await expect(previewButton).toHaveAttribute('aria-busy', 'false', { timeout: 15000 })

    await textArea.click()
    await textArea.fill('')
    await textArea.pressSequentially(text, { delay: 0 })

    await expect(textArea).toHaveValue(text, { timeout: 10000 })
    await expect(previewButton).toBeEnabled({ timeout: 15000 })
}


export async function fillRichTextAndVerify(page: Page, text: string): Promise<void> {
    const editor = page.locator('.tiptap-editor')
    await expect(editor).toBeVisible({ timeout: 10000 })

    const previewButton = getPreviewButton(page)
    await expect(previewButton).toHaveAttribute('aria-busy', 'false', { timeout: 15000 })

    await editor.click()
    await editor.fill(text)

    await expect(editor).toContainText(text, { timeout: 10000 })
    await expect(previewButton).toBeEnabled({ timeout: 15000 })
}


export async function clickRichTextToolbarButton(page: Page, title: string): Promise<void> {
    const button = page.locator(`.editor-toolbar button[title="${title}"]`)
    await expect(button).toBeVisible({ timeout: 5000 })
    await button.click()
}


export async function getCopiedLink(page: Page): Promise<string> {
    return page.evaluate(() => window.__copiedLink || '')
}


export async function getViewCount(page: Page): Promise<number> {
    const viewCountSpan = page
        .locator('.icofont-eye')
        .locator('xpath=following-sibling::span[1]')
        .first()

    await expect(viewCountSpan).toBeVisible()

    const text = (await viewCountSpan.innerText()).trim()
    const value = Number.parseInt(text, 10)

    if (Number.isNaN(value))
        throw new Error(`Could not parse view count from: ${text}`)

    return value
}


export async function gotoHome(page: Page): Promise<void> {
    if (page.isClosed()) {
        throw new Error('Page was closed before navigation could complete')
    }

    const _ = await page.goto('/', { waitUntil: 'domcontentloaded', timeout: 60000 })
    // console.log(`[gotoHome] Navigation response status: ${response?.status()}`)

    await page.waitForFunction(
        () => {
            const app = document.querySelector('#app')
            return app && app.children.length > 0
        },
        { timeout: 30000 }
    )

    const currentUrl = page.url()
    // console.log(`[gotoHome] Current URL after Vue mount: ${currentUrl}`)

    if (currentUrl.includes('/error')) {
        const bodyText = await page.locator('body').textContent()
        throw new Error(`App redirected to error page: ${currentUrl}\nContent: ${bodyText?.substring(0, 500)}`)
    }

    try {
        await expect(page.getByTestId('mode-simple')).toBeVisible({ timeout: 30000 })
    } catch (error) {
        const bodyText = await page.locator('body').textContent()
        console.log(`[gotoHome] mode-simple not found. URL: ${currentUrl}`)
        console.log(`[gotoHome] Page content: ${bodyText?.substring(0, 500)}`)

        throw error
    }
}


const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)
const fixturesDir = path.resolve(__dirname, 'fixtures')


export function resolveFixturePath(name: string): string {
    return path.join(fixturesDir, name)
}


async function isToggleActive(toggle: Locator): Promise<boolean> {
    await toggle.waitFor({ state: 'visible', timeout: 30000 })
    return toggle.evaluate((btn) => btn.classList.contains('active'))
}


export async function setTextMode(page: Page, mode: 'Simple' | 'Advanced'): Promise<Locator> {
    const simpleModeToggle = getSimpleModeToggle(page)
    const advancedModeToggle = getAdvancedModeToggle(page)

    const target = mode === 'Simple' ? simpleModeToggle : advancedModeToggle

    if (await isToggleActive(target)) {
        await expect(getTextArea(page)).toBeVisible({ timeout: 10000 })
        return target
    }

    await clickElement(target)
    await expect(getTextArea(page)).toBeVisible({ timeout: 10000 })

    return target
}


declare global {
    interface Window {
        __copiedLink?: string
    }
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
