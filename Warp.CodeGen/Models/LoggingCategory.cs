namespace Warp.CodeGen.Models;

/// <summary>
/// Represents a category of logging events
/// </summary>
public class LoggingCategory
{
    public string Name { get; set; } = string.Empty;
    public LoggingEvent[] Events { get; set; } = [];
}
