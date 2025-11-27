import type { Locator, Page } from '@playwright/test'


export function getMainHeading(page: Page): Locator {
    return page.getByRole('main').getByRole('heading', { level: 1 }).first()
}


export function getTextArea(page: Page): Locator {
    return page.getByLabel('Text')
}


export function getExpirationSelect(page: Page): Locator {
    return page.getByLabel('Expires in')
}


export function getPreviewButton(page: Page): Locator {
    return page.getByRole('button', { name: /preview/i })
}


export function getSaveButton(page: Page): Locator {
    return page.getByRole('button', { name: /^Save$/i })
}


export function getCopyLinkButton(page: Page): Locator {
    return page.getByRole('button', { name: /copy link/i })
}


export function getEditButton(page: Page): Locator {
    return page.getByRole('button', { name: /edit/i })
}


export function getDeleteButton(page: Page): Locator {
    return page.getByRole('button', { name: /delete/i })
}


export function getReportButton(page: Page): Locator {
    return page.getByRole('button', { name: /report/i })
}


export function getSimpleModeToggle(page: Page): Locator {
    return page.getByTestId('mode-simple')
}


export function getAdvancedModeToggle(page: Page): Locator {
    return page.getByTestId('mode-advanced')
}


export function getImageFileInput(page: Page): Locator {
    return page.locator('input[type="file"][accept="image/*"]')
}


export function getEditorGalleryImages(page: Page): Locator {
    return page.locator('.gallery img')
}


export function getPreviewGalleryImages(page: Page): Locator {
    return page.locator('article .gallery img')
}


export function getEntryArticle(page: Page): Locator {
    return page.locator('article')
}
