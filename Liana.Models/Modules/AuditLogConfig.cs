using Liana.Models.Enums;

namespace Liana.Models.Modules;

public class AuditLogConfig
{
    public List<ulong>? IncludedChannels { get; set; }
    public List<ulong>? ExcludedChannels { get; set; }
    public required List<AuditEventEnum> EnabledEvents { get; set; }
    public string? ChannelCreate { get; set; }
    public string? ChannelUpdate { get; set; }
    public string? ChannelDelete { get; set; }
    public string? MessageUpdate { get; set; }
    public string? MessageDelete { get; set; }
    public string? VoiceChannelJoin { get; set; }
    public string? VoiceChannelLeave { get; set; }
    public string? VoiceChannelSwitch { get; set; }
    public string? MemberAdd { get; set; }
    public string? MemberRemove { get; set; }
    public string? RoleAdd { get; set; }
    public string? RoleRemove { get; set; }
    public string? RoleCreate { get; set; }
    public string? RoleUpdate { get; set; }
    public string? RoleDelete { get; set; }
    public string? NicknameAdd { get; set; }
    public string? NicknameRemove { get; set; }
    public string? NicknameUpdate { get; set; }
    public string? UsernameChange { get; set; }
    public string? BanAdd { get; set; }
    public string? BanRemove { get; set; }
}