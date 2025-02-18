using Liana.Models.Enums;
using Liana.Models.Modules;

namespace Liana.Models;

public class GuildConfig
{
    public Dictionary<ulong, RoleEnum>? Roles { get; set; }
    public Dictionary<ulong, AuditLogConfig>? AuditLogs { get; set; }
    public AutoroleConfig? Autorole { get; set; }
    public List<MessagingConfig>? Messaging { get; set; }
}