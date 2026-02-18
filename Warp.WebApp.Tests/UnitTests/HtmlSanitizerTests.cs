using Warp.WebApp.Services;

namespace Warp.WebApp.Tests.UnitTests;

public class HtmlSanitizerTests
{
    [Fact]
    public void Sanitize_AllowedTags_PassThrough()
    {
        var html = "<p>Hello <strong>world</strong> with <em>emphasis</em> and <b>bold</b></p>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.Contains("<p>", result);
        Assert.Contains("<strong>", result);
        Assert.Contains("<em>", result);
        Assert.Contains("<b>", result);
    }


    [Fact]
    public void Sanitize_AllowedHeadings_PassThrough()
    {
        var html = "<h1>Title</h1><h2>Subtitle</h2><h3>Section</h3>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.Contains("<h1>", result);
        Assert.Contains("<h2>", result);
        Assert.Contains("<h3>", result);
    }


    [Fact]
    public void Sanitize_AllowedLists_PassThrough()
    {
        var html = "<ul><li>Item 1</li><li>Item 2</li></ul><ol><li>First</li><li>Second</li></ol>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.Contains("<ul>", result);
        Assert.Contains("<ol>", result);
        Assert.Contains("<li>", result);
    }


    [Fact]
    public void Sanitize_AllowedLinks_PassThroughWithHref()
    {
        var html = "<a href=\"https://example.com\">Link</a>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.Contains("<a href=\"https://example.com\">", result);
    }


    [Fact]
    public void Sanitize_AllowedCodeBlocks_PassThrough()
    {
        var html = "<pre><code>const x = 42;</code></pre>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.Contains("<pre>", result);
        Assert.Contains("<code>", result);
    }


    [Fact]
    public void Sanitize_AllowedBlockquote_PassesThrough()
    {
        var html = "<blockquote>Quote text</blockquote>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.Contains("<blockquote>", result);
    }


    [Fact]
    public void Sanitize_ScriptTag_Removed()
    {
        var html = "<p>Safe content</p><script>alert('xss')</script>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("alert", result);
        Assert.Contains("<p>", result);
    }


    [Fact]
    public void Sanitize_JavaScriptHref_Removed()
    {
        var html = "<a href=\"javascript:alert('xss')\">Click</a>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.DoesNotContain("javascript:", result);
    }


    [Fact]
    public void Sanitize_OnClickAttribute_Removed()
    {
        var html = "<p onclick=\"alert('xss')\">Click me</p>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.DoesNotContain("onclick", result);
        Assert.Contains("Click me", result);
    }


    [Fact]
    public void Sanitize_StyleAttribute_Removed()
    {
        var html = "<p style=\"color: red;\">Text</p>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.DoesNotContain("style=", result);
        Assert.Contains("Text", result);
    }


    [Fact]
    public void Sanitize_DisallowedTags_Removed()
    {
        var html = "<div><iframe src=\"evil.com\"></iframe><img src=\"x\" onerror=\"alert(1)\"></div>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.DoesNotContain("<div>", result);
        Assert.DoesNotContain("<iframe>", result);
        Assert.DoesNotContain("<img", result);
    }


    [Fact]
    public void Sanitize_DataAttributes_Removed()
    {
        var html = "<p data-custom=\"value\">Text</p>";
        
        var result = HtmlSanitizer.Sanitize(html);
        
        Assert.DoesNotContain("data-custom", result);
        Assert.Contains("Text", result);
    }


    [Fact]
    public void Sanitize_EmptyString_ReturnsEmpty()
    {
        var result = HtmlSanitizer.Sanitize("");
        
        Assert.Equal(string.Empty, result);
    }


    [Fact]
    public void Sanitize_Whitespace_ReturnsEmpty()
    {
        var result = HtmlSanitizer.Sanitize("   \n\t  ");
        
        Assert.Equal(string.Empty, result);
    }


    [Fact]
    public void GetPlainText_RemovesAllTags()
    {
        var html = "<p>Hello <strong>world</strong></p>";
        
        var result = HtmlSanitizer.GetPlainText(html);
        
        Assert.Equal("Hello world", result);
        Assert.DoesNotContain("<", result);
    }


    [Fact]
    public void GetPlainText_EmptyHtml_ReturnsEmpty()
    {
        var html = "<p><br></p>";
        
        var result = HtmlSanitizer.GetPlainText(html);
        
        Assert.Empty(result);
    }


    [Fact]
    public void GetPlainText_NormalizesWhitespace()
    {
        var html = "<p>Text   with    multiple    spaces</p>";
        
        var result = HtmlSanitizer.GetPlainText(html);
        
        Assert.Equal("Text with multiple spaces", result);
    }


    [Fact]
    public void GetPlainText_ComplexNestedHtml_ExtractsText()
    {
        var html = "<h1>Title</h1><p>Paragraph with <strong>bold</strong> and <em>italic</em>.</p><ul><li>Item 1</li><li>Item 2</li></ul>";
        
        var result = HtmlSanitizer.GetPlainText(html);
        
        Assert.Contains("Title", result);
        Assert.Contains("Paragraph with bold and italic", result);
        Assert.Contains("Item 1", result);
        Assert.DoesNotContain("<", result);
    }
}
