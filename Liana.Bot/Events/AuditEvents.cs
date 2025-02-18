using Discord;
using Discord.WebSocket;
using Liana.Bot.Services;
using Liana.Database;
using Liana.Database.Entities;
using Liana.Models;
using Liana.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Liana.Bot.Events;

public class AuditEvents(IServiceProvider serviceProvider)
{
    private readonly DiscordSocketClient client = serviceProvider.GetRequiredService<DiscordSocketClient>();

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
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
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
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(channel.Guild, channel, AuditEventEnum.ChannelCreate, new FormatLogOptions
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
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(oldChannel.Guild, oldChannel, AuditEventEnum.ChannelUpdate, new FormatLogOptions
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
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(channel.Guild, channel, AuditEventEnum.ChannelDelete, new FormatLogOptions
            {
                Channel = channel,
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging channel deleted");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageCreated(SocketMessage message)
    {
        Task.Run(async () =>
        {
            if (message.Channel is not SocketGuildChannel channel || message.Author.Id == client.CurrentUser.Id) return;
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            await db.AddAsync(new MessageEntity
            {
                Id = message.Id,
                GuildId = channel.Guild.Id,
                ChannelId = channel.Id,
                AuthorId = message.Author.Id,
                AuthorTag = Format.UsernameAndDiscriminator(message.Author, false),
                Content = string.IsNullOrEmpty(message.Content) ? null : message.Content,
                Attachments = message.Attachments?.Select(a => a.Url).ToList() ?? []
            });
            await db.SaveChangesAsync();
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while saving message");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage socketMessage, ISocketMessageChannel _)
    {
        Task.Run(async () =>
        {
            if (socketMessage.Channel is not SocketGuildChannel channel) return;
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == socketMessage.Id);
            if (message == null)
            {
                message = new MessageEntity
                {
                    Id = socketMessage.Id,
                    GuildId = channel.Guild.Id,
                    ChannelId = channel.Id,
                    AuthorId = socketMessage.Author.Id,
                    AuthorTag = Format.UsernameAndDiscriminator(socketMessage.Author, false),
                    Content = string.IsNullOrEmpty(socketMessage.Content) ? null : socketMessage.Content,
                    Attachments = socketMessage.Attachments?.Select(a => a.Url).ToList() ?? []
                };
                await db.AddAsync(message);
                await db.SaveChangesAsync();
                // Message wasn't cached, so we don't have the original content to send log for
                return;
            }

            message.EditedContent = string.IsNullOrEmpty(socketMessage.Content) ? null : socketMessage.Content;
            db.Update(message);
            await db.SaveChangesAsync();

            // Probably interaction message
            if (message.Content == null && message.EditedContent == null && message.Attachments.Count == 0) return;

            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(channel.Guild, channel, AuditEventEnum.MessageUpdate, new FormatLogOptions
            {
                Channel = channel,
                Message = message,
                Member = channel.Guild.GetUser(message.AuthorId)
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging message update");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageDeleted(Cacheable<IMessage, ulong> cacheableMessage, Cacheable<IMessageChannel, ulong> cacheableChannel)
    {
        Task.Run(async () =>
        {
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == cacheableMessage.Id);
            // Message wasn't cached, and we can't really cache it now because it's deleted
            if (message == null) return;
            message.Deleted = true;
            db.Update(message);
            await db.SaveChangesAsync();

            var channel = await cacheableChannel.GetOrDownloadAsync() as SocketGuildChannel;

            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(message.GuildId, message.ChannelId, AuditEventEnum.MessageDelete,
                new FormatLogOptions
                {
                    Channel = channel,
                    Message = message,
                    Member = channel?.Guild.GetUser(message.AuthorId)
                });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging message delete");
        });
        return Task.CompletedTask;
    }

    public Task OnMessageBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cacheableMessages, Cacheable<IMessageChannel, ulong> cacheableChannel)
    {
        Task.Run(async () =>
        {
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var messages = await db.Messages.Where(m => cacheableMessages.Select(c => c.Id).Contains(m.Id)).ToListAsync();
            var channel = await cacheableChannel.GetOrDownloadAsync() as SocketGuildChannel;
            var member = channel?.Guild.GetUser(messages.First().AuthorId);
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            foreach (var message in messages)
            {
                message.Deleted = true;
                db.Update(message);
                await auditLogService.SendAuditLog(message.GuildId, message.ChannelId, AuditEventEnum.MessageDelete,
                    new FormatLogOptions
                    {
                        Channel = channel,
                        Message = message,
                        Member = member
                    });
            }

            await db.SaveChangesAsync();
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging message bulk delete");
        });
        return Task.CompletedTask;
    }

    public Task OnVoiceStateUpdated(SocketUser socketUser, SocketVoiceState oldState, SocketVoiceState newState)
    {
        Task.Run(async () =>
        {
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            if (oldState.VoiceChannel == null && newState.VoiceChannel != null)
            {
                await auditLogService.SendAuditLog(newState.VoiceChannel.Guild, newState.VoiceChannel, AuditEventEnum.VoiceChannelJoin,
                    new FormatLogOptions
                    {
                        Channel = newState.VoiceChannel,
                        Member = newState.VoiceChannel.Guild.GetUser(socketUser.Id)
                    });
            }
            else if (oldState.VoiceChannel != null && newState.VoiceChannel == null)
            {
                await auditLogService.SendAuditLog(oldState.VoiceChannel.Guild, oldState.VoiceChannel, AuditEventEnum.VoiceChannelLeave,
                    new FormatLogOptions
                    {
                        Channel = oldState.VoiceChannel,
                        Member = oldState.VoiceChannel.Guild.GetUser(socketUser.Id)
                    });
            }
            else if (oldState.VoiceChannel != null && newState.VoiceChannel != null && oldState.VoiceChannel.Id != newState.VoiceChannel.Id)
            {
                await auditLogService.SendAuditLog(oldState.VoiceChannel.Guild, oldState.VoiceChannel, AuditEventEnum.VoiceChannelSwitch,
                    new FormatLogOptions
                    {
                        Channel = oldState.VoiceChannel,
                        Channel2 = newState.VoiceChannel,
                        Member = newState.VoiceChannel.Guild.GetUser(socketUser.Id)
                    });
            }
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging voice state update");
        });
        return Task.CompletedTask;
    }

    public Task OnMemberJoined(SocketGuildUser socketUser)
    {
        Task.Run(async () =>
        {
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(socketUser.Guild, AuditEventEnum.MemberAdd, new FormatLogOptions
            {
                Guild = socketUser.Guild,
                Member = socketUser
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging member join");
        });
        return Task.CompletedTask;
    }

    public Task OnMemberLeft(SocketGuild socketGuild, SocketUser socketUser)
    {
        Task.Run(async () =>
        {
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(socketGuild, AuditEventEnum.MemberRemove, new FormatLogOptions
            {
                Guild = socketGuild,
                User = socketUser
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging member leave");
        });
        return Task.CompletedTask;
    }

    public Task OnMemberUpdated(Cacheable<SocketGuildUser, ulong> cachedMember, SocketGuildUser updatedMember)
    {
        Task.Run(async () =>
        {
            var oldMember = cachedMember.Value;

            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            if (oldMember.Nickname == null && updatedMember.Nickname != null)
            {
                await auditLogService.SendAuditLog(updatedMember.Guild, AuditEventEnum.NicknameAdd, new FormatLogOptions
                {
                    Guild = updatedMember.Guild,
                    Member = updatedMember
                });
            }
            else if (updatedMember.Nickname == null && oldMember.Nickname != null)
            {
                await auditLogService.SendAuditLog(updatedMember.Guild, AuditEventEnum.NicknameRemove, new FormatLogOptions
                {
                    Guild = updatedMember.Guild,
                    Member = oldMember
                });
            }
            else if (updatedMember.Nickname != oldMember.Nickname)
            {
                await auditLogService.SendAuditLog(updatedMember.Guild, AuditEventEnum.NicknameUpdate, new FormatLogOptions
                {
                    Guild = updatedMember.Guild,
                    Member = oldMember,
                    Member2 = updatedMember
                });
            }

            var removedRoles = oldMember.Roles.Except(updatedMember.Roles).ToList();
            var addedRoles = updatedMember.Roles.Except(oldMember.Roles).ToList();

            foreach (var role in removedRoles)
            {
                await auditLogService.SendAuditLog(updatedMember.Guild, AuditEventEnum.MemberRoleRemove, new FormatLogOptions
                {
                    Guild = updatedMember.Guild,
                    Member = updatedMember,
                    Role = role
                });
            }

            foreach (var role in addedRoles)
            {
                await auditLogService.SendAuditLog(updatedMember.Guild, AuditEventEnum.MemberRoleAdd, new FormatLogOptions
                {
                    Guild = updatedMember.Guild,
                    Member = updatedMember,
                    Role = role
                });
            }
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging member update");
        });
        return Task.CompletedTask;
    }

    public Task OnRoleCreated(SocketRole role)
    {
        Task.Run(async () =>
        {
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(role.Guild, AuditEventEnum.RoleCreate, new FormatLogOptions
            {
                Guild = role.Guild,
                Role = role
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging role create");
        });
        return Task.CompletedTask;
    }

    public Task OnRoleUpdated(SocketRole oldRole, SocketRole newRole)
    {
        Task.Run(async () =>
        {
            if (oldRole.Name == newRole.Name) return; // Maybe log other updates in future
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(newRole.Guild, AuditEventEnum.RoleUpdate, new FormatLogOptions
            {
                Guild = newRole.Guild,
                Role = oldRole,
                Role2 = newRole
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging role update");
        });
        return Task.CompletedTask;
    }

    public Task OnRoleDeleted(SocketRole role)
    {
        Task.Run(async () =>
        {
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
            await auditLogService.SendAuditLog(role.Guild, AuditEventEnum.RoleDelete, new FormatLogOptions
            {
                Guild = role.Guild,
                Role = role
            });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging role remove");
        });
        return Task.CompletedTask;
    }

    public Task OnUserUpdated(SocketUser oldUser, SocketUser newUser)
    {
        Task.Run(async () =>
        {
            if (Format.UsernameAndDiscriminator(oldUser, false) == Format.UsernameAndDiscriminator(newUser, false)) return;

            var guilds = client.Guilds.Where(g => g.Users.Select(u => u.Id).Contains(newUser.Id));
            foreach (var guild in guilds)
            {
                var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();
                await auditLogService.SendAuditLog(guild, AuditEventEnum.UsernameChange, new FormatLogOptions
                {
                    User = oldUser,
                    User2 = newUser,
                });
            }
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while logging user updated");
        });
        return Task.CompletedTask;
    }
}