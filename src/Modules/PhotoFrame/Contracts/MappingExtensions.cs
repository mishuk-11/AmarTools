using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;

namespace AmarTools.Modules.PhotoFrame.Contracts;

internal static class MappingExtensions
{
    internal static PhotoFrameSetupDto ToSetupDto(
        this PhotoFrameConfig config, IFileStorageService storage) =>
        new(
            config.Id,
            config.EventToolId,
            config.EventName,
            config.SponsorName,
            config.VenueName,
            config.EventDateTime,
            FrameImageUrl: config.FrameImagePath is not null
                ? storage.GetPublicUrl(config.FrameImagePath) : null,
            LogoImageUrl: config.LogoImagePath is not null
                ? storage.GetPublicUrl(config.LogoImagePath) : null,
            config.SharingSlug,
            SharingUrl: $"/api/photo-frame/public/{config.SharingSlug}",
            config.IsPublished,
            LandingPage: config.LandingPage?.ToLandingPageDto(config, storage)
        );

    internal static LandingPageDto ToLandingPageDto(
        this LandingPageConfig lp, PhotoFrameConfig config, IFileStorageService storage) =>
        new(
            lp.TemplateName,
            lp.BackgroundColor,
            BackgroundImageUrl: lp.BackgroundImagePath is not null
                ? storage.GetPublicUrl(lp.BackgroundImagePath) : null,
            lp.HeadlineText,
            lp.InstructionText,
            lp.DownloadButtonText,
            config.EventName,
            config.SponsorName,
            config.VenueName,
            config.EventDateTime,
            LogoUrl: config.LogoImagePath is not null
                ? storage.GetPublicUrl(config.LogoImagePath) : null,
            FrameImageUrl: config.FrameImagePath is not null
                ? storage.GetPublicUrl(config.FrameImagePath) : null
        );
}
