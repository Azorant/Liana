namespace Liana.Models.Enums;

public enum AuditEventEnum
{
    ChannelCreate,
    ChannelUpdate,
    ChannelDelete,
    MessageUpdate,
    MessageDelete,
    VoiceChannelJoin,
    VoiceChannelLeave,
    VoiceChannelSwitch,
    MemberAdd,
    MemberRemove,
    MemberRoleAdd,
    MemberRoleRemove,
    RoleCreate,
    RoleUpdate,
    RoleDelete,
    NicknameAdd,
    NicknameRemove,
    NicknameUpdate,
    UsernameChange,
    BanAdd,
    BanRemove,
    All
}