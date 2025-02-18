using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Liana.Database;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Liana.Bot.Events;

/// <summary>
/// Handle member joins and reaction/button roles
/// </summary>
/// <param name="serviceProvider"></param>
public class RoleEvents(IServiceProvider serviceProvider)
{
    public Task OnMemberJoined(SocketGuildUser socketUser)
    {
        Task.Run(async () =>
        {
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var config = await db.GetConfig(socketUser.Guild.Id);
            var guild = socketUser.Guild;

            if (config.Autorole == null || !guild.CurrentUser.GuildPermissions.ManageRoles) return;

            var maxPosition = guild.CurrentUser.Roles.Max(r => r.Position);

            if (config.Autorole.BotRoles != null || config.Autorole.UserRoles != null)
            {
                switch (socketUser.IsBot)
                {
                    case true when config.Autorole.BotRoles == null:
                    case false when config.Autorole.UserRoles == null:
                        break;
                }

                var roles = new List<ulong>();

                foreach (var roleId in (socketUser.IsBot ? config.Autorole.BotRoles : config.Autorole.UserRoles)!)
                {
                    var role = guild.GetRole(roleId);
                    if (role == null || role.Position >= maxPosition) continue;
                    roles.Add(role.Id);
                }

                await socketUser.AddRolesAsync(roles, new RequestOptions
                {
                    AuditLogReason = "[Autorole]"
                });
            }

            if (config.Autorole.PersistentRoles != null)
            {
                var roles = new List<ulong>();
                foreach (var roleId in config.Autorole.PersistentRoles)
                {
                    var role = guild.GetRole(roleId);
                    if (role == null || role.Position >= maxPosition) continue;
                    roles.Add(role.Id);
                }

                await socketUser.AddRolesAsync(roles, new RequestOptions
                {
                    AuditLogReason = "[Autorole/Persistent]"
                });
            }
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while giving role on join");
        });
        return Task.CompletedTask;
    }

    public Task OnMemberUpdated(Cacheable<SocketGuildUser, ulong> cachedMember, SocketGuildUser updatedMember)
    {
        Task.Run(async () =>
        {
            var db = serviceProvider.GetRequiredService<DatabaseContext>();
            var config = await db.GetConfig(updatedMember.Guild.Id);
            if (config.Autorole?.PersistentRoles == null || config.Autorole.PersistentRoles.Count == 0) return;

            var oldMember = cachedMember.Value;

            var removePersistent = oldMember.Roles.Except(updatedMember.Roles).Where(r => config.Autorole.PersistentRoles.Contains(r.Id)).Select(r => r.Id).ToList();
            var addPersistent = updatedMember.Roles.Except(oldMember.Roles).Where(r => config.Autorole.PersistentRoles.Contains(r.Id)).Select(r => r.Id).ToList();
            if (removePersistent.Count == 0 && addPersistent.Count == 0) return;

            var member = await db.GetMember(updatedMember.Guild.Id, updatedMember.Id);
            member.PersistentRoles = member.PersistentRoles.Where(r => !removePersistent.Contains(r)).ToList();
            member.PersistentRoles.AddRange(addPersistent);

            db.Update(member);
            await db.SaveChangesAsync();
        }).ContinueWith(t =>
        {
            if (t.Exception != null) Log.Error(t.Exception, "Error while persisting roles");
        });
        return Task.CompletedTask;
    }
}