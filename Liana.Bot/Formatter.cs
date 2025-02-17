using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Liana.Database.Entities;

namespace Liana.Bot;

public static class Formatter
{
    public static string FormatLog(string message, FormatLogOptions options)
    {
        message = message.ReplaceRegex("{timestamp}", TimestampTag.FormatFromDateTime(DateTime.UtcNow, TimestampTagStyles.ShortDateTime));
        if (options.Guild != null)
        {
            message = message
                .ReplaceRegex("{guild.id}", options.Guild.Id.ToString())
                .ReplaceRegex("{guild.icon}", options.Guild.IconUrl)
                .ReplaceRegex("{guild.name}", options.Guild.Name)
                .ReplaceRegex("{guild.channels}", options.Guild.Channels.Count.ToString("N0"))
                .ReplaceRegex("{guild.channels.text}", options.Guild.TextChannels.Count.ToString("N0"))
                .ReplaceRegex("{guild.roles}", options.Guild.Roles.Count.ToString("N0"))
                .ReplaceRegex("{guild.members}", options.Guild.Users.Count.ToString())
                .ReplaceRegex("{guild.members.formatted}", options.Guild.Users.Count.ToString("N0"))
                .ReplaceRegex("{guild.users}", options.Guild.Users.Count(u => !u.IsBot).ToString())
                .ReplaceRegex("{guild.users.formatted}", options.Guild.Users.Count(u => !u.IsBot).ToString("N0"))
                .ReplaceRegex("{guild.bots}", options.Guild.Users.Count(u => u.IsBot).ToString())
                .ReplaceRegex("{guild.bots.formatted}", options.Guild.Users.Count(u => u.IsBot).ToString("N0"));
        }

        if (options.User != null)
        {
            message = message
                .ReplaceRegex("{user.id}", options.User.Id.ToString())
                .ReplaceRegex("{user.username}", options.User.Username)
                .ReplaceRegex("{user.discriminator}", options.User.Discriminator)
                .ReplaceRegex("{user.tag}", Format.UsernameAndDiscriminator(options.User, false))
                .ReplaceRegex("{user.avatar}", options.User.GetAvatarUrl())
                .ReplaceRegex("{user.mention}", MentionUtils.MentionUser(options.User.Id))
                .ReplaceRegex("{user.age}", DateTimeOffset.UtcNow.Subtract(options.User.CreatedAt).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second));
        }

        if (options.User2 != null)
        {
            message = message
                .ReplaceRegex("{user2.id}", options.User2.Id.ToString())
                .ReplaceRegex("{user2.username}", options.User2.Username)
                .ReplaceRegex("{user2.discriminator}", options.User2.Discriminator)
                .ReplaceRegex("{user2.tag}", Format.UsernameAndDiscriminator(options.User2, false))
                .ReplaceRegex("{user2.avatar}", options.User2.GetAvatarUrl())
                .ReplaceRegex("{user2.mention}", MentionUtils.MentionUser(options.User2.Id))
                .ReplaceRegex("{user2.age}", DateTimeOffset.UtcNow.Subtract(options.User2.CreatedAt).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second));
        }

        if (options.Member != null)
        {
            message = message
                .ReplaceRegex("{member.id}", options.Member.Id.ToString())
                .ReplaceRegex("{member.username}", options.Member.Username)
                .ReplaceRegex("{member.discriminator}", options.Member.Discriminator)
                .ReplaceRegex("{member.tag}", Format.UsernameAndDiscriminator(options.Member, false))
                .ReplaceRegex("{member.avatar}", options.Member.GetAvatarUrl())
                .ReplaceRegex("{member.nick}", options.Member.Nickname ?? Format.UsernameAndDiscriminator(options.Member, false))
                .ReplaceRegex("{member.mention}", MentionUtils.MentionUser(options.Member.Id))
                .ReplaceRegex("{member.age}", DateTimeOffset.UtcNow.Subtract(options.Member.CreatedAt).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second))
                .ReplaceRegex("{member.joined}", DateTimeOffset.UtcNow.Subtract(options.Member.JoinedAt.GetValueOrDefault()).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second));
        }

        if (options.Member2 != null)
        {
            message = message
                .ReplaceRegex("{member2.id}", options.Member2.Id.ToString())
                .ReplaceRegex("{member2.username}", options.Member2.Username)
                .ReplaceRegex("{member2.discriminator}", options.Member2.Discriminator)
                .ReplaceRegex("{member2.tag}", Format.UsernameAndDiscriminator(options.Member2, false))
                .ReplaceRegex("{member2.avatar}", options.Member2.GetAvatarUrl())
                .ReplaceRegex("{member2.nick}", options.Member2.Nickname ?? Format.UsernameAndDiscriminator(options.Member2, false))
                .ReplaceRegex("{member2.mention}", MentionUtils.MentionUser(options.Member2.Id))
                .ReplaceRegex("{member2.age}", DateTimeOffset.UtcNow.Subtract(options.Member2.CreatedAt).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second))
                .ReplaceRegex("{member2.joined}", DateTimeOffset.UtcNow.Subtract(options.Member2.JoinedAt.GetValueOrDefault()).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second));
        }

        if (options.Moderator != null)
        {
            message = message
                .ReplaceRegex("{moderator.id}", options.Moderator.Id.ToString())
                .ReplaceRegex("{moderator.username}", options.Moderator.Username)
                .ReplaceRegex("{moderator.discriminator}", options.Moderator.Discriminator)
                .ReplaceRegex("{moderator.tag}", Format.UsernameAndDiscriminator(options.Moderator, false))
                .ReplaceRegex("{moderator.avatar}", options.Moderator.GetAvatarUrl())
                .ReplaceRegex("{moderator.nick}", options.Moderator.Nickname ?? Format.UsernameAndDiscriminator(options.Moderator, false))
                .ReplaceRegex("{moderator.mention}", MentionUtils.MentionUser(options.Moderator.Id))
                .ReplaceRegex("{moderator.age}", DateTimeOffset.UtcNow.Subtract(options.Moderator.CreatedAt).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second))
                .ReplaceRegex("{moderator.joined}", DateTimeOffset.UtcNow.Subtract(options.Moderator.JoinedAt.GetValueOrDefault()).Humanize(2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second));
        }

        if (options.Channel != null)
        {
            message = message
                .ReplaceRegex("{channel.id}", options.Channel.Id.ToString())
                .ReplaceRegex("{channel.name}", options.Channel.Name)
                .ReplaceRegex("{channel.mention}", MentionUtils.MentionChannel(options.Channel.Id));
        }

        if (options.Channel2 != null)
        {
            message = message
                .ReplaceRegex("{channel2.id}", options.Channel2.Id.ToString())
                .ReplaceRegex("{channel2.name}", options.Channel2.Name)
                .ReplaceRegex("{channel2.mention}", MentionUtils.MentionChannel(options.Channel2.Id));
        }

        if (options.Message != null)
        {
            var origContent = "";
            if (options.Message.Content is { Length: > 0 })
            {
                origContent =
                    $"{Format.Sanitize(options.Message.Content)}{(options.Message.Attachments is { Count: > 0 } ? $"\n\nUploads:\n{string.Join("\n", options.Message.Attachments)}" : string.Empty)}";
            }
            else if (options.Message.Attachments is { Count: > 0 })
            {
                origContent = options.Message.Attachments.Count > 0 ? $"Uploads:\n{string.Join("\n", options.Message.Attachments)}" : string.Empty;
            }

            var editedContent = "";
            if (options.Message.EditedContent is { Length: > 0 })
            {
                editedContent =
                    $"{Format.Sanitize(options.Message.EditedContent)}{(options.Message.Attachments is { Count: > 0 } ? $"\n\nUploads:\n{string.Join("\n", options.Message.Attachments)}" : string.Empty)}";
            }
            else if (options.Message.Attachments is { Count: > 0 })
            {
                editedContent = options.Message.Attachments.Count > 0 ? $"Uploads:\n{string.Join("\n", options.Message.Attachments)}" : string.Empty;
            }

            if (string.IsNullOrEmpty(origContent)) origContent = "No content";
            if (string.IsNullOrEmpty(editedContent)) editedContent = "No content";

            message = message
                .ReplaceRegex("{message.id}", options.Message.Id.ToString())
                .ReplaceRegex("{message.channel.id}", options.Message.ChannelId.ToString())
                .ReplaceRegex("{message.channel.mention}", MentionUtils.MentionChannel(options.Message.ChannelId))
                .ReplaceRegex("{message.content}", Format.Code(origContent, string.Empty))
                .ReplaceRegex("{message.edited}", Format.Code(editedContent, string.Empty))
                .ReplaceRegex("{message.link}", $"https://discord.com/channels/{options.Message.GuildId}/{options.Message.ChannelId}/{options.Message.Id}");
        }
        
        if (options.Role != null)
        {
            message = message
                .ReplaceRegex("{role.id}", options.Role.Id.ToString())
                .ReplaceRegex("{role.name}", options.Role.Name)
                .ReplaceRegex("{role.mention}", MentionUtils.MentionRole(options.Role.Id));
        }

        if (options.Role2 != null)
        {
            message = message
                .ReplaceRegex("{role2.id}", options.Role2.Id.ToString())
                .ReplaceRegex("{role2.name}", options.Role2.Name)
                .ReplaceRegex("{role2.mention}", MentionUtils.MentionRole(options.Role2.Id));
        }

        if (options.Reason != null) message = message.ReplaceRegex("{reason}", options.Reason);

        return message;
    }
}

public class FormatLogOptions
{
    public SocketGuildChannel? Channel { get; set; }
    public SocketGuildChannel? Channel2 { get; set; }
    public SocketGuild? Guild { get; set; }
    public MessageEntity? Message { get; set; }
    public SocketGuildUser? Member { get; set; }
    public SocketGuildUser? Member2 { get; set; }
    public SocketUser? User { get; set; }
    public SocketUser? User2 { get; set; }
    public SocketGuildUser? Moderator { get; set; }
    public string? Reason { get; set; }
    public SocketRole? Role { get; set; }
    public SocketRole? Role2 { get; set; }
}