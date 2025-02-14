using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace Liana.Bot;

public static class Formatter
{
    public static string FormatLog(string message, FormatLogOptions options)
    {
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

        // TODO: Rest of variables
        
        return message;
    }
}

public class FormatLogOptions
{
    public SocketGuildChannel? Channel { get; set; }
    public SocketGuildChannel? Channel2 { get; set; }
    public SocketGuild? Guild { get; set; }
    public SocketMessage? Message { get; set; }
    public SocketMessage? Message2 { get; set; }
    public SocketGuildUser? User { get; set; }
    public SocketGuildUser? User2 { get; set; }
    public SocketGuildUser? Moderator { get; set; }
    public string? Reason { get; set; }
    public SocketRole? Role { get; set; }
}