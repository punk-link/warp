using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using Warp.WebApp.Attributes;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Extensions;

public static class LogEventsExtensions
{
    public static string ToDescriptionString(this LogEvents target)
    {
        if (_descriptionCache.TryGetValue(target, out var cachedDescription))
            return cachedDescription;
            
        var description = GetDescriptionFromAttributes(target);
        _descriptionCache[target] = description;
        
        return description;
    }


    public static HttpStatusCode ToHttpStatusCode(this LogEvents target)
    {
        if (_statusCache.TryGetValue(target, out var cachedStatus))
            return cachedStatus;

        var status = GetHttpStatusCodeFromAttributes(target);
        _statusCache[target] = status;

        return status;
    }


    public static int ToHttpStatusCodeInt(this LogEvents target) 
        => (int)ToHttpStatusCode(target);


    private static string GetDescriptionFromAttributes(LogEvents target)
    {
        var attribute = GetAttribute<DescriptionAttribute>(target);
        if (attribute is not null)
            return attribute.Description;

        return target.ToString();
    }


    private static HttpStatusCode GetHttpStatusCodeFromAttributes(LogEvents target)
    {
        var attribute = GetAttribute<HttpStatusCodeAttribute>(target);
        if (attribute is not null)
            return (HttpStatusCode) attribute.StatusCode;

        return HttpStatusCode.InternalServerError;
    }


    private static T? GetAttribute<T>(this LogEvents target)
    {
        var fieldInfo = target.GetType().GetField(target.ToString());
        var attribute = fieldInfo?.GetCustomAttributes(typeof(T), false) as T[];
        if (attribute?.Length > 0)
            return attribute[0];

        return default;
    }


    private static readonly ConcurrentDictionary<LogEvents, string> _descriptionCache = new();
    private static readonly ConcurrentDictionary<LogEvents, HttpStatusCode> _statusCache = new();
}
