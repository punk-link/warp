using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

/// <summary>
/// Provides functionality for managing creator authentication cookies.
/// </summary>
public interface ICookieService
{
    /// <summary>
    /// Retrieves the creator ID from the current HTTP context's authentication cookie.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing user claims.</param>
    /// <returns>The creator's GUID if found and valid; otherwise, null.</returns>
    public Guid? GetCreatorId(HttpContext httpContext);

    /// <summary>
    /// Sets the authentication cookie for a creator in the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to set the authentication cookie in.</param>
    /// <param name="creator">The creator whose ID will be stored in the authentication cookie.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Set(HttpContext httpContext, Creator creator);
}
