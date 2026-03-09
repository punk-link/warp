using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

public sealed class EntryValidatorOptions
{
    [Range(1, int.MaxValue)]
    public int MaxContentDeltaSize { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxHtmlSize { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxPlainTextSize { get; set; }
}
