using Discord;
using Discord.Interactions;
using Liana.Database;
using Liana.Database.Entities;
using Liana.Models;
using Liana.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Liana.Bot.Modules;

[CommandContextType(InteractionContextType.Guild), IntegrationType(ApplicationIntegrationType.GuildInstall)]
public class BaseModule(DatabaseContext db) : InteractionModuleBase<SocketInteractionContext>
{
    protected Task<GuildConfig> GetConfigAsync() => db.GetConfig(Context.Guild.Id);
    protected Task<string> GetRawConfigAsync() => db.GetRawConfig(Context.Guild.Id);

    /// <summary>
    /// Check if user has config role
    /// </summary>
    /// <param name="config"></param>
    /// <param name="permission"></param>
    /// <returns></returns>
    protected bool AssertConfigRole(GuildConfig config, RoleEnum permission)
    {
        if (Context.Guild.OwnerId == Context.User.Id) return true;
        if (config.Roles == null) return false;
        var userRoles = Context.Guild.GetUser(Context.User.Id).Roles.Select(r => r.Id).ToList();
        return config.Roles.Where(r => userRoles.Contains(r.Key)).Any(role => role.Value.HasFlag(permission));
    }

    protected async Task SendErrorAsync(string error, string title = "Error")
    {
        var embed = new EmbedBuilder()
            .WithTitle($":warning: {title}")
            .WithDescription(error)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithColor(Color.Gold)
            .Build();
        if (Context.Interaction.HasResponded)
        {
            await FollowupAsync(embed: embed);
        }
        else
        {
            await RespondAsync(embed: embed);
        }
    }

    protected async Task SendSuccessAsync(string description, bool modify = false)
    {
        var embed = new EmbedBuilder()
            .WithTitle(":white_check_mark: Success")
            .WithDescription(description)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithColor(Color.Green)
            .Build();
        if (Context.Interaction.HasResponded)
        {
            if (modify)
                await ModifyOriginalResponseAsync(m => m.Embed = embed);
            else
                await FollowupAsync(embed: embed);
        }
        else
        {
            await RespondAsync(embed: embed);
        }
    }

    protected Task SendPermissionErrorAsync(string text) => SendErrorAsync(text, "Missing Permission");
}