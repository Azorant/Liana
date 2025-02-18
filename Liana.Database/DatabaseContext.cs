using Liana.Database.Entities;
using Liana.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Liana.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<GuildEntity> Guilds { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<GuildMemberEntity> GuildMembers { get; set; }

    public async Task<GuildConfig> GetConfig(ulong guildId)
    {
        var record = await Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
        if (record != null)
            return record.Config;

        record = new GuildEntity
        {
            Id = guildId,
            Config = new GuildConfig()
        };
        await AddAsync(record);
        await SaveChangesAsync();
        return record.Config;
    }

    public async Task<GuildMemberEntity> GetMember(ulong guildId, ulong userId)
    {
        var record = await GuildMembers.FirstOrDefaultAsync(m => m.Id == userId && m.GuildId == guildId);
        if (record != null) return record;
        record = new GuildMemberEntity
        {
            Id = userId,
            GuildId = guildId,
        };
        await AddAsync(record);
        await SaveChangesAsync();
        return record;
    }

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

        modelBuilder.Entity<MessageEntity>(
            entity =>
            {
                entity.Property(e => e.Attachments)
                    .HasColumnType("json");
            });
    }
};