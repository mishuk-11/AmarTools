using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Queries.GetLandingPage;

/// <summary>
/// Public query: returns the landing page data for a given sharing slug.
/// Used by the guest-facing React page to render the upload UI.
/// </summary>
public sealed record GetLandingPageQuery(string SharingSlug)
    : IRequest<Result<LandingPageDto>>;
