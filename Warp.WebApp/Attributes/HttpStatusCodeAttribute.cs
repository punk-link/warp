// This file is auto-generated. Do not edit directly.
using System;

namespace Warp.WebApp.Attributes;

/// <summary>
/// Specifies the HTTP status code associated with an enum value. 
/// Used primarily with LogEvents enum to indicate what HTTP status code should be returned for each logging event.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class HttpStatusCodeAttribute : Attribute
{
    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpStatusCodeAttribute"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    public HttpStatusCodeAttribute(int statusCode)
    {
        StatusCode = statusCode;
    }
}