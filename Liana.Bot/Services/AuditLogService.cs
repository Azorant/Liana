using Discord;
using Discord.WebSocket;
using Liana.Database;
using Liana.Models.Constants;
using Liana.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Liana.Bot.Services;

public class AuditLogService(DatabaseContext db, DiscordSocketClient client)
{
    public async Task SendAuditLog(ulong guildId, ulong? channelId, AuditEventEnum auditEvent, FormatLogOptions options)
    {
        var guild = await db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
        if (guild?.Config.AuditLogs == null ||
            guild.Config.AuditLogs.Count == 0) return;

        var configs = guild.Config.AuditLogs.Where(c => c.Value.EnabledEvents.Contains(auditEvent) || c.Value.EnabledEvents.Contains(AuditEventEnum.All)).ToList();

        foreach (var (logChannelId, config) in configs)
        {
            // Event didn't happen in a whitelisted channel
            if (config.IncludedChannels is { Count: > 0 } && !config.IncludedChannels.Contains(channelId.GetValueOrDefault())) continue;
            // Event happened in a blacklisted channel
            if (config.ExcludedChannels is { Count: > 0 } && config.ExcludedChannels.Contains(channelId.GetValueOrDefault())) continue;
            var logChannel = await client.GetChannelAsync(logChannelId);
            if (logChannel is not SocketTextChannel channel || channel.Guild.Id != guildId) continue;

            var permissions = channel.Guild.CurrentUser.GetPermissions(channel);
            if (!permissions.ViewChannel || !permissions.SendMessages) continue;

            var message = auditEvent switch
            {
                AuditEventEnum.ChannelCreate => config.ChannelCreate ?? AuditLogMessages.ChannelCreate,
                AuditEventEnum.ChannelUpdate => config.ChannelUpdate ?? AuditLogMessages.ChannelUpdate,
                AuditEventEnum.ChannelDelete => config.ChannelDelete ?? AuditLogMessages.ChannelDelete,
                AuditEventEnum.MessageUpdate => config.MessageUpdate ?? AuditLogMessages.MessageUpdate,
                AuditEventEnum.MessageDelete => config.MessageDelete ?? AuditLogMessages.MessageDelete,
                AuditEventEnum.VoiceChannelJoin => config.VoiceChannelJoin ?? AuditLogMessages.VoiceChannelJoin,
                AuditEventEnum.VoiceChannelLeave => config.VoiceChannelLeave ?? AuditLogMessages.VoiceChannelLeave,
                AuditEventEnum.VoiceChannelSwitch => config.VoiceChannelSwitch ?? AuditLogMessages.VoiceChannelSwitch,
                AuditEventEnum.MemberAdd => config.MemberAdd ?? AuditLogMessages.MemberAdd,
                AuditEventEnum.MemberRemove => config.MemberRemove ?? AuditLogMessages.MemberRemove,
                AuditEventEnum.MemberRoleAdd => config.RoleAdd ?? AuditLogMessages.RoleAdd,
                AuditEventEnum.MemberRoleRemove => config.RoleRemove ?? AuditLogMessages.RoleRemove,
                AuditEventEnum.RoleCreate => config.RoleCreate ?? AuditLogMessages.RoleCreate,
                AuditEventEnum.RoleUpdate => config.RoleUpdate ?? AuditLogMessages.RoleUpdate,
                AuditEventEnum.RoleDelete => config.RoleDelete ?? AuditLogMessages.RoleDelete,
                AuditEventEnum.NicknameAdd => config.NicknameAdd ?? AuditLogMessages.NicknameAdd,
                AuditEventEnum.NicknameRemove => config.NicknameRemove ?? AuditLogMessages.NicknameRemove,
                AuditEventEnum.NicknameUpdate => config.NicknameUpdate ?? AuditLogMessages.NicknameUpdate,
                AuditEventEnum.UsernameChange => config.UsernameChange ?? AuditLogMessages.UsernameChange,
                AuditEventEnum.BanAdd => config.BanAdd ?? AuditLogMessages.BanAdd,
                AuditEventEnum.BanRemove => config.BanRemove ?? AuditLogMessages.BanRemove,
                _ => $"{auditEvent.ToString()} event is missing template"
            };

            // TODO: Message queueing system
            await channel.SendMessageAsync(Formatter.FormatLog(message, options), allowedMentions: AllowedMentions.None);
        }
    }

    public async Task SendAuditLog(SocketGuild guild, ulong? channelId, AuditEventEnum auditEvent, FormatLogOptions options)
    {
        await SendAuditLog(guild.Id, channelId, auditEvent, options);
    }

    public async Task SendAuditLog(SocketGuild guild, SocketGuildChannel? channel, AuditEventEnum auditEvent, FormatLogOptions options)
    {
        await SendAuditLog(guild.Id, channel?.Id, auditEvent, options);
    }

    public async Task SendAuditLog(ulong guildId, AuditEventEnum auditEvent, FormatLogOptions options)
    {
        await SendAuditLog(guildId, null, auditEvent, options);
    }

    public async Task SendAuditLog(SocketGuild guild, AuditEventEnum auditEvent, FormatLogOptions options)
    {
        await SendAuditLog(guild.Id, null, auditEvent, options);
    }
}