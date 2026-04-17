using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(320); // RFC 5321 max

        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("ix_users_email");

        builder.Property(u => u.IsVerifiedPlatformUser)
               .HasDefaultValue(false);

        // Audit columns
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.CreatedById);
        builder.Property(u => u.UpdatedById);

        // Owned events (cascade delete: removing a user removes their events)
        builder.HasMany(u => u.OwnedEvents)
               .WithOne(e => e.Owner)
               .HasForeignKey(e => e.OwnerId)
               .OnDelete(DeleteBehavior.Cascade);

        // Subscriptions
        builder.HasMany(u => u.Subscriptions)
               .WithOne(s => s.User)
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Contact book entries this user owns
        builder.HasMany<ContactBookEntry>()
               .WithOne(c => c.Owner)
               .HasForeignKey(c => c.OwnerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
