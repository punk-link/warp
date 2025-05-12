using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Controllers;

/// <summary>
/// Controller for managing creator authentication and lifecycle operations.
/// </summary>
public class CreatorController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatorController"/> class.
    /// </summary>
    /// <param name="cookieService">Service for managing creator authentication cookies.</param>
    /// <param name="creatorService">Service for managing creators.</param>
    public CreatorController(ICookieService cookieService, ICreatorService creatorService) : base(cookieService, creatorService)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
    }


    /// <summary>
    /// Retrieves the current creator from the authentication cookie, or creates and sets a new creator if none exists.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the creator information.
    /// Returns 200 OK with the creator if found or newly created.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetOrSet(CancellationToken cancellationToken = default)
    {
        var (isSuccess, _, creator, _) = await GetCreator(cancellationToken);
        if (isSuccess)
            return Ok(creator);

        creator = await _creatorService.Add(cancellationToken);
        await _cookieService.Set(HttpContext, creator);

        return Ok(creator);
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
}
