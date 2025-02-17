namespace Liana.Models.Constants;

public static class AuditLogMessages
{
    public const string ChannelCreate = "[{timestamp}] 🖊️ Channel {channel.mention} created";
    
    public const string ChannelUpdate = "[{timestamp}] 🖊️ Channel {channel.mention} renamed `#{channel.name}` -> `#{channel2.name}`";
    
    public const string ChannelDelete = "[{timestamp}] 🗑️ Channel {channel.name} ({channel.id}) deleted";
    
    public const string MessageUpdate =
        "[{timestamp}] ✏️ {member.tag} (`{member.id}`) edited a message in {channel.mention}\nBefore:\n{message.content}\nAfter:\n{message.edited}";
    
    public const string MessageDelete = "[{timestamp}] 🗑️ {member.tag} (`{member.id}`) message deleted in {channel.mention}:\nMessage:\n{message.content}";
    
    public const string VoiceChannelJoin = "[{timestamp}] 🔊 {member.tag} (`{member.id}`) joined the voice channel {channel.mention}";
    
    public const string VoiceChannelLeave = "[{timestamp}] 🔇 {member.tag} (`{member.id}`) left the voice channel {channel.mention}";
    
    public const string VoiceChannelSwitch = "[{timestamp}] 🔊 {member.tag} (`{member.id}`) switched voice channels {channel.mention} -> {channel2.mention}";
    
    public const string MemberAdd = "[{timestamp}] 📥 {member.tag} (`{member.id}`) joined guild (account age `{member.age}`)";
    
    public const string MemberRemove = "[{timestamp}] 📤 {user.tag} (`{user.id}`) left guild";
    
    public const string RoleAdd = "[{timestamp}] 🔑 {member.tag} (`{member.id}`) role added {role.mention}";
    
    public const string RoleRemove = "[{timestamp}] 🔑 {member.tag} (`{member.id}`) role removed {role.mention}";
    
    public const string RoleCreate = "[{timestamp}] 🖊️ Role created: {role.mention}";
    
    public const string RoleUpdate = "[{timestamp}] 🖊️ Role {role.mention} renamed `{role.name}` -> `{role2.name}`";
    
    public const string RoleDelete = "[{timestamp}] 🗑️ Role deleted: {role.name} (`{role.id}`)";
    
    public const string NicknameAdd = "[{timestamp}] ✍️ {member.tag} (`{member.id}`) added a nickname `{member.nick}`";
    
    public const string NicknameRemove = "[{timestamp}] ✍️ {member.tag} (`{member.id}`) removed their nickname `{member.nick}`";
    
    public const string NicknameUpdate = "[{timestamp}] ✍️ {member.tag} (`{member.id}`) nickname was changed from `{member.nick}` to `{member2.nick}`";
    
    public const string UsernameChange = "[{timestamp}] 📛 {user.tag} (`{user.id}`) updated their username from `{user.tag}` to `{user2.tag}`";
    
    public const string BanAdd = "[{timestamp}] 🔨 {user.tag} (`{user.id}`) was banned by {moderator.mention} for: `{reason}`";
    
    public const string BanRemove = "[{timestamp}] 🚨 {user.tag} (`{user.id}`) was unbanned by {moderator.mention}";
}