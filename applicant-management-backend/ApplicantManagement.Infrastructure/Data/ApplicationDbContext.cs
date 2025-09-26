using ApplicantManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApplicantManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Applicant entity
            modelBuilder.Entity<Applicant>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.FamilyName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.EmailAddress)
                .IsRequired()
                .HasMaxLength(100);
                
            modelBuilder.Entity<Applicant>()
                .Property(a => a.Address)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.Phone)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.Age)
                .IsRequired();

            modelBuilder.Entity<Applicant>()
                .Property(a => a.CountryOfOrigin)
                .IsRequired();

            modelBuilder.Entity<Applicant>()
                .Property(a => a.AppliedDate)
                .IsRequired();
                
            modelBuilder.Entity<Applicant>()
                .Property(a => a.Hired)
                .HasDefaultValue(false);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Applicant>()
                .Property(a => a.LastModifiedDate);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.CreatedBy)
                .HasMaxLength(100);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.LastModifiedBy)
                .HasMaxLength(100);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.DeletedDate);

            modelBuilder.Entity<Applicant>()
                .Property(a => a.DeletedReason)
                .HasMaxLength(500);
                
            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.EmailAddress);

            // Configure Country entity
            modelBuilder.Entity<Country>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Country>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Country>()
                .Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(10);
                
            modelBuilder.Entity<Country>()
                .Property(c => c.Region)
                .HasMaxLength(50);
                
            modelBuilder.Entity<Country>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            // Add indexes for performance
            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.EmailAddress)
                .IsUnique();

            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.AppliedDate);

            // Additional indexes for better query performance
            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.Hired);

            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.IsDeleted);

            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.CreatedDate);

            // Composite index for efficient soft delete queries
            modelBuilder.Entity<Applicant>()
                .HasIndex(a => new { a.IsDeleted, a.CreatedDate });
        }
    }
}