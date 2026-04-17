namespace AmarTools.BuildingBlocks.Common;

/// <summary>
/// A discriminated-union return type that forces callers to handle both
/// success and failure paths without relying on exceptions for control flow.
///
/// Usage pattern:
/// <code>
/// Result&lt;Event&gt; result = await _eventService.CreateAsync(command);
/// if (result.IsFailure) return result.Error;
/// return result.Value;
/// </code>
/// </summary>
/// <typeparam name="T">The payload type returned on success.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value    = value;
        IsSuccess = true;
        Error     = Error.None;
    }

    private Result(Error error)
    {
        _value    = default;
        IsSuccess = false;
        Error     = error;
    }

    /// <summary><c>true</c> when the operation completed without errors.</summary>
    public bool IsSuccess { get; }

    /// <summary><c>true</c> when the operation produced an error.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The successful payload. Throws <see cref="InvalidOperationException"/>
    /// if accessed on a failed result — always check <see cref="IsSuccess"/> first.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    /// <summary>
    /// The error details. Returns <see cref="Error.None"/> on a successful result.
    /// </summary>
    public Error Error { get; }

    // ── Factories ─────────────────────────────────────────────────────────────

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failed result carrying <paramref name="error"/>.</summary>
    public static Result<T> Failure(Error error) => new(error);

    // ── Implicit conversions (optional syntactic sugar) ───────────────────────

    /// <summary>Allows returning a value directly where a <see cref="Result{T}"/> is expected.</summary>
    public static implicit operator Result<T>(T value)    => Success(value);

    /// <summary>Allows returning an <see cref="Error"/> directly where a <see cref="Result{T}"/> is expected.</summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Non-generic result for operations that succeed or fail with no payload
/// (e.g. delete, send email).
/// </summary>
public sealed class Result
{
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error     = error;
    }

    /// <summary><c>true</c> when the operation completed without errors.</summary>
    public bool IsSuccess { get; }

    /// <summary><c>true</c> when the operation produced an error.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The error details. Returns <see cref="Error.None"/> on success.</summary>
    public Error Error { get; }

    /// <summary>A cached successful result (allocates once).</summary>
    public static readonly Result Ok = new(true, Error.None);

    /// <summary>Creates a failed result carrying <paramref name="error"/>.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Allows returning an <see cref="Error"/> directly where a <see cref="Result"/> is expected.</summary>
    public static implicit operator Result(Error error) => Failure(error);
}
