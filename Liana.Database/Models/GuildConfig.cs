using Liana.Database.Models.Enums;
using Liana.Database.Models.Modules;

namespace Liana.Database.Models;

public class GuildConfig
{
    public Dictionary<ulong, RoleEnum>? Roles { get; set; }
    public Dictionary<ulong, AuditLogConfig>? AuditLogs { get; set; }
}