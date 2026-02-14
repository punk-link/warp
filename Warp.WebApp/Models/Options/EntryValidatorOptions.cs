using System.ComponentModel.DataAnnotations;

namespace Warp.WebApp.Models.Options;

public sealed class EntryValidatorOptions
{
    [Range(1, int.MaxValue)]
    public required int MaxContentDeltaSizeBytes { get; set; }
    [Range(1, int.MaxValue)]
    public required int MaxHtmlSizeBytes { get; set; }
    [Range(1, int.MaxValue)]
    public required int MaxPlainTextSizeBytes { get; set; }
}
