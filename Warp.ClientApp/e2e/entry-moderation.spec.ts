/// <reference types="node" />

import { test, expect } from '@playwright/test'
import { expectOnEntry } from './utils'


const FAKE_ENTRY_ID = 'blurtestid1'

// 1×1 transparent GIF data URL — avoids any real network request for the fake image.
const FAKE_IMAGE_DATA_URL = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7'

const MOCKED_FLAGGED_ENTRY = {
    id: FAKE_ENTRY_ID,
    editMode: 1,
    expirationPeriod: 1,
    expiresAt: '2099-01-01T00:00:00.000Z',
    images: [
        {
            id: 'imgblur001',
            entryId: FAKE_ENTRY_ID,
            url: FAKE_IMAGE_DATA_URL,
            isBlurred: true,
            moderationResult: { status: 1, isFlagged: true, completedAt: '2024-01-01T00:00:00.000Z' }
        }
    ],
    excludedImages: [],
    rejectedFiles: [],
    textContent: 'Sensitive text content',
    contentDelta: null,
    viewCount: 3,
    isTextBlurred: true,
    textModerationResult: { status: 1, isFlagged: true, completedAt: '2024-01-01T00:00:00.000Z' }
}


/*
 * Moderation-driven blur flows: sensitive content overlays and per-item reveal.
 */


test.describe('entry blur flows', () => {
    // Scenario: Entry with flagged text and image shows blur overlays; each can be individually revealed.
    // Steps:
    // 1. Mock GET /api/entries/{id} to return an entry with isTextBlurred=true and one image with isBlurred=true.
    // 2. Navigate to the entry page.
    // 3. Assert the text content has the blur class and the text reveal overlay is visible.
    // 4. Assert the gallery has no data-fancybox anchor (image is sensitive).
    // 5. Click the Reveal button inside the image sensitive overlay; assert a data-fancybox anchor now exists.
    // 6. Click the Reveal button inside the text reveal overlay; assert the blur class is removed from the text content.
    test('blurred text and image show reveal overlays and can be revealed', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await page.route(`**/api/entries/${FAKE_ENTRY_ID}`, async (route) => {
            if (route.request().method() === 'GET') {
                await route.fulfill({
                    status: 200,
                    contentType: 'application/json',
                    body: JSON.stringify(MOCKED_FLAGGED_ENTRY)
                })
                
                return
            }

            await route.continue()
        })

        await page.goto(`/entry/${FAKE_ENTRY_ID}`)
        await expectOnEntry(page)

        const article = page.locator('article')
        const gallery = article.locator('.gallery')
        const textContent = article.locator('.text-content')

        // Text content is blurred.
        await expect(textContent).toHaveClass(/blur-sm/)

        // Text reveal overlay is visible (inside the .relative wrapper that contains .text-content).
        const textRevealOverlay = article.locator('.relative')
            .filter({ has: textContent })
            .locator('.absolute.inset-0')
        await expect(textRevealOverlay).toBeVisible()

        // Sensitive image: no fancybox anchor yet.
        await expect(gallery.locator('[data-fancybox="entry"]')).toHaveCount(0)

        // Image sensitive overlay is visible.
        const imageSensitiveOverlay = gallery.locator('.image-container .absolute.inset-0')
        await expect(imageSensitiveOverlay).toBeVisible()

        // Reveal the image by clicking the Reveal button inside the overlay — fancybox anchor should appear.
        await imageSensitiveOverlay.locator('button').click()
        await expect(gallery.locator('[data-fancybox="entry"]')).toHaveCount(1)

        // Reveal the text by clicking the Reveal button inside the overlay — blur class should be removed.
        await textRevealOverlay.locator('button').click()
        await expect(textContent).not.toHaveClass(/blur-sm/)
        await expect(textRevealOverlay).not.toBeVisible()
    })
})
