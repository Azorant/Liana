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
    public DbSet<ReminderEntity> Reminders { get; set; }

    public async Task<string> GetRawConfig(ulong guildId)
    {
        var record = await Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
        if (record != null)
            return record.Config;

        record = new GuildEntity
        {
            Id = guildId,
            Config = GuildConfig.DefaultConfig
        };
        await AddAsync(record);
        await SaveChangesAsync();
        return record.Config;
    }

    public async Task<GuildConfig> GetConfig(ulong guildId) => Parser.DeserializeConfig(await GetRawConfig(guildId));

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

    public async Task ApplyMigrations()
    {
        var pending = (await Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Any())
        {
            Log.Information($"Applying {pending.Count} migrations: {string.Join(", ", pending)}");
            await Database.MigrateAsync();
            Log.Information("Migrations applied");
        }
        else
        {
            Log.Information("No migrations to apply.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageEntity>(
            entity =>
            {
                entity.Property(e => e.Attachments)
                    .HasColumnType("json");
                entity.Property(e => e.ContentEdits)
                    .HasColumnType("json");
                entity.Property(e => e.AttachmentsEdits)
                    .HasColumnType("json");
            });
    }
};