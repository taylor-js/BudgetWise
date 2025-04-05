using Microsoft.EntityFrameworkCore;
using BudgetWise.Areas.Identity.Data;
using BudgetWise.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BudgetWise.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationships and foreign keys
            modelBuilder.Entity<Category>(entity =>
            {
                /*entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);*/

                entity.Property(c => c.CategoryId)
                     .ValueGeneratedOnAdd(); // Removed PostgreSQL-specific UseIdentityAlwaysColumn()

                entity.Property(c => c.Title)
                    .HasMaxLength(50) // Changed from PostgreSQL-specific varchar to MaxLength
                    .IsRequired();

                entity.Property(c => c.Icon)
                    .HasMaxLength(5) // Changed from PostgreSQL-specific varchar to MaxLength
                    .IsRequired();

                entity.Property(c => c.Type)
                    .HasMaxLength(10) // Changed from PostgreSQL-specific varchar to MaxLength
                    .IsRequired();

                entity.Property(c => c.UserId)
                    .IsRequired(); // Removed PostgreSQL-specific text type
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                /*entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);*/

                entity.HasOne(t => t.Category)
                    .WithMany()
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(t => t.TransactionId)
                    .ValueGeneratedOnAdd(); // Removed PostgreSQL-specific UseIdentityAlwaysColumn()

                entity.Property(t => t.Note)
                    .HasMaxLength(75); // Changed from PostgreSQL-specific varchar to MaxLength

                entity.Property(t => t.Date)
                    .IsRequired(); // Removed PostgreSQL-specific timestamp without time zone

                entity.Property(t => t.Amount)
                    .IsRequired();

                entity.Property(c => c.UserId)
                   .IsRequired(); // Removed PostgreSQL-specific text type
            });
        }
    }
}