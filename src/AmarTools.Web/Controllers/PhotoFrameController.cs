using AmarTools.Modules.PhotoFrame.Commands.ProcessGuestPhoto;
using AmarTools.Modules.PhotoFrame.Commands.DownloadProcessedPhoto;
using AmarTools.Modules.PhotoFrame.Commands.SetupPhotoFrame;
using AmarTools.Modules.PhotoFrame.Commands.UpdateLandingPage;
using AmarTools.Modules.PhotoFrame.Commands.UploadFrameImage;
using AmarTools.Modules.PhotoFrame.Commands.UploadLandingBackgroundImage;
using AmarTools.Modules.PhotoFrame.Commands.UploadLogoImage;
using AmarTools.Modules.PhotoFrame.Contracts;
using AmarTools.Modules.PhotoFrame.Queries.GetLandingPage;
using AmarTools.Modules.PhotoFrame.Queries.GetPhotoFrameSetup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Admin and guest-facing endpoints for the Photo Frame tool.
/// </summary>
[Route("api/photo-frame")]
[ApiController]
public sealed class PhotoFrameController : ApiControllerBase
{
    private readonly ISender _sender;

    public PhotoFrameController(ISender sender) => _sender = sender;

    /// <summary>
    /// Returns the current admin setup payload for a photo-frame-enabled event tool.
    /// </summary>
    [Authorize]
    [HttpGet("setup/{eventToolId:guid}")]
    [ProducesResponseType(typeof(PhotoFrameSetupDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSetup(Guid eventToolId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPhotoFrameSetupQuery(eventToolId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates the base configuration for a photo frame event tool.
    /// </summary>
    [Authorize]
    [HttpPost("setup")]
    [ProducesResponseType(typeof(PhotoFrameSetupDto), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> SetupPhotoFrame(
        [FromBody] SetupPhotoFrameRequest request,
        CancellationToken ct)
    {
        var command = new SetupPhotoFrameCommand(
            request.EventToolId,
            request.EventName,
            request.SponsorName,
            request.VenueName,
            request.EventDateTime);

        var result = await _sender.Send(command, ct);
        return Created(result);
    }

    /// <summary>
    /// Uploads or replaces the transparent PNG/JPG frame image used for guest photo compositing.
    /// </summary>
    [Authorize]
    [HttpPost("{photoFrameConfigId:guid}/frame-image")]
    [ProducesResponseType(typeof(PhotoFrameSetupDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UploadFrameImage(
        Guid photoFrameConfigId,
        [FromForm] UploadFrameImageRequest request,
        CancellationToken ct)
    {
        if (request.Image is null || request.Image.Length == 0)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "PhotoFrame.ImageRequired",
                Detail = "Please provide a frame image file."
            });

        await using var imageStream = request.Image.OpenReadStream();

        var command = new UploadFrameImageCommand(
            photoFrameConfigId,
            imageStream,
            request.Image.FileName,
            request.Image.ContentType);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Uploads or replaces the event logo used in the landing page and setup preview.
    /// </summary>
    [Authorize]
    [HttpPost("{photoFrameConfigId:guid}/logo-image")]
    [ProducesResponseType(typeof(PhotoFrameSetupDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UploadLogoImage(
        Guid photoFrameConfigId,
        [FromForm] UploadFileRequest request,
        CancellationToken ct)
    {
        if (request.Image is null || request.Image.Length == 0)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "PhotoFrame.ImageRequired",
                Detail = "Please provide a logo image file."
            });

        await using var imageStream = request.Image.OpenReadStream();

        var command = new UploadLogoImageCommand(
            photoFrameConfigId,
            imageStream,
            request.Image.FileName,
            request.Image.ContentType);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Uploads or replaces the landing page background image.
    /// </summary>
    [Authorize]
    [HttpPost("{photoFrameConfigId:guid}/landing-page/background-image")]
    [ProducesResponseType(typeof(PhotoFrameSetupDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UploadLandingBackgroundImage(
        Guid photoFrameConfigId,
        [FromForm] UploadFileRequest request,
        CancellationToken ct)
    {
        if (request.Image is null || request.Image.Length == 0)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "PhotoFrame.ImageRequired",
                Detail = "Please provide a background image file."
            });

        await using var imageStream = request.Image.OpenReadStream();

        var command = new UploadLandingBackgroundImageCommand(
            photoFrameConfigId,
            imageStream,
            request.Image.FileName,
            request.Image.ContentType);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates the guest landing page and can publish/unpublish it.
    /// </summary>
    [Authorize]
    [HttpPatch("{photoFrameConfigId:guid}/landing-page")]
    [ProducesResponseType(typeof(PhotoFrameSetupDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UpdateLandingPage(
        Guid photoFrameConfigId,
        [FromBody] UpdateLandingPageRequest request,
        CancellationToken ct)
    {
        var command = new UpdateLandingPageCommand(
            photoFrameConfigId,
            request.TemplateName,
            request.BackgroundColor,
            request.HeadlineText,
            request.InstructionText,
            request.DownloadButtonText,
            request.Publish);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns the public landing page content for a published sharing slug.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("public/{sharingSlug}")]
    [ProducesResponseType(typeof(LandingPageDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLandingPage(string sharingSlug, CancellationToken ct)
    {
        var result = await _sender.Send(new GetLandingPageQuery(sharingSlug), ct);
        return Ok(result);
    }

    /// <summary>
    /// Processes a guest image upload against the published frame and returns the merged image URL.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("public/{sharingSlug}/process")]
    [ProducesResponseType(typeof(ProcessedPhotoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> ProcessGuestPhoto(
        string sharingSlug,
        [FromForm] ProcessGuestPhotoRequest request,
        CancellationToken ct)
    {
        if (request.Photo is null || request.Photo.Length == 0)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "PhotoFrame.ImageRequired",
                Detail = "Please provide a guest photo file."
            });

        await using var photoStream = request.Photo.OpenReadStream();

        var command = new ProcessGuestPhotoCommand(
            sharingSlug,
            photoStream,
            request.Photo.FileName,
            request.Photo.ContentType,
            request.OffsetX,
            request.OffsetY,
            request.Scale);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Marks the processed guest image as downloaded and redirects to the merged file URL.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("public/download/{sessionId:guid}")]
    [ProducesResponseType(302)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> DownloadProcessedPhoto(Guid sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new DownloadProcessedPhotoCommand(sessionId), ct);
        return result.IsSuccess
            ? Redirect(result.Value)
            : Problem(title: result.Error.Code, detail: result.Error.Description, statusCode: result.Error.Type switch
            {
                AmarTools.BuildingBlocks.Common.ErrorType.NotFound => 404,
                AmarTools.BuildingBlocks.Common.ErrorType.Validation => 422,
                AmarTools.BuildingBlocks.Common.ErrorType.Forbidden => 403,
                AmarTools.BuildingBlocks.Common.ErrorType.Unauthorized => 401,
                AmarTools.BuildingBlocks.Common.ErrorType.Conflict => 409,
                _ => 400
            });
    }
}

public sealed record SetupPhotoFrameRequest(
    Guid EventToolId,
    string EventName,
    string? SponsorName,
    string? VenueName,
    DateTime? EventDateTime
);

public sealed record UpdateLandingPageRequest(
    string TemplateName,
    string BackgroundColor,
    string? HeadlineText,
    string? InstructionText,
    string? DownloadButtonText,
    bool Publish
);

public sealed record UploadFrameImageRequest(IFormFile Image);

public sealed record UploadFileRequest(IFormFile Image);

public sealed record ProcessGuestPhotoRequest(
    IFormFile Photo,
    double OffsetX,
    double OffsetY,
    double Scale
);
