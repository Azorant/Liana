using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Liana.Database;
using Liana.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Liana.Bot.Events;

public class MessagingEvents(IServiceProvider serviceProvider)
{
    public Task OnMemberJoined(SocketGuildUser socketUser)
    {
        Task.Run(async () =>
        {
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var config = await db.GetConfig(socketUser.Guild.Id);
            var guild = socketUser.Guild;

            if (config.Messaging == null) return;

            foreach (var message in config.Messaging.Where(x => x.Type == MessagingEnum.Join))
            {
                var content = string.IsNullOrEmpty(message.Content) ? null : Formatter.FormatLog(message.Content, new FormatLogOptions { Guild = guild, Member = socketUser });
                var embeds = message.Embeds.Select(x => EmbedBuilderUtils.Parse(Formatter.FormatLog(x, new FormatLogOptions { Guild = guild, Member = socketUser })).Build())
                    .ToArray();
                if (message.ChannelId.HasValue)
                {
                    var channel = guild.GetChannel(message.ChannelId.Value);
                    if (channel is not SocketTextChannel textChannel) continue;
                    var permissions = guild.CurrentUser.GetPermissions(textChannel);
                    if (!permissions.ViewChannel || !permissions.SendMessages || !permissions.EmbedLinks) continue;

                    await textChannel.SendMessageAsync(content, embeds: embeds, allowedMentions: AllowedMentions.All);
                }
                else
                {
                    try
                    {
                        await socketUser.SendMessageAsync(content, embeds: embeds, components: new ComponentBuilder()
                            .WithButton($"Sent from {guild.Name}", style: ButtonStyle.Link, url: $"https://discord.com/channels/{guild.Id}")
                            .Build());
                    }
                    catch (HttpException ex)
                    {
                        if (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                        {
                            Log.Warning(ex, "Error sending message to user");
                        }
                        else Log.Error(ex, "Error sending message to user");
                    }
                }
            }
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while sending join messages");
        });
        return Task.CompletedTask;
    }

    public Task OnMemberLeft(SocketGuild socketGuild, SocketUser socketUser)
    {
        Task.Run(async () =>
        {
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var config = await db.GetConfig(socketGuild.Id);

            if (config.Messaging == null) return;

            foreach (var message in config.Messaging.Where(x => x.Type == MessagingEnum.Leave))
            {
                if (!message.ChannelId.HasValue) continue;
                var channel = socketGuild.GetChannel(message.ChannelId.Value);

                if (channel is not SocketTextChannel textChannel) continue;
                var permissions = socketGuild.CurrentUser.GetPermissions(textChannel);
                if (!permissions.ViewChannel || !permissions.SendMessages || !permissions.EmbedLinks) continue;
                
                var content = string.IsNullOrEmpty(message.Content) ? null : Formatter.FormatLog(message.Content, new FormatLogOptions { Guild = socketGuild, User = socketUser });
                var embeds = message.Embeds.Select(x => EmbedBuilderUtils.Parse(Formatter.FormatLog(x, new FormatLogOptions { Guild = socketGuild, User = socketUser })).Build())
                    .ToArray();
                await textChannel.SendMessageAsync(content, embeds: embeds, allowedMentions: AllowedMentions.All);
            }
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while sending leave messages");
        });
        return Task.CompletedTask;
    }
}