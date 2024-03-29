﻿namespace Warp.WebApp.Services;

public static class TextFormatter
{
    public static string Format(string text)
    {
        var paragraphs = text.ReplaceLineEndings()
            .Split(Environment.NewLine);

        var result = string.Empty;
        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
                continue;

            result += $"<p>{paragraph}</p>";
        }

        return result;
    }
}