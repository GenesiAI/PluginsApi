using AiPlugin.Domain.Plugin;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Infrastructure
{
    public class AiPluginDbContext : DbContext
    {
        public AiPluginDbContext(DbContextOptions<AiPluginDbContext> options) : base(options)
        {
        }

        // Genesi app core tables:
        public DbSet<Plugin> Plugins { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<PluginWhitelistedUser> PluginWhitelistedUsers { get; set; } 
        public DbSet<PluginWhitelist> PluginWhitelists { get; set; }

        // tables for handling subscriptions and interactions with Stripe:
        //public DbSet<Checkout> Checkouts { get; set; }
        // public DbSet<Customer> Customers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the many-to-many relationship between Plugin and PluginWhitelistedUser
            modelBuilder.Entity<PluginWhitelist>()
                .HasKey(pw => new { pw.PluginId, pw.Email });   // Define the composite primary key

            modelBuilder.Entity<PluginWhitelist>()
                .HasOne(pw => pw.Plugin)                        // Define the one-to-many relationship with Plugin
                .WithMany(p => p.PluginWhitelists)              // Define the inverse navigation property
                .HasForeignKey(pw => pw.PluginId);              // Define the foreign key

            modelBuilder.Entity<PluginWhitelist>()
                .HasOne(pw => pw.PluginWhitelistedUser)         // Define the one-to-many relationship with PluginWhitelistedUser
                .WithMany(pw => pw.PluginWhitelists)            // Define the inverse navigation property
                .HasForeignKey(pw => pw.Email);                 // Define the foreign key
        }
    }
}