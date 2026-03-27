import { afterEach, beforeEach, describe, it, expect } from 'vitest'
import { isAllowedImageExtension } from '../helpers/image-file-helper'


function makeFile(name: string): File {
    return new File([], name)
}


describe('isAllowedImageExtension', () => {
    beforeEach(() => {
        delete (window as any).appConfig
    })

    afterEach(() => {
        delete (window as any).appConfig
    })


    it('returns false when appConfig is not loaded', () => {
        expect(isAllowedImageExtension(makeFile('image.png'))).toBe(false)
    })


    it('returns false when allowedImageExtensions is an empty array', () => {
        (window as any).appConfig = { allowedImageExtensions: [] }

        expect(isAllowedImageExtension(makeFile('image.png'))).toBe(false)
    })


    it('returns false when allowedImageExtensions is absent from config', () => {
        (window as any).appConfig = {}

        expect(isAllowedImageExtension(makeFile('image.png'))).toBe(false)
    })


    it('returns false for a file with no extension', () => {
        (window as any).appConfig = { allowedImageExtensions: ['.png'] }

        expect(isAllowedImageExtension(makeFile('noextension'))).toBe(false)
    })


    it('returns true when the file extension matches a config entry', () => {
        (window as any).appConfig = { allowedImageExtensions: ['.png', '.jpg', '.webp'] }

        expect(isAllowedImageExtension(makeFile('photo.jpg'))).toBe(true)
    })


    it('returns false when the file extension is not in the config', () => {
        (window as any).appConfig = { allowedImageExtensions: ['.png', '.jpg'] }

        expect(isAllowedImageExtension(makeFile('file.svg'))).toBe(false)
    })


    it('is case-insensitive for file extension', () => {
        (window as any).appConfig = { allowedImageExtensions: ['.png'] }

        expect(isAllowedImageExtension(makeFile('IMAGE.PNG'))).toBe(true)
    })


    it('is case-insensitive for config entries', () => {
        (window as any).appConfig = { allowedImageExtensions: ['.PNG'] }

        expect(isAllowedImageExtension(makeFile('image.png'))).toBe(true)
    })
})
