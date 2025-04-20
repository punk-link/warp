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
    public bool GenerateLogMessage { get; set; } = true;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}