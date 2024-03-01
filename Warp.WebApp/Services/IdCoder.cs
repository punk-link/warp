namespace Warp.WebApp.Services;

public static class IdCoder
{
    public static Guid Decode(string id)
    {
        var restored = id.Replace('_', '/').Replace('-', '+');
        var buffer = new Span<byte>(new byte[16]);
        
        return Convert.TryFromBase64String(restored + "==", buffer, out _) 
            ? new Guid(buffer) 
            : Guid.Empty;
    }


    public static string Encode(in Guid id)
        => Convert.ToBase64String(id.ToByteArray())
            .Replace('/', '_')
            .Replace('+', '-')
            [..22];
}