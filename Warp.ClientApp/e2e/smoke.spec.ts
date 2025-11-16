import { test, expect } from '@playwright/test'

// Minimal smoke test for /app home page

test('home renders', async ({ page }) => {
  await page.goto('/app')
  const heroHeading = page.getByRole('main').getByRole('heading', { level: 1 })
  await expect(heroHeading.first()).toContainText(/warp/i)
})
