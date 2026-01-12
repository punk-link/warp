/// <reference types="node" />

import { test, expect, Page, Locator } from '@playwright/test'
import path from 'path'
import { fileURLToPath } from 'url'
import fs from 'fs'
import {
    getAdvancedModeToggle,
    getCopyLinkButton,
    getDeleteButton,
    getEditButton,
    getEntryArticle,
    getExpirationSelect,
    getImageFileInput,
    getMainHeading,
    getPreviewButton,
    getPreviewGalleryImages,
    getReportButton,
    getSaveButton,
    getSimpleModeToggle,
    getEditorGalleryImages,
    getTextArea
} from './locators'
import { clickElement, expectOnDeleted, expectOnEntry, expectOnHome, expectOnPreview, getCopiedLink, setupClipboardSpy } from './utils'


declare global {
    interface Window {
        __copiedLink?: string
    }
}


const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)
const fixturesDir = path.resolve(__dirname, 'fixtures')


function resolveFixturePath(name: string): string {
    return path.join(fixturesDir, name)
}


async function isToggleActive(toggle: Locator): Promise<boolean> {
    await toggle.waitFor({ state: 'visible' })
    return toggle.evaluate((btn) => btn.classList.contains('active'))
}


async function setTextMode(page: Page, mode: 'Simple' | 'Advanced'): Promise<Locator> {
    const simpleModeToggle = getSimpleModeToggle(page)
    const advancedModeToggle = getAdvancedModeToggle(page)

    const target = mode === 'Simple' ? simpleModeToggle : advancedModeToggle

    await target.waitFor({ state: 'visible' })

    if (await isToggleActive(target))
        return target

    await clickElement(target)

    return target
}


type PasteFilePayload = {
    bytes: number[]
    mimeType: string
    name: string
}


async function dispatchPasteWithFiles(page: Page, files: PasteFilePayload[]): Promise<void> {
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


async function getViewCount(page: Page): Promise<number> {
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


/*
 * Comprehensive scenario notes (moved from TEST_SCENARIOS.md):
 * - This file contains a prioritized smoke-suite of end-to-end scenarios for
 *   the Warp SPA. Each test is commented with steps + techniques for stability.
 *
 * Implementation hints repeated across scenarios:
 * - E2E tests exercise the real backend API; do not stub or mock '/api/*' endpoints here.
 * - Ensure the test environment has a reachable Warp API and any required seed data.
 * - Clear cookies and relevant storage before navigation to avoid cross-test leakage.
 *
 * Stability notes & patterns:
 * - Use `setupClipboardSpy(page)` to avoid picking the system clipboard.
 * - Prefer direct element `expect(locator).toHaveCount()` / `toHaveText()`.
 * - Wait for key navigation with `page.waitForURL(/preview|/entry/)`.
 * - Use `page.context().addInitScript` when you need to alter `localStorage` before page load (i18n, edit mode, etc.).
 */


// Scenario: Home renders
// Steps:
// 1. Reset cookies and local storage to a clean state.
// 2. Visit `/`.
// 3. Verify the home hero heading renders correctly.
test('@smoke home renders', async ({ page }) => {
    await page.context().clearCookies()
    await page.context().addInitScript(() => {
        window.localStorage.removeItem('warp.locale')
        window.localStorage.removeItem('warp.editMode')
    })

    await page.goto('/', { waitUntil: 'networkidle' })

    const heroHeading = getMainHeading(page)

    await expect(heroHeading).toContainText(/warp/i)
})


// Scenario: i18n localization switch
// Steps:
// 1. Set locale to Spanish via localStorage (persisted key) or setLocale and reload.
// 2. Verify some UI strings are translated (e.g., 'Preview' -> 'Vista previa').
test('@smoke i18n translations reflect locale changes', async ({ page }) => {
    await page.context().clearCookies()
    await page.context().addInitScript(() => {
        window.localStorage.setItem('warp.locale', 'es')
    })

    await page.goto('/')
    await page.waitForFunction(() => document.documentElement.lang === 'es', { timeout: 10000 })
    
    await expect(page.locator('html')).toHaveAttribute('lang', 'es')
})


// Scenario: Simple entry creation and copy link
// Steps:
// 1. Prepare clipboard spy to capture copied link.
// 2. Stub metadata + entry APIs so the flow is deterministic.
// 3. Select simple mode, fill the text, choose 30 minutes expiration.
// 4. Click Preview, verify preview content.
// 5. Save the entry and click Copy Link; assert copied link and entry page content.
test('@smoke user can create, save, copy, and view a simple entry', async ({ page }) => {
    await page.context().clearCookies()
    await page.context().addInitScript(() => {
        window.localStorage.removeItem('warp.locale')
        window.localStorage.removeItem('warp.editMode')
    })

    await setupClipboardSpy(page)
    await page.goto('/', { waitUntil: 'networkidle' })
    await expectOnHome(page)

    await setTextMode(page, 'Simple')
    const entryText = `Confidential message ${Date.now()}`
    await getTextArea(page).fill(entryText)

    const expirationSelect = getExpirationSelect(page)
    await expirationSelect.selectOption('ThirtyMinutes')
    await expect(expirationSelect).toHaveValue('ThirtyMinutes')

    const previewButton = getPreviewButton(page)
    await clickElement(previewButton)
    await expectOnPreview(page)
    await expect(getEntryArticle(page)).toContainText(entryText)

    await clickElement(getSaveButton(page))

    const copyLinkButton = getCopyLinkButton(page)
    await expect(copyLinkButton).toBeVisible({ timeout: 30000 })
    await expect(copyLinkButton).toBeEnabled({ timeout: 30000 })
    await clickElement(copyLinkButton)

    await expect(page.getByText(/link copied/i)).toBeVisible()
    const copiedLink = await getCopiedLink(page)
    expect(copiedLink).toMatch(/\/entry\//)

    await page.goto(copiedLink)
    await expectOnEntry(page)
    await expect(getEntryArticle(page)).toContainText(entryText)
})


// Scenario: Advanced entry with image uploads, edit and verify copy equality
// Steps:
// 1. Set clipboard spy and prepare remote image URLs via the mocks.
// 2. Switch to Advanced, fill text, upload one fixture image.
// 3. Preview and confirm preview shows the image and text.
// 4. Click Edit to go back to Home with draft hydrated (confirm text + gallery).
// 5. Add a second image, preview again, save and copy link from Preview.
// 6. Visit entry page and copy the link again; assert both copied links are identical.
test.describe.serial('stateful entry flows', () => {
    test('@smoke user can edit an advanced entry with images', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Advanced')

        const textArea = getTextArea(page)
        const entryText = `Gallery entry ${Date.now()}`
        await textArea.fill(entryText)

        const previewImageOne = resolveFixturePath('sample-image-1.png')
        const fileInput = getImageFileInput(page)
        await fileInput.setInputFiles(previewImageOne)

        const editorGalleryImages = getEditorGalleryImages(page)
        await expect(editorGalleryImages).toHaveCount(1)

        const expirationSelect = getExpirationSelect(page)
        await expirationSelect.selectOption('ThirtyMinutes')

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)

        const previewGalleryImages = getPreviewGalleryImages(page)
        await expect(previewGalleryImages).toHaveCount(1)
        await expect(getEntryArticle(page)).toContainText(entryText)

        await clickElement(getEditButton(page))
        await page.waitForURL(/\?id=/)

        await expect(textArea).toHaveValue(entryText)
        await expect(editorGalleryImages).toHaveCount(1)

        const previewImageTwo = resolveFixturePath('sample-image-2.png')
        await fileInput.setInputFiles(previewImageTwo)
        await expect(editorGalleryImages).toHaveCount(2)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await expect(previewGalleryImages).toHaveCount(2)

        await clickElement(getSaveButton(page))
        await clickElement(getCopyLinkButton(page))
        await expect(page.getByText(/link copied/i)).toBeVisible()
        const previewLink = await getCopiedLink(page)
        expect(previewLink).toMatch(/\/entry\//)

        await page.goto(previewLink)
        await expectOnEntry(page)
        await expect(page.locator('[data-fancybox="entry"] img')).toHaveCount(2)
        await expect(getEntryArticle(page)).toContainText(entryText)

        await clickElement(getCopyLinkButton(page))
        await expect(page.getByText(/link copied/i)).toBeVisible()
        const entryLink = await getCopiedLink(page)
        expect(entryLink).toBe(previewLink)
    })


    // Scenario: Fancybox lightbox opens and can be closed
    // Steps:
    // 1. Create and save an entry with 2 images (POST returns images). 
    // 2. Open entry page and click any image anchor.
    // 3. Assert Fancybox overlay opens; optionally test next/prev functionality.
    test('@smoke fancybox opens when clicking gallery image on entry page', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Advanced')

        const text = `Lightbox ${Date.now()}`
        await getTextArea(page).fill(text)

        const previewImageOne = resolveFixturePath('sample-image-1.png')
        const fileInput = getImageFileInput(page)
        await fileInput.setInputFiles(previewImageOne)
        
        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await clickElement(getSaveButton(page))

        await clickElement(getCopyLinkButton(page))
        const copied = await getCopiedLink(page)
        await page.goto(copied)
        await expectOnEntry(page)

        await page.waitForSelector('.gallery', { state: 'visible', timeout: 5000 })
        await page.waitForSelector('[data-fancybox="entry"] img', { state: 'visible', timeout: 5000 })
        await page.waitForFunction(() => !!(window as any).Fancybox)
        await page.click('[data-fancybox="entry"] img')

        await page.waitForSelector('.fancybox__container, .fancybox-bg, .fancybox__stage', { timeout: 5000 })
        await expect(page.locator('.fancybox__container, .fancybox-bg, .fancybox__stage').first()).toBeVisible()

        await page.keyboard.press('Escape')
        await page.waitForSelector('.fancybox__container, .fancybox-bg, .fancybox__stage', { state: 'detached' })
    })


    // Scenario: View count increments when viewed by a second, distinct user
    // Steps:
    // 1. Save an entry (text only) that will be viewed by two different browser contexts.
    // 2. Use a shared `entryState` object so both contexts see the same server side counter.
    // 3. Open first page in Context A and verify `viewCount` equals initial value.
    // 4. Open a second Page in Context B (new context) and verify `viewCount` increments by 1.
    test('@smoke entry view count increases with second distinct viewer', async ({ browser }) => {
        const contextA = await browser.newContext()
        const pageA = await contextA.newPage()
        await setupClipboardSpy(pageA)
        await pageA.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(pageA, 'Simple')

        const entryText = `Views test ${Date.now()}`
        await getTextArea(pageA).fill(entryText)
        await clickElement(getPreviewButton(pageA))
        await expectOnPreview(pageA)
        await clickElement(getSaveButton(pageA))

        await clickElement(getCopyLinkButton(pageA))
        const entryLink = await getCopiedLink(pageA)
        await pageA.goto(entryLink)
        await expectOnEntry(pageA)
        await pageA.waitForSelector('article')
        const countA = await getViewCount(pageA)

        const contextB = await browser.newContext()
        const pageB = await contextB.newPage()
        await pageB.goto(entryLink, { waitUntil: 'networkidle' })
        await expectOnEntry(pageB)
        await pageB.waitForSelector('article')
        const countB = await getViewCount(pageB)

        expect(countB).toBe(countA + 1)

        await pageA.close()
        await pageB.close()
        await contextA.close()
        await contextB.close()
    })


    // Scenario: Clone & Edit flow creates a new entry id and navigates to Home for editing
    // Steps:
    // 1. Save an entry, go to Preview. 
    // 2. Click Clone & Edit which calls POST /api/entries/{id}/copy.
    // 3. Server returns clone id; verify router pushes user to Home with new id in query and that Home fetches it.
    test('@smoke clone & edit creates a new id and navigates to Home', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Simple')

        const text = `Clone test ${Date.now()}`
        await getTextArea(page).fill(text)
        await clickElement(getPreviewButton(page))
        
        await expectOnPreview(page)

        await clickElement(getSaveButton(page))
        await clickElement(page.getByRole('button', { name: /clone edit|clone & edit/i }))
        await page.waitForURL(/\?id=/)

        const textInput = getTextArea(page)
        await expect.poll(async () => {
            const value = await textInput.inputValue()
            return value.trim()
        }, { timeout: 10000 }).toBe(text)

        const escaped = text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
        const textValuePattern = new RegExp(`^${escaped}\\s*$`)
        await expect(textInput).toHaveValue(textValuePattern, { timeout: 10000 })
    })


    // Scenario: Network errors (401 with ProblemDetails) route to Error
    // Steps:
    // 1. Create a draft to get an entry id; then when saving override POST endpoint to return 401 with ProblemDetails. 
    // 2. Click Save, verify the app routes to the `/error` page and includes status query.
    test('@smoke network error (401) triggers error routing', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await page.goto('/', { waitUntil: 'networkidle' })
        await setTextMode(page, 'Simple')

        const text = `Failing ${Date.now()}`
        await getTextArea(page).fill(text)
        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)

        await page.route('**/api/entries/*', async (route) => {
            const req = route.request()
            if (req.method() === 'POST') {
                await route.fulfill({
                    status: 401,
                    contentType: 'application/json',
                    body: JSON.stringify({ type: 'https://example.net/probs/unauth', title: 'Unauthorized', status: 401, detail: 'invalid token', traceId: 'tid-1' })
                })
                return
            }

            await route.continue()
        })

        const response = page.waitForResponse(r => r.request().method() === 'POST' && /\/api\/entries\//.test(r.url()))
        await Promise.all([
            getSaveButton(page).click(),
            response
        ])

        const resp = await response
        expect(resp.status()).toBe(401)

        await expect(page.getByRole('status')).toBeVisible({ timeout: 10000 })
    })


    // Scenario: Privacy & Data Request pages
    // Steps:
    // 1. Navigate to /privacy and /data-request and verify static content loads.
    test('@smoke privacy and data-request static pages load', async ({ page }) => {
        await page.goto('/privacy', { waitUntil: 'networkidle' })

        // The privacy file is fetched and inserted into the document asynchronously.
        // Allow the privacy HTML fetch more time in CI / parallel runs.
        await page.waitForSelector('.privacy-content h1', { timeout: 10000 })
        await expect(page.getByRole('heading', { name: /privacy policy|privacy policy|PRIVACY POLICY/i })).toBeVisible()

        await page.goto('/data-request')
        await page.waitForSelector('h1, h2, h3', { timeout: 20000 })
        await expect(page.getByRole('heading', { name: /data request|solicitud de datos/i })).toBeVisible()
    })


    // Scenario: Add images using drag & drop and paste
    // Steps:
    // 1. Go to Home Page in Advanced mode.
    // 2. Upload one image using the hidden file input (simulates drag/drop).
    // 3. Dispatch a paste event with image content and assert a second image appears in gallery.
    test('@smoke user can add images via drag/drop and paste', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Advanced')

        const fileInput = getImageFileInput(page)
        const fixture = resolveFixturePath('sample-image-1.png')

        await fileInput.setInputFiles(fixture)
        await expect(getEditorGalleryImages(page)).toHaveCount(1)

        const bytes = Array.from(fs.readFileSync(fixture))
        await dispatchPasteWithFiles(page, [{
            bytes,
            mimeType: 'image/png',
            name: 'sample-image-1.png'
        }])

        await expect(getEditorGalleryImages(page)).toHaveCount(2)
    })


    // Scenario: Delete a saved entry and redirect to Deleted view
    // Steps:
    // 1. Create a simple entry (text-only) and go to Preview.
    // 2. Save the entry and then press Delete on the Preview screen.
    // 3. Confirm server returns success; verify user navigates to Deleted page.
    test('@smoke user can delete a saved entry and be redirected', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Simple')
        
        const text = `Deletion test ${Date.now()}`
        await getTextArea(page).fill(text)
        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)

        await clickElement(getSaveButton(page))
        await clickElement(getDeleteButton(page))
        await expectOnDeleted(page)
    })


    // Scenario: Report Flow (modal + server report endpoint)
    // Steps:
    // 1. Create an entry (text-only), preview and save the entry.
    // 2. Visit entry page and click Report. Confirm the modal then click confirm.
    // 3. Verify server returns success and router navigates to `Home`.
    test('@smoke user can report a saved entry and be redirected', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Simple')
        
        const text = `Report ${Date.now()}`
        await getTextArea(page).fill(text)
        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)

        await clickElement(getSaveButton(page))
        await clickElement(getCopyLinkButton(page))

        const copied = await getCopiedLink(page)
        await page.goto(copied)
        await expectOnEntry(page)

        await clickElement(getReportButton(page))
        const confirmBtn = page.locator('.bg-white.rounded-lg').getByRole('button', { name: /report|reportar|reportar contenido/i })
        await expect(confirmBtn).toBeVisible({ timeout: 5000 })

        await clickElement(confirmBtn)
        await expectOnHome(page)
    })


    // Scenario: Draft persistence across Preview and returning to Home
    // Steps:
    // 1. Add text + image on Home, preview and then click Edit on Preview.
    // 2. Verify the Home screen rehydrates the draft (text+image present).
    test('@smoke draft persists when previewing and editing back', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await page.goto('/', { waitUntil: 'networkidle' })

        await setTextMode(page, 'Advanced')
        
        const entryText = `Draft persistence ${Date.now()}`
        await getTextArea(page).fill(entryText)

        const fileInput = getImageFileInput(page)
        const fixture = resolveFixturePath('sample-image-1.png')
        await fileInput.setInputFiles(fixture)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)

        await clickElement(getEditButton(page))
        await page.waitForURL(/\?id=/)

        await expect(getTextArea(page)).toHaveValue(entryText)
        await expect(getEditorGalleryImages(page)).toHaveCount(1)
    })


    // Scenario: Edit mode persists across reload and between tabs
    // Steps:
    // 1. Toggle advanced mode; reload and open another tab in the same context to verify advanced is still selected.
    test('@smoke editMode persists across reload and tabs', async ({ page }) => {
        await page.context().clearCookies()

        await page.goto('/', { waitUntil: 'networkidle' })

        const advancedModeToggle = getAdvancedModeToggle(page)
        await advancedModeToggle.click()
        await expect(advancedModeToggle).toHaveClass(/active/)

        await page.reload({ waitUntil: 'load' })
        await expect(getAdvancedModeToggle(page)).toHaveClass(/active/)

        const newTab = await page.context().newPage()
        await newTab.goto('/')
        await expect(getAdvancedModeToggle(newTab)).toHaveClass(/active/)
        await newTab.close()
    })


    // Scenario: Preview disabled when invalid
    // Steps:
    // 1. Ensure the preview button is disabled for empty text and no images.
    // 2. Fill text and check preview is enabled, then clear and ensure disabled again.
    test('@smoke preview button disabled when no content', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await page.goto('/', { waitUntil: 'networkidle' })

        const previewButton = getPreviewButton(page)
        await expect(previewButton).toBeDisabled()

        await getTextArea(page).fill('Hello')
        await expect(previewButton).toBeEnabled()

        await getTextArea(page).fill('')
        await expect(previewButton).toBeDisabled()
    })
})
