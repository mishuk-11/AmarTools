using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Represents a single entry in an <see cref="ApplicationUser"/>'s Contact Book.
///
/// Two variants exist:
/// <list type="bullet">
///   <item>
///     <b>Plain contact</b> — an external person with a name/email stored manually.
///     <see cref="LinkedUserId"/> is <c>null</c>.
///   </item>
///   <item>
///     <b>Platform contact</b> — a reference to a verified AmarTools user.
///     <see cref="LinkedUserId"/> points to their <see cref="ApplicationUser.Id"/>,
///     and their profile data is resolved at query time rather than duplicated.
///   </item>
/// </list>
/// </summary>
public sealed class ContactBookEntry : AuditableEntity
{
    /// <summary>FK to the user who owns this contact book.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Navigation property to the contact book owner.</summary>
    public ApplicationUser Owner { get; private set; } = null!;

    // ── Plain contact fields (used when LinkedUserId is null) ─────────────────

    /// <summary>Display name for a plain contact.</summary>
    public string? ContactName { get; private set; }

    /// <summary>Email address for a plain contact.</summary>
    public string? ContactEmail { get; private set; }

    /// <summary>Optional phone number for a plain contact.</summary>
    public string? ContactPhone { get; private set; }

    /// <summary>Free-text notes about this contact.</summary>
    public string? Notes { get; private set; }

    // ── Platform contact fields ───────────────────────────────────────────────

    /// <summary>
    /// When set, this entry links to a verified AmarTools platform user.
    /// Display name and email are resolved from <see cref="ApplicationUser"/> at read time.
    /// </summary>
    public Guid? LinkedUserId { get; private set; }

    /// <summary>Navigation to the linked platform user. Null for plain contacts.</summary>
    public ApplicationUser? LinkedUser { get; private set; }

    /// <summary><c>true</c> when this entry references a verified platform user.</summary>
    public bool IsPlatformUser => LinkedUserId.HasValue;

    // ── Factory ───────────────────────────────────────────────────────────────

    private ContactBookEntry() { } // EF Core

    /// <summary>Creates a plain (non-platform) contact entry.</summary>
    public static ContactBookEntry CreatePlain(
        Guid ownerId, string contactName, string contactEmail,
        string? phone = null, string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contactName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactEmail);

        return new ContactBookEntry
        {
            OwnerId      = ownerId,
            ContactName  = contactName.Trim(),
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            ContactPhone = phone?.Trim(),
            Notes        = notes?.Trim()
        };
    }

    /// <summary>Creates a platform-linked contact entry.</summary>
    public static ContactBookEntry CreateLinked(Guid ownerId, Guid linkedUserId, string? notes = null)
    {
        if (ownerId == linkedUserId)
            throw new InvalidOperationException("A user cannot add themselves to their own contact book.");

        return new ContactBookEntry
        {
            OwnerId      = ownerId,
            LinkedUserId = linkedUserId,
            Notes        = notes?.Trim()
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Updates the fields of a plain contact.</summary>
    public void UpdatePlainContact(string name, string email, string? phone, string? notes)
    {
        if (IsPlatformUser)
            throw new InvalidOperationException("Cannot manually edit a linked platform contact.");

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        ContactName  = name.Trim();
        ContactEmail = email.Trim().ToLowerInvariant();
        ContactPhone = phone?.Trim();
        Notes        = notes?.Trim();
    }

    /// <summary>Updates the notes on any contact type.</summary>
    public void UpdateNotes(string? notes) => Notes = notes?.Trim();
}
