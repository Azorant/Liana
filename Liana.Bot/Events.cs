using Discord;
using Discord.WebSocket;
using Liana.Bot.Services;
using Liana.Database;
using Liana.Database.Entities;
using Liana.Models;
using Liana.Models.Constants;
using Liana.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Liana.Bot;

public class Events(DiscordSocketClient client, DatabaseContext db, AuditLogService auditLogService)
{
    public Task OnGuildJoined(SocketGuild guild)
    {
        Task.Run(async () =>
        {
            if (!ulong.TryParse(Environment.GetEnvironmentVariable("GUILD_CHANNEL"), out var channelId)) return;
            if (client.GetChannel(channelId) is not SocketTextChannel channel || channel.GetChannelType() != ChannelType.Text) return;

            var user = await client.GetUserAsync(guild.OwnerId);
            var owner = user == null
                ? $"**Owner ID:** {guild.OwnerId}"
                : $"**Owner:** {Format.UsernameAndDiscriminator(user, false)}\n**Owner ID:** {user.Id}";

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("Joined guild")
                .WithDescription($"**Name:** {guild.Name}\n**ID:** {guild.Id}\n{owner}\n**Members:** {guild.MemberCount}\n**Created:** {guild.CreatedAt:f}")
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(guild.IconUrl).Build());
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging guild join");
        });
        return Task.CompletedTask;
    }

    public Task OnGuildLeft(SocketGuild guild)
    {
        Task.Run(async () =>
        {
            if (!ulong.TryParse(Environment.GetEnvironmentVariable("GUILD_CHANNEL"), out var channelId)) return;
            if (client.GetChannel(channelId) is not SocketTextChannel channel || channel.GetChannelType() != ChannelType.Text) return;

            var user = await client.GetUserAsync(guild.OwnerId);
            var owner = user == null
                ? $"**Owner ID:** {guild.OwnerId}"
                : $"**Owner:** {Format.UsernameAndDiscriminator(user, false)}\n**Owner ID:** {user.Id}";

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("Left guild")
                .WithDescription($"**Name:** {guild.Name}\n**ID:** {guild.Id}\n{owner}\n**Members:** {guild.MemberCount}\n**Created:** {guild.CreatedAt:f}")
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(guild.IconUrl).Build());
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging guild leave");
        });
        return Task.CompletedTask;
    }

    public Task OnClientReady()
    {
        Task.Run(async () =>
        {
            if (!ulong.TryParse(Environment.GetEnvironmentVariable("LOG_CHANNEL"), out var channelId)) return;
            if (client.GetChannel(channelId) is not SocketTextChannel channel || channel.GetChannelType() != ChannelType.Text) return;

            var existingGuilds = await db.Guilds.Select(g => g.Id).ToListAsync();
            foreach (var guild in client.Guilds)
            {
                if (existingGuilds.Contains(guild.Id)) continue;
                await db.AddAsync(new GuildEntity
                {
                    Id = guild.Id,
                    Config = new GuildConfig()
                });
                Log.Information($"Creating config for guild {guild.Name} ({guild.Id})");
            }

            await db.SaveChangesAsync();

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("Client Ready")
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build());
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging client ready");
        });
        return Task.CompletedTask;
    }

    public Task OnClientDisconnected(Exception exception)
    {
        Task.Run(async () =>
        {
            if (!ulong.TryParse(Environment.GetEnvironmentVariable("LOG_CHANNEL"), out var channelId)) return;
            if (client.GetChannel(channelId) is not SocketTextChannel channel || channel.GetChannelType() != ChannelType.Text) return;

            await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithTitle("Client Disconnected")
                .WithDescription(exception.Message)
                .WithColor(Color.Gold)
                .WithCurrentTimestamp()
                .Build());
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging client disconnect");
        });
        return Task.CompletedTask;
    }

    public Task OnChannelCreated(SocketChannel socketChannel)
    {
        Task.Run(async () =>
        {
            if (socketChannel is not SocketGuildChannel channel) return;
            await auditLogService.SendAuditLog(channel.Guild.Id, channel.Id, AuditEventEnum.ChannelCreate, new FormatLogOptions
            {
                Channel = channel,
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging channel created");
        });
        return Task.CompletedTask;
    }

    public Task OnChannelUpdated(SocketChannel oldSocketChannel, SocketChannel newSocketChannel)
    {
        Task.Run(async () =>
        {
            if (oldSocketChannel is not SocketGuildChannel oldChannel || newSocketChannel is not SocketGuildChannel newChannel) return;
            await auditLogService.SendAuditLog(oldChannel.Guild.Id, oldChannel.Id, AuditEventEnum.ChannelUpdate, new FormatLogOptions
            {
                Channel = oldChannel,
                Channel2 = newChannel
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging channel updated");
        });
        return Task.CompletedTask;
    }

    public Task OnChannelDeleted(SocketChannel socketChannel)
    {
        Task.Run(async () =>
        {
            if (socketChannel is not SocketGuildChannel channel) return;
            await auditLogService.SendAuditLog(channel.Guild.Id, channel.Id, AuditEventEnum.ChannelDelete, new FormatLogOptions
            {
                Channel = channel,
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging channel deleted");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageCreate(SocketMessage message)
    {
        Task.Run(async () =>
        {
            if (message.Channel is not SocketGuildChannel channel || message.Author.Id == client.CurrentUser.Id) return;
            await db.AddAsync(new MessageEntity
            {
                Id = message.Id,
                GuildId = channel.Guild.Id,
                ChannelId = channel.Id,
                AuthorId = message.Author.Id,
                AuthorTag = Format.UsernameAndDiscriminator(message.Author, false),
                Content = string.IsNullOrEmpty(message.Content) ? null : message.Content,
                Attachments = message.Attachments?.Select(a => a.Url).ToList()
            });
            await db.SaveChangesAsync();
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while saving message");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageUpdate(Cacheable<IMessage, ulong> cacheable, SocketMessage socketMessage, ISocketMessageChannel _)
    {
        Task.Run(async () =>
        {
            if (socketMessage.Channel is not SocketGuildChannel channel) return;
            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == socketMessage.Id);
            if (message == null)
            {
                var oldMessage = await cacheable.GetOrDownloadAsync();
                message = new MessageEntity
                {
                    Id = socketMessage.Id,
                    GuildId = channel.Guild.Id,
                    ChannelId = channel.Id,
                    AuthorId = socketMessage.Author.Id,
                    AuthorTag = Format.UsernameAndDiscriminator(socketMessage.Author, false),
                    Content = string.IsNullOrEmpty(oldMessage.Content) ? null : oldMessage.Content,
                    EditedContent = string.IsNullOrEmpty(socketMessage.Content) ? null : socketMessage.Content,
                    Attachments = oldMessage.Attachments?.Select(a => a.Url).ToList()
                };
                await db.AddAsync(message);
            }
            else
            {
                message.EditedContent = string.IsNullOrEmpty(socketMessage.Content) ? null : socketMessage.Content;
                db.Update(message);
            }

            await db.SaveChangesAsync();

            await auditLogService.SendAuditLog(channel.Guild.Id, channel.Id, AuditEventEnum.MessageUpdate, new FormatLogOptions
            {
                Channel = channel,
                Message = message,
                User = socketMessage.Author
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging message update");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageDelete(Cacheable<IMessage, ulong> cacheableMessage, Cacheable<IMessageChannel, ulong> cacheableChannel)
    {
        Task.Run(async () =>
        {
            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == cacheableMessage.Id);
            if (message == null) return;
            message.Deleted = true;
            db.Update(message);
            await db.SaveChangesAsync();

            var channel = await cacheableChannel.GetOrDownloadAsync() as SocketGuildChannel;

            await auditLogService.SendAuditLog(message.GuildId, message.ChannelId, AuditEventEnum.MessageDelete,
                new FormatLogOptions
                {
                    Channel = channel,
                    Message = message,
                    User = channel?.Guild.GetUser(message.AuthorId)
                });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging message delete");
        });
        return Task.CompletedTask;
    }
}
