namespace Warp.CodeGen.Models;

/// <summary>
/// Represents the complete logging configuration from JSON
/// </summary>
public class LoggingConfig
{
    public LoggingCategory[] LoggingCategories { get; set; } = [];
}
