using AmarTools.BuildingBlocks.Common;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Base controller that standardises how <see cref="Result{T}"/> and <see cref="Result"/>
/// values are converted to HTTP responses across every controller in the solution.
///
/// Extend all API controllers from this class instead of <see cref="ControllerBase"/> directly.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Converts a <see cref="Result{T}"/> into the appropriate <see cref="IActionResult"/>.
    /// Success → 200 OK with payload.
    /// Failure → mapped HTTP status via <see cref="MapError"/>.
    /// </summary>
    protected IActionResult Ok<T>(Result<T> result)
        => result.IsSuccess ? base.Ok(result.Value) : MapError(result.Error);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> into a 201 Created response on success.
    /// </summary>
    protected IActionResult Created<T>(Result<T> result, string? routeName = null, object? routeValues = null)
    {
        if (result.IsFailure) return MapError(result.Error);
        return routeName is not null
            ? CreatedAtRoute(routeName, routeValues, result.Value)
            : StatusCode(201, result.Value);
    }

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> into 204 No Content on success.
    /// </summary>
    protected IActionResult NoContent(Result result)
        => result.IsSuccess ? base.NoContent() : MapError(result.Error);

    // ── Error mapping ─────────────────────────────────────────────────────────

    private ObjectResult MapError(Error error) => error.Type switch
    {
        ErrorType.NotFound     => NotFound(ToProblem(error)),
        ErrorType.Forbidden    => StatusCode(403, ToProblem(error)),
        ErrorType.Unauthorized => Unauthorized(ToProblem(error)),
        ErrorType.Validation   => UnprocessableEntity(ToProblem(error)),
        ErrorType.Conflict     => Conflict(ToProblem(error)),
        _                      => BadRequest(ToProblem(error))
    };

    private static ProblemDetails ToProblem(Error error) => new()
    {
        Title  = error.Code,
        Detail = error.Description
    };
}
