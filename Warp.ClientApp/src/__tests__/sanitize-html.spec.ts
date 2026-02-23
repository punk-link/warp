import { describe, it, expect } from 'vitest'
import { sanitize, stripHtml, hasTextContent } from '../helpers/sanitize-html'


describe('sanitize', () => {
    it('allows safe tags from Tiptap output', () => {
        const html = '<p>Hello <strong>world</strong> with <em>emphasis</em></p>'
        
        const result = sanitize(html)
        
        expect(result).toContain('<p>')
        expect(result).toContain('<strong>')
        expect(result).toContain('<em>')
        expect(result).toContain('Hello')
    })


    it('allows headings h1-h3', () => {
        const html = '<h1>Title</h1><h2>Subtitle</h2><h3>Section</h3>'
        
        const result = sanitize(html)
        
        expect(result).toContain('<h1>Title</h1>')
        expect(result).toContain('<h2>Subtitle</h2>')
        expect(result).toContain('<h3>Section</h3>')
    })


    it('allows lists (ul, ol, li)', () => {
        const html = '<ul><li>Item 1</li><li>Item 2</li></ul><ol><li>First</li></ol>'
        
        const result = sanitize(html)
        
        expect(result).toContain('<ul>')
        expect(result).toContain('<ol>')
        expect(result).toContain('<li>')
    })


    it('allows links with href attribute', () => {
        const html = '<a href="https://example.com">Link</a>'
        
        const result = sanitize(html)
        
        expect(result).toContain('<a href="https://example.com">')
        expect(result).toContain('Link')
    })


    it('allows mailto links', () => {
        const html = '<a href="mailto:test@example.com">Email</a>'
        
        const result = sanitize(html)
        
        expect(result).toContain('mailto:test@example.com')
    })


    it('allows blockquote and code blocks', () => {
        const html = '<blockquote>Quote</blockquote><pre><code>const x = 1;</code></pre>'
        
        const result = sanitize(html)
        
        expect(result).toContain('<blockquote>')
        expect(result).toContain('<pre>')
        expect(result).toContain('<code>')
    })


    it('removes script tags', () => {
        const html = '<p>Safe</p><script>alert("xss")</script>'
        
        const result = sanitize(html)
        
        expect(result).toContain('Safe')
        expect(result).not.toContain('<script>')
        expect(result).not.toContain('alert')
    })


    it('removes javascript: hrefs', () => {
        const html = '<a href="javascript:alert(1)">Click</a>'
        
        const result = sanitize(html)
        
        expect(result).not.toContain('javascript:')
        expect(result).not.toContain('alert')
    })


    it('removes onclick and other event handlers', () => {
        const html = '<p onclick="alert(1)" onmouseover="alert(2)">Text</p>'
        
        const result = sanitize(html)
        
        expect(result).toContain('Text')
        expect(result).not.toContain('onclick')
        expect(result).not.toContain('onmouseover')
    })


    it('removes style attributes', () => {
        const html = '<p style="color: red; display: none;">Text</p>'
        
        const result = sanitize(html)
        
        expect(result).toContain('Text')
        expect(result).not.toContain('style=')
    })


    it('removes data attributes', () => {
        const html = '<p data-custom="value" data-id="123">Text</p>'
        
        const result = sanitize(html)
        
        expect(result).toContain('Text')
        expect(result).not.toContain('data-custom')
        expect(result).not.toContain('data-id')
    })


    it('removes disallowed tags like div, iframe, img', () => {
        const html = '<div><iframe src="evil.com"></iframe><img src="x" onerror="alert(1)"></div>'
        
        const result = sanitize(html)
        
        expect(result).not.toContain('<div>')
        expect(result).not.toContain('<iframe>')
        expect(result).not.toContain('<img')
    })


    it('strips base64 data URIs in href', () => {
        const html = '<a href="data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==">Link</a>'
        
        const result = sanitize(html)
        
        expect(result).not.toContain('data:text/html')
    })


    it('allows http and https schemes', () => {
        const html = '<a href="http://example.com">HTTP</a><a href="https://example.com">HTTPS</a>'
        
        const result = sanitize(html)
        
        expect(result).toContain('http://example.com')
        expect(result).toContain('https://example.com')
    })


    it('returns empty string for empty input', () => {
        expect(sanitize('')).toBe('')
        expect(sanitize('   ')).toBe('')
    })


    it('handles nested allowed tags', () => {
        const html = '<ul><li><strong>Bold item</strong></li><li><em>Italic item</em></li></ul>'
        
        const result = sanitize(html)
        
        expect(result).toContain('<ul>')
        expect(result).toContain('<strong>')
        expect(result).toContain('<em>')
        expect(result).toContain('Bold item')
    })


    it('preserves content while removing dangerous tags', () => {
        const html = '<div><p>Safe paragraph</p><script>evil()</script></div>'
        
        const result = sanitize(html)
        
        expect(result).toContain('Safe paragraph')
        expect(result).toContain('<p>')
        expect(result).not.toContain('<div>')
        expect(result).not.toContain('evil')
    })
})


describe('stripHtml', () => {
    it('removes all HTML tags', () => {
        const html = '<p>Hello <strong>world</strong></p>'
        
        const result = stripHtml(html)
        
        expect(result).toBe('Hello world')
        expect(result).not.toContain('<')
        expect(result).not.toContain('>')
    })


    it('handles complex nested tags', () => {
        const html = '<h1>Title</h1><p>Text with <strong>bold</strong> and <em>italic</em>.</p><ul><li>Item</li></ul>'
        
        const result = stripHtml(html)
        
        expect(result).toContain('Title')
        expect(result).toContain('Text with bold and italic')
        expect(result).toContain('Item')
        expect(result).not.toContain('<')
    })


    it('returns empty string for empty input', () => {
        expect(stripHtml('')).toBe('')
        expect(stripHtml('   ')).toBe('')
    })


    it('returns empty string for HTML with only empty tags', () => {
        const html = '<p></p><br><p><br></p>'
        
        const result = stripHtml(html)
        
        expect(result).toBe('')
    })


    it('preserves whitespace-separated words', () => {
        const html = '<p>Word1</p><p>Word2</p><p>Word3</p>'
        
        const result = stripHtml(html)
        
        expect(result).toContain('Word1')
        expect(result).toContain('Word2')
        expect(result).toContain('Word3')
    })


    it('handles mixed text and tags', () => {
        const html = 'Plain text <strong>bold</strong> more text'
        
        const result = stripHtml(html)
        
        expect(result).toBe('Plain text bold more text')
    })
})


describe('hasTextContent', () => {
    it('returns true for HTML with text content', () => {
        expect(hasTextContent('<p>Hello world</p>')).toBe(true)
        expect(hasTextContent('<strong>Text</strong>')).toBe(true)
        expect(hasTextContent('<h1>Title</h1>')).toBe(true)
    })


    it('returns false for empty HTML', () => {
        expect(hasTextContent('')).toBe(false)
        expect(hasTextContent('   ')).toBe(false)
    })


    it('returns false for HTML with only empty tags', () => {
        expect(hasTextContent('<p></p>')).toBe(false)
        expect(hasTextContent('<p><br></p>')).toBe(false)
        expect(hasTextContent('<p></p><p></p><br>')).toBe(false)
    })


    it('returns false for HTML with only whitespace', () => {
        expect(hasTextContent('<p>   </p>')).toBe(false)
        expect(hasTextContent('<p>\n\t  </p>')).toBe(false)
    })


    it('returns true even with minimal text', () => {
        expect(hasTextContent('<p>A</p>')).toBe(true)
        expect(hasTextContent('<p>1</p>')).toBe(true)
        expect(hasTextContent('<p>.</p>')).toBe(true)
    })


    it('returns true for nested HTML with content', () => {
        expect(hasTextContent('<ul><li><strong>Item</strong></li></ul>')).toBe(true)
    })


    it('returns false for deeply nested empty tags', () => {
        expect(hasTextContent('<div><p><span></span></p></div>')).toBe(false)
    })
})
