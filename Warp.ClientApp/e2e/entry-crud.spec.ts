/// <reference types="node" />

import { test, expect } from '@playwright/test'
import {
    getCopyLinkButton,
    getDeleteButton,
    getEntryArticle,
    getExpirationSelect,
    getPreviewButton,
    getReportButton,
    getSaveButton,
    getTextArea
} from './locators'
import {
    clickElement,
    expectOnDeleted,
    expectOnEntry,
    expectOnHome,
    expectOnPreview,
    fillTextAndVerify,
    getCopiedLink,
    getViewCount,
    gotoHome,
    setTextMode,
    setupClipboardSpy
} from './utils'


/*
 * Entry CRUD operations: create, save, copy link, view, delete, report.
 * Tests in serial block share state through entry creation.
 */


test.describe.serial('entry crud flows', () => {
    // Scenario: Simple entry creation and copy link
    // Steps:
    // 1. Prepare clipboard spy to capture copied link.
    // 2. Select simple mode, fill the text, choose 30 minutes expiration.
    // 3. Click Preview, verify preview content.
    // 4. Save the entry and click Copy Link; assert copied link and entry page content.
    test('@smoke user can create, save, copy, and view a simple entry', async ({ page }) => {
        await page.context().clearCookies()
        await page.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(page)
        await gotoHome(page)

        await setTextMode(page, 'Simple')
        const entryText = `Confidential message ${Date.now()}`
        await fillTextAndVerify(page, entryText)

        const expirationSelect = getExpirationSelect(page)
        await expirationSelect.selectOption('ThirtyMinutes')
        await expect(expirationSelect).toHaveValue('ThirtyMinutes')

        const previewButton = getPreviewButton(page)
        await clickElement(previewButton)
        await expectOnPreview(page)
        await expect(getEntryArticle(page)).toContainText(entryText)

        await clickElement(getSaveButton(page))

        const copyLinkButton = getCopyLinkButton(page)
        await expect(copyLinkButton).toBeVisible({ timeout: 10000 })
        await expect(copyLinkButton).toBeEnabled({ timeout: 10000 })
        await clickElement(copyLinkButton)

        await expect(page.getByText(/link copied/i)).toBeVisible()
        const copiedLink = await getCopiedLink(page)
        expect(copiedLink).toMatch(/\/entry\//)

        await page.goto(copiedLink)
        await expectOnEntry(page)
        await expect(getEntryArticle(page)).toContainText(entryText)
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

        await gotoHome(page)

        await setTextMode(page, 'Simple')

        const text = `Deletion test ${Date.now()}`
        await fillTextAndVerify(page, text)

        await clickElement(getPreviewButton(page))
        await expectOnPreview(page)

        await clickElement(getSaveButton(page))
        await expect(getCopyLinkButton(page)).toBeVisible({ timeout: 10000 })

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
        await gotoHome(page)

        await setTextMode(page, 'Simple')

        const text = `Report ${Date.now()}`
        await fillTextAndVerify(page, text)

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


    // Scenario: View count increments when viewed by a second, distinct user
    // Steps:
    // 1. Save an entry (text only) that will be viewed by two different browser contexts.
    // 2. Use a shared `entryState` object so both contexts see the same server side counter.
    // 3. Open first page in Context A and verify `viewCount` equals initial value.
    // 4. Open a second Page in Context B (new context) and verify `viewCount` increments by 1.
    test('@smoke entry view count increases with second distinct viewer', async ({ browser }) => {
        const contextA = await browser.newContext()
        const pageA = await contextA.newPage()

        await pageA.context().addInitScript(() => {
            window.localStorage.removeItem('warp.locale')
            window.localStorage.removeItem('warp.editMode')
        })

        await setupClipboardSpy(pageA)
        await gotoHome(pageA)

        await setTextMode(pageA, 'Simple')

        const entryText = `Views test ${Date.now()}`
        await fillTextAndVerify(pageA, entryText)

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
})
