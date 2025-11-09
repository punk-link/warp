using Microsoft.Extensions.Logging;

namespace Warp.CodeGen.Models;

/// <summary>
/// Represents a single logging event
/// </summary>
public class LoggingEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DomainErrorDescription { get; set; }
    public bool GenerateLogMessage { get; set; } = true;
    public bool IncludeException { get; set; } = false;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool Obsolete { get; set; } = false;
    public int HttpCode { get; set; } = 200;
}