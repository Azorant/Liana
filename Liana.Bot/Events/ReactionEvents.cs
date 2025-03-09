using Discord;
using Discord.WebSocket;
using Liana.Database;
using Liana.Models;
using Liana.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Liana.Bot.Events;

public class ReactionEvents(IServiceProvider serviceProvider)
{
    private readonly DiscordSocketClient client = serviceProvider.GetRequiredService<DiscordSocketClient>();

    public Task OnReactionAdded(Cacheable<IUserMessage, ulong> cacheableMessage, Cacheable<IMessageChannel, ulong> cacheableChannel, SocketReaction reaction)
    {
        Task.Run(async () =>
        {
            if (reaction.UserId == client.CurrentUser.Id) return;
            if (await cacheableChannel.GetOrDownloadAsync() is not SocketTextChannel channel) return;
            var guild = channel.Guild;
            var currentUser = guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageRoles) return;

            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var config = await db.GetConfig(channel.Guild.Id);
            var emote = Parser.SerializeEmote(reaction.Emote);

            if (config.ReactionRoles == null) return;
            if (!config.ReactionRoles.TryGetValue(channel.Id, out var messages)) return;
            if (!messages.TryGetValue(cacheableMessage.Id, out var emotes)) return;
            if (!emotes.TryGetValue(emote, out var roleConfigs)) return;

            var maxPosition = currentUser.Roles.Max(r => r.Position);
            var user = guild.GetUser(reaction.UserId);
            if (user == null) return;
            var userRoles = user.Roles.Select(r => r.Id).ToList();

            var rolesToGive = new List<ulong>();
            var rolesToRemove = new List<ulong>();

            foreach (var roleConfig in roleConfigs.Where(c => c.Reaction.HasFlag(ReactionRoleEnum.Add)))
            {
                var role = guild.GetRole(roleConfig.Id);
                if (role == null || role.Position >= maxPosition) continue;
                var add = roleConfig.Action.HasFlag(ReactionRoleEnum.Add);

                if (roleConfig.Prerequisite != null && roleConfig.Prerequisite.Any(pre => add ? !userRoles.Contains(pre) : userRoles.Contains(pre))) continue;
                if (roleConfig.Exclusive != null && roleConfig.Exclusive.Any(pre => add ? userRoles.Contains(pre) : !userRoles.Contains(pre))) continue;
                if (add ? userRoles.Contains(roleConfig.Id) : !userRoles.Contains(roleConfig.Id)) continue;
                if (add)
                {
                    rolesToGive.Add(roleConfig.Id);
                }
                else
                {
                    rolesToRemove.Add(roleConfig.Id);
                }
            }

            if (rolesToGive.Count != 0) await user.AddRolesAsync(rolesToGive, new RequestOptions { AuditLogReason = $"[Reaction Role]" });
            if (rolesToRemove.Count != 0) await user.RemoveRolesAsync(rolesToRemove, new RequestOptions { AuditLogReason = $"[Reaction Role]" });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while handling reaction add");
        });
        return Task.CompletedTask;
    }

    public Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cacheableMessage, Cacheable<IMessageChannel, ulong> cacheableChannel, SocketReaction reaction)
    {
        Task.Run(async () =>
        {
            if (reaction.UserId == client.CurrentUser.Id) return;
            if (await cacheableChannel.GetOrDownloadAsync() is not SocketTextChannel channel) return;
            var guild = channel.Guild;
            var currentUser = guild.CurrentUser;
            if (!currentUser.GuildPermissions.ManageRoles) return;

            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var config = await db.GetConfig(channel.Guild.Id);
            var emote = Parser.SerializeEmote(reaction.Emote);

            if (config.ReactionRoles == null) return;
            if (!config.ReactionRoles.TryGetValue(channel.Id, out var messages)) return;
            if (!messages.TryGetValue(cacheableMessage.Id, out var emotes)) return;
            if (!emotes.TryGetValue(emote, out var roleConfigs)) return;

            var maxPosition = currentUser.Roles.Max(r => r.Position);
            var user = guild.GetUser(reaction.UserId);
            if (user == null) return;
            var userRoles = user.Roles.Select(r => r.Id).ToList();

            var rolesToGive = new List<ulong>();
            var rolesToRemove = new List<ulong>();

            foreach (var roleConfig in roleConfigs.Where(c => c.Reaction.HasFlag(ReactionRoleEnum.Remove)))
            {
                var role = guild.GetRole(roleConfig.Id);
                if (role == null || role.Position >= maxPosition) continue;
                var add = roleConfig.Action.HasFlag(ReactionRoleEnum.Add);

                if (roleConfig.Prerequisite != null && roleConfig.Prerequisite.Any(pre => add ? !userRoles.Contains(pre) : userRoles.Contains(pre))) continue;
                if (roleConfig.Exclusive != null && roleConfig.Exclusive.Any(pre => add ? userRoles.Contains(pre) : !userRoles.Contains(pre))) continue;
                if (add ? !userRoles.Contains(roleConfig.Id) : userRoles.Contains(roleConfig.Id)) continue;
                if (add)
                {
                    rolesToRemove.Add(roleConfig.Id);
                }
                else
                {
                    rolesToGive.Add(roleConfig.Id);
                }
            }

            if (rolesToGive.Count != 0) await user.AddRolesAsync(rolesToGive, new RequestOptions { AuditLogReason = $"[Reaction Role]" });
            if (rolesToRemove.Count != 0) await user.RemoveRolesAsync(rolesToRemove, new RequestOptions { AuditLogReason = $"[Reaction Role]" });
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while handling reaction remove");
        });
        return Task.CompletedTask;
    }
}