using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

public sealed class EntryValidatorOptions
{
    [Range(1, int.MaxValue)]
    public required int MaxContentDeltaSize { get; set; }
    [Range(1, int.MaxValue)]
    public required int MaxHtmlSize { get; set; }
    [Range(1, int.MaxValue)]
    public required int MaxPlainTextSize { get; set; }
}
