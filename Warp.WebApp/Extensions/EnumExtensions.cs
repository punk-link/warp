using System.Collections.Concurrent;
using System.ComponentModel;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Extensions;

public static class EnumExtensions
{
    public static string ToDescriptionString(this LoggingEvents target)
    {
        if (_descriptionCache.TryGetValue(target, out var cachedDescription))
            return cachedDescription;
            
        var description = GetDescriptionFromAttributes(target);
        _descriptionCache[target] = description;
        
        return description;
    }
    
    
    private static string GetDescriptionFromAttributes(LoggingEvents target)
    {
        var fieldInfo = target.GetType().GetField(target.ToString());
        if (fieldInfo is null)
            return target.ToString();
            
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 
            ? attributes[0].Description 
            : target.ToString();
    }


    private static readonly ConcurrentDictionary<LoggingEvents, string> _descriptionCache = new();
}
