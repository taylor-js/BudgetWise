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
                     .UseIdentityAlwaysColumn()
                     .ValueGeneratedOnAdd();

                entity.Property(c => c.Title)
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(c => c.Icon)
                    .HasColumnType("varchar(5)")
                    .IsRequired();

                entity.Property(c => c.Type)
                    .HasColumnType("varchar(10)")
                    .IsRequired();

                entity.Property(c => c.UserId)
                    .HasColumnType("text")
                    .IsRequired();
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
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(t => t.TransactionId)
                    .UseIdentityAlwaysColumn()
                    .ValueGeneratedOnAdd();

                entity.Property(t => t.Note)
                    .HasColumnType("varchar(75)");

                entity.Property(t => t.Date)
                    .IsRequired()
                    .HasColumnType("timestamp without time zone");

                entity.Property(t => t.Amount)
                    .IsRequired();

                entity.Property(c => c.UserId)
                   .HasColumnType("text")
                   .IsRequired();
            });
        }
    }
}
