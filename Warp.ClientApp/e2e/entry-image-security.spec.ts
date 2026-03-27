/// <reference types="node" />

import { test, expect } from '@playwright/test'
import {
    getEditorGalleryImages,
    getImageFileInput,
    getCopyLinkButton,
    getPreviewButton
} from './locators'
import {
    clickElement,
    expectOnEntry,
    expectOnPreview,
    getCopiedLink,
    gotoHome,
    resolveFixturePath,
    saveAndAwaitPostSaveButtons,
    setTextMode,
    setupClipboardSpy
} from './utils'


/*
 * Security invariants for file upload: extension allow-list, security headers.
 * These tests validate the full stack: appsettings → config.js → window.appConfig → gallery.
 */


test.describe('file upload security', () => {
    // Scenario: allowedImageExtensions flows from appsettings through config.js to the browser
    // Steps:
    // 1. Navigate to the home page.
    // 2. Read window.appConfig.allowedImageExtensions in the browser context.
    // 3. Assert it is a non-empty array and contains .png.
    test('@smoke allowedImageExtensions is populated from backend config', async ({ page }) => {
        await gotoHome(page)

        const extensions = await page.evaluate(() => (window as any).appConfig?.allowedImageExtensions)

        expect(extensions).toBeDefined()
        expect(Array.isArray(extensions)).toBe(true)
        expect((extensions as string[]).length).toBeGreaterThan(0)
        expect(extensions).toContain('.png')
    })


    // Scenario: SVG files are silently discarded by client-side extension validation before upload
    // Steps:
    // 1. Navigate to home page in Advanced mode.
    // 2. Upload a valid PNG image to confirm that the gallery does accept permitted files.
    // 3. Attempt to upload an SVG file via the file input.
    // 4. Assert the gallery count does not increase — SVG is not in allowedImageExtensions.
    test('@smoke SVG files are rejected by client-side extension validation', async ({ page }) => {
        await gotoHome(page)
        await setTextMode(page, 'Advanced')

        const fileInput = getImageFileInput(page)

        await fileInput.setInputFiles(resolveFixturePath('sample-image-1.png'))
        await expect(getEditorGalleryImages(page)).toHaveCount(1)

        await fileInput.setInputFiles({
            name: 'malicious.svg',
            mimeType: 'image/svg+xml',
            buffer: Buffer.from('<svg xmlns="http://www.w3.org/2000/svg"><script>alert(1)</script></svg>')
        })

        // Gallery count must remain 1 — the SVG was rejected by isAllowedImageExtension
        await expect(getEditorGalleryImages(page)).toHaveCount(1)
    })


    // Scenario: Backend responses include X-Content-Type-Options: nosniff security header
    // Steps:
    // 1. Register a response listener for /config.js before navigating.
    // 2. Navigate to the home page — the SPA always fetches /config.js on load.
    // 3. Assert the captured response carries x-content-type-options: nosniff.
    test('@smoke API responses include X-Content-Type-Options nosniff header', async ({ page }) => {
        const responsePromise = page.waitForResponse(
            response => response.url().includes('/config.js') && response.status() === 200
        )

        await gotoHome(page)

        const response = await responsePromise
        expect(response.headers()['x-content-type-options']).toBe('nosniff')
    })
})


test.describe.serial('image serving security headers', () => {
    // Scenario: Images are served with Content-Disposition: inline header
    // Steps:
    // 1. Create an Advanced entry with one image and save it.
    // 2. Register a response listener for image GET requests before navigating to the entry.
    // 3. Navigate to the saved entry page — browser fetches the image.
    // 4. Assert the image response has Content-Disposition starting with "inline".
    test('@smoke served images include Content-Disposition inline header', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await gotoHome(page)
        await setTextMode(page, 'Advanced')

        await page.getByLabel('Text').fill(`Security header test ${Date.now()}`)

        const fileInput = getImageFileInput(page)
        await fileInput.setInputFiles(resolveFixturePath('sample-image-1.png'))
        await expect(getEditorGalleryImages(page)).toHaveCount(1)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await saveAndAwaitPostSaveButtons(page)
        await clickElement(getCopyLinkButton(page))
        const entryLink = await getCopiedLink(page)

        const imageResponsePromise = page.waitForResponse(
            response => /\/api\/images\/entry-id\/.+\/image-id\/.+/.test(response.url()) && response.status() === 200,
            { timeout: 30000 }
        )

        await page.goto(entryLink)
        await expectOnEntry(page)

        const imageResponse = await imageResponsePromise
        expect(imageResponse.headers()['content-disposition']).toMatch(/^inline/)
    })
})
