namespace Warp.CodeGen.Utilities;

/// <summary>
/// Utilities for file operations
/// </summary>
public static class FileUtilities
{
    public static void WriteToFile(string filePath, ReadOnlySpan<char> content)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        
        File.WriteAllText(filePath, content);
        Console.WriteLine($"Successfully generated {filePath}");
    }
}
