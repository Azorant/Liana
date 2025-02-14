namespace Liana.Models.Constants;

public static class AuditLogMessages
{
    public const string ChannelCreate = "[{timestamp}] 🖊️ Channel created: {channel.mention}";
    public const string ChannelUpdate = "[{timestamp}] 🖊️ Channel renamed {channel.name} -> {channel2.mention}";
    public const string ChannelDelete = "[{timestamp}] 🗑️ Channel deleted: {channel.name} ({channel.id})";
    public const string MessageUpdate =
        "[{timestamp}] ✏️ {user.tag} (`{user.id}`) edited a message in {channel.mention}\nOld message:\n{message.content}\nNew message:\n{message2.content}";
    public const string MessageDelete = "[{timestamp}] 🗑️ {user.tag} (`{user.id}`) message deleted in {channel.mention}:\nMessage:\n{message.content}";
    public const string VoiceChannelJoin = "[{timestamp}] 🔊 {user.tag} (`{user.id}`) joined the voice channel {channel.mention}";
    public const string VoiceChannelLeave = "[{timestamp}] 🔇 {user.tag} (`{user.id}`) left the voice channel {channel.mention}";
    public const string VoiceChannelSwitch = "[{timestamp}] 🔊 {user.tag} (`{user.id}`) switched voice channels {channel.mention} -> {channel2.mention}";
    public const string MemberAdd = "[{timestamp}] 📥 {user.tag} (`{user.id}`) joined guild (Account age `{user.age}`)";
    public const string MemberRemove = "[{timestamp}] 📤 {user.tag} (`{user.id}`) left guild";
    public const string RoleAdd = "[{timestamp}] 🔑 {user.tag} (`{user.id}`) role added: {role.mention}";
    public const string RoleRemove = "[{timestamp}] 🔑 {user.tag} (`{user.id}`) role removed: {role.mention}";
    public const string RoleCreate = "[{timestamp}] 🖊️ Role created: {role.mention}";
    public const string RoleDelete = "[{timestamp}] 🗑️ Role deleted: {role.name} (`{role.id}`)";
    public const string NicknameAdd = "[{timestamp}] ✍️ {user.tag} (`{user.id}`) added a nickname `{user.nick}`";
    public const string NicknameRemove = "[{timestamp}] ✍️ {user.tag} (`{user.id}`) removed their nickname `{user.oldNick}`";
    public const string NicknameUpdate = "[{timestamp}] ✍️ {user.tag} (`{user.id}`) nickname was changed from `{user.oldNick}` to `{user.nick}`";
    public const string UsernameChange = "[{timestamp}] 📛 {user.tag} (`{user.id}`) updated their username from `{user.oldTag}` to `{user.tag}`";
    public const string BanAdd = "[{timestamp}] 🔨 {user.tag} (`{user.id}`) was banned by {moderator.mention} for: `{reason}`";
    public const string BanRemove = "[{timestamp}] 🚨 {user.tag} (`{user.id}`) was unbanned by {moderator.mention}";
}