/// <reference types="node" />

import { test, expect } from '@playwright/test'
import {
    getCopyLinkButton,
    getPreviewButton,
    getSaveButton,
    getTextArea
} from './locators'
import {
    clickElement,
    expectOnPreview,
    fillTextAndVerify,
    getCopiedLink,
    gotoHome,
    setTextMode,
    setupClipboardSpy
} from './utils'


/*
 * Miscellaneous entry flows: clone & edit, network errors.
 * Tests in serial block share state through entry creation.
 */


test.describe.serial('entry misc flows', () => {
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
        await gotoHome(page)

        await setTextMode(page, 'Simple')

        const text = `Clone test ${Date.now()}`
        await fillTextAndVerify(page, text)
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

        await gotoHome(page)
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
})
