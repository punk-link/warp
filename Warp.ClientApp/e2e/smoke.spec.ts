import { test, expect } from '@playwright/test'

// Minimal smoke test for /app home page

test('home renders', async ({ page, baseURL }) => {
  await page.goto(baseURL ?? 'http://localhost:5173/app')
  await expect(page.getByRole('heading', { level: 1 })).toContainText(/warp/i)
})
