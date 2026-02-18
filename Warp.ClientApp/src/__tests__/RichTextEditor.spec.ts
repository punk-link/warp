import { render, waitFor } from '@testing-library/vue'
import { describe, it, expect, beforeEach } from 'vitest'
import RichTextEditor from '../components/RichTextEditor.vue'
import { createI18nInstance } from '../i18n'


describe('RichTextEditor.vue', () => {
    let i18n: Awaited<ReturnType<typeof createI18nInstance>>

    beforeEach(async () => {
        i18n = await createI18nInstance()
    })


    it('renders with empty content', async () => {
        const { container } = render(RichTextEditor, {
            props: {
                modelValue: '',
            },
            global: {
                plugins: [i18n],
            },
        })

        await waitFor(() => {
            expect(container.querySelector('.rich-text-editor')).toBeTruthy()
            expect(container.querySelector('.editor-toolbar')).toBeTruthy()
            expect(container.querySelector('.editor-content')).toBeTruthy()
        })
    })


    it('renders with initial JSON content', async () => {
        const json = JSON.stringify({
            type: 'doc',
            content: [
                {
                    type: 'paragraph',
                    content: [{ type: 'text', text: 'Hello world' }],
                },
            ],
        })

        const { container } = render(RichTextEditor, {
            props: {
                modelValue: json,
            },
            global: {
                plugins: [i18n],
            },
        })

        await waitFor(() => {
            const content = container.querySelector('.editor-content')
            expect(content?.textContent).toContain('Hello world')
        })
    })


    it('renders toolbar buttons', async () => {
        const { container } = render(RichTextEditor, {
            props: {
                modelValue: '',
            },
            global: {
                plugins: [i18n],
            },
        })

        await waitFor(() => {
            expect(container.querySelector('button[title="Bold"]')).toBeTruthy()
            expect(container.querySelector('button[title="Italic"]')).toBeTruthy()
            expect(container.querySelector('button[title="Underline"]')).toBeTruthy()
            expect(container.querySelector('button[title="Strikethrough"]')).toBeTruthy()
            expect(container.querySelector('button[title="Heading 1"]')).toBeTruthy()
            expect(container.querySelector('button[title="Heading 2"]')).toBeTruthy()
            expect(container.querySelector('button[title="Heading 3"]')).toBeTruthy()
            expect(container.querySelector('button[title="Bullet List"]')).toBeTruthy()
            expect(container.querySelector('button[title="Ordered List"]')).toBeTruthy()
            expect(container.querySelector('button[title="Blockquote"]')).toBeTruthy()
            expect(container.querySelector('button[title="Code Block"]')).toBeTruthy()
            expect(container.querySelector('button[title="Link"]')).toBeTruthy()
        })
    })


    it('respects editable prop for toolbar buttons', async () => {
        const { container } = render(RichTextEditor, {
            props: {
                modelValue: '',
                editable: false,
            },
            global: {
                plugins: [i18n],
            },
        })

        await waitFor(() => {
            const boldButton = container.querySelector('button[title="Bold"]') as HTMLButtonElement
            expect(boldButton?.disabled).toBe(true)
        })
    })


    it('enables toolbar buttons when editable is true', async () => {
        const { container } = render(RichTextEditor, {
            props: {
                modelValue: '',
                editable: true,
            },
            global: {
                plugins: [i18n],
            },
        })

        await waitFor(() => {
            const boldButton = container.querySelector('button[title="Bold"]') as HTMLButtonElement
            expect(boldButton?.disabled).toBe(false)
        })
    })


    it('handles placeholder prop', async () => {
        const placeholder = 'Enter your text here'
        const { container } = render(RichTextEditor, {
            props: {
                modelValue: '',
                placeholder,
            },
            global: {
                plugins: [i18n],
            },
        })

        await waitFor(() => {
            const editorElement = container.querySelector('.tiptap-editor')
            expect(editorElement?.getAttribute('placeholder')).toBe(placeholder)
        })
    })
})
