namespace Warp.WebApp.Services;

public static class IdCoder
{
    public static Guid Decode(string id)
    {
        var restored = id.Replace(Char62Replacement, Char62)
            .Replace(Char63Replacement, Char63);

        Span<byte> buffer = stackalloc byte[GuidSize];

        return Convert.TryFromBase64String(restored + Padding, buffer, out _)
            ? new Guid(buffer)
            : Guid.Empty;
    }


    public static string Encode(in Guid id)
        => Convert.ToBase64String(id.ToByteArray())
            .Replace(Char62, Char62Replacement)
            .Replace(Char63, Char63Replacement)
            [..EncodedStringLengthWithoutPadding];


    private const char Char62 = '+';
    private const char Char62Replacement = '-';
    private const char Char63 = '/';
    private const char Char63Replacement = '_';
    private const byte GuidSize = 16;
    private const string Padding = "==";
    private const byte EncodedStringLengthWithoutPadding = 22;
}