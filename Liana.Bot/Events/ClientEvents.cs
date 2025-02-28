using Discord;
using Discord.WebSocket;
using Liana.Database;
using Liana.Database.Entities;
using Liana.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Liana.Bot.Events;

public class ClientEvents(IServiceProvider serviceProvider)
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
                    Config = GuildConfig.DefaultConfig
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
}