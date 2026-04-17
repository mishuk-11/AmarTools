using AmarTools.InvoiceGenerator.Entities;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.InvoiceGenerator.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // This switch helps with DateTime handling in PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<CompanyInfo> CompanyInfos { get; set; } = null!;
        public DbSet<LineItem> LineItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === From Company (One-to-One) ===
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.FromCompany)
                .WithOne()                          // No inverse navigation (CompanyInfo.Invoice removed)
                .HasForeignKey<Invoice>(i => i.FromCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // === To Company (One-to-One) ===
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ToCompany)
                .WithOne()                          // No inverse navigation
                .HasForeignKey<Invoice>(i => i.ToCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Line Items (One-to-Many) ===
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Items)
                .WithOne(li => li.Invoice)
                .HasForeignKey(li => li.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Soft delete query filter (only on Invoice)
            modelBuilder.Entity<Invoice>()
                .HasQueryFilter(i => !i.IsDeleted);

            // Use identity columns for PostgreSQL
            modelBuilder.UseIdentityByDefaultColumns();
        }
    }
}