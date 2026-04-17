using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class ContactBookEntryConfiguration : IEntityTypeConfiguration<ContactBookEntry>
{
    public void Configure(EntityTypeBuilder<ContactBookEntry> builder)
    {
        builder.ToTable("contact_book_entries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContactName).HasMaxLength(200);
        builder.Property(c => c.ContactEmail).HasMaxLength(320);
        builder.Property(c => c.ContactPhone).HasMaxLength(50);
        builder.Property(c => c.Notes).HasMaxLength(1000);

        // Audit columns
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.CreatedById);
        builder.Property(c => c.UpdatedById);

        // Index for looking up contacts by owner + email
        builder.HasIndex(c => new { c.OwnerId, c.ContactEmail })
               .HasDatabaseName("ix_contacts_ownerid_email");

        // Each user can be linked at most once per owner's contact book.
        // PostgreSQL allows multiple NULLs in a unique index natively,
        // so no HasFilter() is needed.
        builder.HasIndex(c => new { c.OwnerId, c.LinkedUserId })
               .IsUnique()
               .HasDatabaseName("ix_contacts_ownerid_linkeduserid");

        // Navigation: linked platform user (no cascade — deleting a user
        // should set LinkedUserId to null, not delete the contact entry)
        builder.HasOne(c => c.LinkedUser)
               .WithMany()
               .HasForeignKey(c => c.LinkedUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
