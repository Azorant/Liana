﻿using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Liana.Bot.HostedServices;

namespace Liana.Bot.Modules;

public class MiscModule() : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("about", "Information about the bot")]
    public async Task AboutCommand()
    {
        var library = Assembly.GetAssembly(typeof(InteractionModuleBase))!.GetName();
        var self = Context.Client.GetUser(160168328520794112);
        var embed = new EmbedBuilder()
            .WithAuthor(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
            .AddField("Guilds", Context.Client.Guilds.Count.ToString("N0"), true)
            .AddField("Users", Context.Client.Guilds.Select(guild => guild.MemberCount).Sum().ToString("N0"), true)
            .AddField("Library", $"Discord.Net {library.Version!.ToString()}", true)
            .AddField("Developer", $"{Format.UsernameAndDiscriminator(self, false)}", true)
            .AddField("Links",
                $"[GitHub](https://github.com/Azorant)\n[Support](https://discord.gg/{Environment.GetEnvironmentVariable("DISCORD_INVITE")})\n[Ko-fi](https://ko-fi.com/azorant)",
                true)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(Format.UsernameAndDiscriminator(Context.User, false), Context.User.GetAvatarUrl())
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("invite", "Invite the bot")]
    public async Task InviteCommand()
        => await RespondAsync(
            $"https://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot%20applications.commands");

    [SlashCommand("test", "test")]
    public async Task TestCommand()
    {
        await RespondAsync(new EmbedBuilder().WithTitle("test").WithImageUrl("https://cdn.carbon.pics/ranch.png").ToJsonString());
    }
}