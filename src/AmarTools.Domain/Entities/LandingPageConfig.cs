using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Stores the visual/design settings for the guest landing page
/// that belongs to a <see cref="PhotoFrameConfig"/>.
/// One-to-one with <see cref="PhotoFrameConfig"/>.
/// </summary>
public sealed class LandingPageConfig : AuditableEntity
{
    /// <summary>FK to the parent <see cref="PhotoFrameConfig"/>.</summary>
    public Guid PhotoFrameConfigId { get; private set; }

    /// <summary>Navigation to the parent photo frame config.</summary>
    public PhotoFrameConfig PhotoFrameConfig { get; private set; } = null!;

    // ── Template ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Visual template identifier. Front-end maps these to pre-built layouts.
    /// Built-in values: <c>"default"</c>, <c>"minimal"</c>, <c>"elegant"</c>.
    /// </summary>
    public string TemplateName { get; private set; } = "default";

    // ── Colours ───────────────────────────────────────────────────────────────

    /// <summary>Primary background colour as a CSS hex string (e.g. <c>"#3e256c"</c>).</summary>
    public string BackgroundColor { get; private set; } = "#3e256c";

    /// <summary>Optional background image (overrides <see cref="BackgroundColor"/> when set).</summary>
    public string? BackgroundImagePath { get; private set; }

    // ── Copy ──────────────────────────────────────────────────────────────────

    /// <summary>Optional headline shown above the upload area.</summary>
    public string? HeadlineText { get; private set; }

    /// <summary>Instruction text shown to guests before they upload their photo.</summary>
    public string InstructionText { get; private set; } =
        "Upload your photo, adjust the position, then download your personalised frame!";

    /// <summary>Label on the download button.</summary>
    public string DownloadButtonText { get; private set; } = "Download My Photo";

    // ── Factory ───────────────────────────────────────────────────────────────

    private LandingPageConfig() { }

    /// <summary>Creates a landing page config with sensible defaults.</summary>
    public static LandingPageConfig CreateDefault(Guid photoFrameConfigId) =>
        new() { PhotoFrameConfigId = photoFrameConfigId };

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Updates all landing page design settings.</summary>
    public void Update(
        string  templateName,
        string  backgroundColor,
        string? backgroundImagePath,
        string? headlineText,
        string? instructionText,
        string? downloadButtonText)
    {
        TemplateName         = templateName;
        BackgroundColor      = backgroundColor;
        BackgroundImagePath  = backgroundImagePath;
        HeadlineText         = headlineText?.Trim();
        InstructionText      = instructionText?.Trim() ?? InstructionText;
        DownloadButtonText   = downloadButtonText?.Trim() ?? DownloadButtonText;
    }
}
