/// <reference types="node" />

import { test, expect } from '@playwright/test'
import fs from 'fs'
import {
    getCopyLinkButton,
    getEditButton,
    getEntryArticle,
    getExpirationSelect,
    getImageFileInput,
    getPreviewButton,
    getPreviewGalleryImages,
    getEditorGalleryImages,
    getTextArea
} from './locators'
import {
    clickElement,
    dispatchPasteWithFiles,
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
 * Entry flows involving images: upload, gallery, fancybox, drag/drop, paste.
 * Tests in serial block share state through entry creation.
 */


test.describe.serial('entry image flows', () => {
    // Scenario: Advanced entry with image uploads, edit and verify copy equality
    // Steps:
    // 1. Set clipboard spy and prepare remote image URLs via the mocks.
    // 2. Switch to Advanced, fill text, upload one fixture image.
    // 3. Preview and confirm preview shows the image and text.
    // 4. Click Edit to go back to Home with draft hydrated (confirm text + gallery).
    // 5. Add a second image, preview again, save and copy link from Preview.
    // 6. Visit entry page and copy the link again; assert both copied links are identical.
    test('@smoke user can edit an advanced entry with images', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await gotoHome(page)

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

        await expect(textArea).toContainText(entryText)
        await expect(editorGalleryImages).toHaveCount(1)

        const previewImageTwo = resolveFixturePath('sample-image-2.png')
        await fileInput.setInputFiles(previewImageTwo)
        await expect(editorGalleryImages).toHaveCount(2)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await expect(previewGalleryImages).toHaveCount(2)

        await saveAndAwaitPostSaveButtons(page)
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
        await gotoHome(page)

        await setTextMode(page, 'Advanced')

        const text = `Lightbox ${Date.now()}`
        await getTextArea(page).fill(text)

        const previewImageOne = resolveFixturePath('sample-image-1.png')
        const fileInput = getImageFileInput(page)
        await fileInput.setInputFiles(previewImageOne)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await saveAndAwaitPostSaveButtons(page)
        await clickElement(getCopyLinkButton(page))
        const copied = await getCopiedLink(page)
        await page.goto(copied)
        await expectOnEntry(page)

        await page.waitForSelector('.gallery', { state: 'visible', timeout: 10000 })
        await page.waitForSelector('[data-fancybox="entry"] img', { state: 'visible', timeout: 10000 })
        await page.waitForFunction(() => !!(window as any).Fancybox, { timeout: 10000 })

        const fancyboxAnchor = page.locator('a[data-fancybox="entry"]').first()
        await fancyboxAnchor.waitFor({ state: 'visible', timeout: 10000 })
        await fancyboxAnchor.click()

        await page.waitForSelector('.fancybox__container, .fancybox-bg, .fancybox__stage', { timeout: 10000 })
        await expect(page.locator('.fancybox__container, .fancybox-bg, .fancybox__stage').first()).toBeVisible()

        await page.keyboard.press('Escape')
        await page.waitForSelector('.fancybox__container, .fancybox-bg, .fancybox__stage', { state: 'detached', timeout: 10000 })
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

        await gotoHome(page)

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

        await gotoHome(page)

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

        await expect(getTextArea(page)).toContainText(entryText)
        await expect(getEditorGalleryImages(page)).toHaveCount(1)
    })


    // Scenario: Clone & Edit preserves existing images when adding new ones
    // Steps:
    // 1. Create and save an entry with 1 image.
    // 2. Click Clone & Edit to create a copy.
    // 3. Verify the cloned entry shows the image in the editor.
    // 4. Add a second image and preview.
    // 5. Save the entry and verify both images are preserved on the entry page.
    test('@smoke clone & edit preserves images when adding new ones', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await gotoHome(page)

        await setTextMode(page, 'Advanced')

        const entryText = `Clone images ${Date.now()}`
        await getTextArea(page).fill(entryText)

        const fileInput = getImageFileInput(page)
        const fixture1 = resolveFixturePath('sample-image-1.png')
        await fileInput.setInputFiles(fixture1)

        await expect(getEditorGalleryImages(page)).toHaveCount(1)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await expect(getPreviewGalleryImages(page)).toHaveCount(1)

        await saveAndAwaitPostSaveButtons(page)
        await clickElement(page.getByRole('button', { name: /clone edit|clone & edit/i }))
        await page.waitForURL(/\?id=/)

        await expect(getTextArea(page)).toContainText(entryText, { timeout: 10000 })
        await expect(getEditorGalleryImages(page)).toHaveCount(1, { timeout: 10000 })

        // Use a different fixture to avoid duplicate hash detection on server
        const fixture2 = resolveFixturePath('sample-image-2.png')
        await fileInput.setInputFiles(fixture2)
        await expect(getEditorGalleryImages(page)).toHaveCount(2)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)
        await expect(getPreviewGalleryImages(page)).toHaveCount(2, { timeout: 10000 })

        await saveAndAwaitPostSaveButtons(page)
        await clickElement(getCopyLinkButton(page))
        const link = await getCopiedLink(page)

        await page.goto(link)
        await expectOnEntry(page)
        await expect(page.locator('[data-fancybox="entry"] img')).toHaveCount(2)
        await expect(getEntryArticle(page)).toContainText(entryText)
    })
})
