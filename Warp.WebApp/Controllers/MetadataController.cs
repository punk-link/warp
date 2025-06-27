using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Controllers;

/// <summary>
/// Controller for secondary metadata including OpenGraph descriptions and enum values.
/// </summary>
[ApiController]
[Route("/api/metadata")]
public class MetadataController : ControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataController"/> class.
    /// </summary>
    /// <param name="openGraphService">The OpenGraph service.</param>
    public MetadataController(IOpenGraphService openGraphService) 
    {
        _openGraphService = openGraphService;
    }


    /// <summary>
    /// Gets the default OpenGraph description for use in new entries.
    /// </summary>
    /// <returns>The default OpenGraph description.</returns>
    [HttpGet("opengraph/default")]
    [ProducesResponseType(typeof(EntryOpenGraphDescription), StatusCodes.Status200OK)]
    public IActionResult GetDefaultOpenGraphDescription() 
        => Ok(_openGraphService.GetDefaultDescription());


    /// <summary>
    /// Gets all available edit modes.
    /// </summary>
    /// <returns>Dictionary of edit mode values and their names.</returns>
    [HttpGet("enums/edit-modes")]
    [ProducesResponseType(typeof(Dictionary<int, string>), StatusCodes.Status200OK)]
    public IActionResult GetEditModes() 
        => Ok(ConvertEnumToDictionary<EditMode>());


    /// <summary>
    /// Gets all available expiration periods.
    /// </summary>
    /// <returns>Dictionary of expiration period values and their durations in minutes.</returns>
    [HttpGet("enums/expiration-periods")]
    [ProducesResponseType(typeof(Dictionary<int, string>), StatusCodes.Status200OK)]
    public IActionResult GetExpirationPeriods() 
        => Ok(ConvertEnumToDictionary<ExpirationPeriod>());


    public static Dictionary<int, string> ConvertEnumToDictionary<T>() where T : struct, Enum => 
        Enum.GetValues<T>()
            .ToDictionary(value => Convert.ToInt32(value), value => value.ToString());


    private readonly IOpenGraphService _openGraphService;
}
