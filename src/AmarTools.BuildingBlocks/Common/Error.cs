namespace AmarTools.BuildingBlocks.Common;

/// <summary>
/// Represents a structured, typed error that can be returned from any layer
/// without throwing exceptions for expected failure paths.
/// </summary>
/// <param name="Code">
/// A dot-namespaced machine-readable code (e.g. <c>"Event.LimitReached"</c>).
/// Used by the API middleware to map to HTTP status codes.
/// </param>
/// <param name="Description">A human-readable message safe to surface to clients.</param>
/// <param name="Type">Categorises the error for HTTP mapping.</param>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    /// <summary>Represents the absence of an error. Used inside <see cref="Result{T}"/>.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    // ── Convenience factories ─────────────────────────────────────────────────

    /// <summary>General business-rule or processing failure (maps to HTTP 400).</summary>
    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);

    /// <summary>Resource not found (maps to HTTP 404).</summary>
    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    /// <summary>Caller is not authorised to perform the operation (maps to HTTP 403).</summary>
    public static Error Forbidden(string code, string description)
        => new(code, description, ErrorType.Forbidden);

    /// <summary>Caller is not authenticated (maps to HTTP 401).</summary>
    public static Error Unauthorized(string code, string description)
        => new(code, description, ErrorType.Unauthorized);

    /// <summary>Input validation failed (maps to HTTP 422).</summary>
    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    /// <summary>A unique-constraint or duplicate-resource conflict (maps to HTTP 409).</summary>
    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);
}

/// <summary>Categorises an <see cref="Error"/> for HTTP status-code mapping.</summary>
public enum ErrorType
{
    None        = 0,
    Failure     = 1,
    NotFound    = 2,
    Forbidden   = 3,
    Unauthorized = 4,
    Validation  = 5,
    Conflict    = 6
}
