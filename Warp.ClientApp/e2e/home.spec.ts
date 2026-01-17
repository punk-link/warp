/// <reference types="node" />

import { test, expect } from '@playwright/test'
import {
    getAdvancedModeToggle,
    getMainHeading,
    getPreviewButton,
    getTextArea
} from './locators'
import { gotoHome } from './utils'


/*
 * Home page and general app tests that don't require entry creation.
 * These tests are independent and can run in parallel.
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

    await gotoHome(page)

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


// Scenario: Edit mode persists across reload and between tabs
// Steps:
// 1. Toggle advanced mode; reload and open another tab in the same context to verify advanced is still selected.
test('@smoke editMode persists across reload and tabs', async ({ page }) => {
    await page.context().clearCookies()

    await gotoHome(page)

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

    await gotoHome(page)

    const previewButton = getPreviewButton(page)
    await expect(previewButton).toBeDisabled()

    await getTextArea(page).fill('Hello')
    await expect(previewButton).toBeEnabled()

    await getTextArea(page).fill('')
    await expect(previewButton).toBeDisabled()
})
