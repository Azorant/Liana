using Liana.Models.Enums;
using Liana.Models.Modules;

namespace Liana.Models;

public class GuildConfig
{
    public Dictionary<ulong, RoleEnum>? Roles { get; set; }
    public Dictionary<ulong, AuditLogConfig>? AuditLogs { get; set; }
}