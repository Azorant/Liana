using Liana.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Liana.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<GuildEntity> Guilds { get; set; }

    public void ApplyMigrations()
    {
        var pending = Database.GetPendingMigrations().ToList();
        if (pending.Any())
        {
            Log.Information($"Applying {pending.Count} migrations: {string.Join(',', pending)}");
            Database.Migrate();
            Log.Information("Migrations applied");
        }
        else
        {
            Log.Information("No migrations to apply.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildEntity>(
            entity =>
            {
                entity.Property(e => e.Config)
                    .HasColumnType("json");
            });
    }
};