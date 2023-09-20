using AiPlugin.Domain.Plugin;
using AiPlugin.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Infrastructure
{
    public class AiPluginDbContext : DbContext
    {
        public AiPluginDbContext(DbContextOptions<AiPluginDbContext> options) : base(options)
        {
        }

        // Genesi app core tables:
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Plugin> Plugins { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;

        // tables for handling subscriptions and interactions with Stripe:
        //public DbSet<Checkout> Checkouts { get; set; }
        // public DbSet<Customer> Customers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}