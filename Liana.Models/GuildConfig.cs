using Liana.Models.Enums;
using Liana.Models.Modules;

namespace Liana.Models;

public class GuildConfig
{
    public static string DefaultConfig => $"# Get support at discord.gg/{Environment.GetEnvironmentVariable("DISCORD_INVITE")}";

    public Dictionary<ulong, RoleEnum>? Roles { get; set; }
    public Dictionary<ulong, AuditLogConfig>? AuditLogs { get; set; }
    public AutoroleConfig? Autorole { get; set; }
    public List<MessagingConfig>? Messaging { get; set; }
    public Dictionary<ulong, Dictionary<ulong, Dictionary<string, List<ReactionRoleConfig>>>>? ReactionRoles { get; set; }
}