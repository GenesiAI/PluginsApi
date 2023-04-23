using AiPlugin.Domain;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Infrastructure
{
    public class AiPluginDbContext : DbContext
    {
        public AiPluginDbContext(DbContextOptions<AiPluginDbContext> options) : base(options)
        {
        }

        public DbSet<Plugin> Plugins { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}