/// <reference types="node" />

import { test, expect } from '@playwright/test'


/*
 * Static pages tests: privacy, data-request.
 * These tests are independent and can run in parallel.
 */


// Scenario: Privacy & Data Request pages
// Steps:
// 1. Navigate to /privacy and /data-request and verify static content loads.
test('@smoke privacy and data-request static pages load', async ({ page }) => {
    await page.goto('/privacy', { waitUntil: 'networkidle' })

    await page.waitForSelector('.privacy-content h1', { timeout: 10000 })
    await expect(page.getByRole('heading', { name: /privacy policy|privacy policy|PRIVACY POLICY/i })).toBeVisible()

    await page.goto('/data-request')
    await page.waitForSelector('h1, h2, h3', { timeout: 10000 })
    await expect(page.getByRole('heading', { name: /data request|solicitud de datos/i })).toBeVisible()
})
