using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Liana.Bot.Events;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Liana.Bot.HostedServices;

internal sealed class DiscordClientHost : IHostedService
{
    private readonly DiscordSocketClient client;
    private readonly InteractionService interactionService;
    private readonly IServiceProvider serviceProvider;
    private readonly AuditEvents auditEvents;
    private readonly RoleEvents roleEvents;
    private readonly MessagingEvents messagingEvents;

    public DiscordClientHost(
        DiscordSocketClient client,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(interactionService);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.client = client;
        this.interactionService = interactionService;
        this.serviceProvider = serviceProvider;
        auditEvents = new AuditEvents(serviceProvider);
        roleEvents = new RoleEvents(serviceProvider);
        messagingEvents = new MessagingEvents(serviceProvider);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.InteractionCreated += InteractionCreated;
        client.Ready += ClientReady;
        client.Log += LogAsync;
        client.JoinedGuild += auditEvents.OnGuildJoined;
        client.LeftGuild += auditEvents.OnGuildLeft;
        client.Ready += auditEvents.OnClientReady;
        client.Disconnected += auditEvents.OnClientDisconnected;
        interactionService.Log += LogAsync;
        interactionService.SlashCommandExecuted += SlashCommandExecuted;

        #region audit log events
        client.ChannelCreated += auditEvents.OnChannelCreated;
        client.ChannelUpdated += auditEvents.OnChannelUpdated;
        client.ChannelDestroyed += auditEvents.OnChannelDeleted;
        client.MessageReceived += auditEvents.OnMessageCreated;
        client.MessageUpdated += auditEvents.OnMessageUpdated;
        client.MessageDeleted += auditEvents.OnMessageDeleted;
        client.MessagesBulkDeleted += auditEvents.OnMessageBulkDeleted;
        client.UserVoiceStateUpdated += auditEvents.OnVoiceStateUpdated;
        client.UserJoined += auditEvents.OnMemberJoined;
        client.UserLeft += auditEvents.OnMemberLeft;
        client.GuildMemberUpdated += auditEvents.OnMemberUpdated;
        client.RoleCreated += auditEvents.OnRoleCreated;
        client.RoleUpdated += auditEvents.OnRoleUpdated;
        client.RoleDeleted += auditEvents.OnRoleDeleted;
        client.UserUpdated += auditEvents.OnUserUpdated;
        #endregion
        
        #region guild role events
        client.UserJoined += roleEvents.OnMemberJoined;
        client.GuildMemberUpdated += roleEvents.OnMemberUpdated;
        #endregion
        
        #region messaging events
        client.UserJoined += messagingEvents.OnMemberJoined;
        client.UserLeft += messagingEvents.OnMemberLeft;
        #endregion

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN"));
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        client.InteractionCreated -= InteractionCreated;
        client.Ready -= ClientReady;
        client.Log -= LogAsync;
        client.JoinedGuild -= auditEvents.OnGuildJoined;
        client.LeftGuild -= auditEvents.OnGuildLeft;
        client.Ready -= auditEvents.OnClientReady;
        client.Disconnected -= auditEvents.OnClientDisconnected;
        interactionService.Log -= LogAsync;
        interactionService.SlashCommandExecuted -= SlashCommandExecuted;

        #region audit log events
        client.ChannelCreated -= auditEvents.OnChannelCreated;
        client.ChannelUpdated -= auditEvents.OnChannelUpdated;
        client.ChannelDestroyed -= auditEvents.OnChannelDeleted;
        client.MessageReceived -= auditEvents.OnMessageCreated;
        client.MessageDeleted -= auditEvents.OnMessageDeleted;
        client.MessagesBulkDeleted -= auditEvents.OnMessageBulkDeleted;
        client.UserVoiceStateUpdated -= auditEvents.OnVoiceStateUpdated;
        client.UserJoined -= auditEvents.OnMemberJoined;
        client.UserLeft -= auditEvents.OnMemberLeft;
        client.GuildMemberUpdated -= auditEvents.OnMemberUpdated;
        client.RoleCreated -= auditEvents.OnRoleCreated;
        client.RoleUpdated -= auditEvents.OnRoleUpdated;
        client.RoleDeleted -= auditEvents.OnRoleDeleted;
        client.UserUpdated -= auditEvents.OnUserUpdated;
        #endregion

        #region guild role events
        client.UserJoined -= roleEvents.OnMemberJoined;
        client.GuildMemberUpdated -= roleEvents.OnMemberUpdated;
        #endregion
        
        #region messaging events
        client.UserJoined -= messagingEvents.OnMemberJoined;
        client.UserLeft -= messagingEvents.OnMemberLeft;
        #endregion
        
        await client.StopAsync();
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);

            await interactionService.ExecuteCommandAsync(context, serviceProvider);
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    private async Task ClientReady()
    {
        await interactionService
            .AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);

        if (IsDebug())
        {
            ulong.TryParse(Environment.GetEnvironmentVariable("DEV_GUILD")!, out var guildId);
            var guild = client.Guilds.FirstOrDefault(g => g.Id == guildId);
            var commands = await interactionService.RegisterCommandsToGuildAsync(guildId);
            Log.Information($"Deployed {commands.Count} commands to {guild?.Name ?? guildId.ToString()}");
        }
        else
        {
            var commands = await interactionService.RegisterCommandsGloballyAsync();
            Log.Information($"Deployed {commands.Count} commands globally");
        }
    }

    private static async Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }

    private static async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            Log.Warning("[Command] {ContextUser} tried to run {CommandName} but ran into {S}", context.User, command.Name, result.Error.ToString());
            var embed = new EmbedBuilder
            {
                Color = new Color(0x2F3136)
            };
            switch (result.Error)
            {
                case InteractionCommandError.BadArgs:
                    embed.Title = "Invalid Arguments";
                    embed.Description =
                        "Please make sure the arguments you're providing are correct.\nIf you keep running into this message, please join the support server";
                    break;
                case InteractionCommandError.ConvertFailed:
                case InteractionCommandError.Exception:
                    if (result.ErrorReason.StartsWith("Unable to connect to"))
                    {
                        embed.Title = "Missing Permissions";
                        embed.Description = result.ErrorReason;
                    }
                    else
                    {
                        embed.Title = "Something Happened";
                        embed.Description = "I was unable to run your command.\nIf it continues to happen join the support server";
                    }

                    break;
                case InteractionCommandError.UnmetPrecondition:
                    embed.Title = "Missing Permissions";
                    embed.Description = result.ErrorReason;
                    break;
                default:
                    embed.Title = "Something Happened";
                    embed.Description = "I was unable to run your command.\nIf it continues to happen join the support server";
                    break;
            }

            if (context.Interaction.HasResponded)
                await context.Interaction.ModifyOriginalResponseAsync(m => m.Embed = embed.Build());
            else
                await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
        else
        {
            var guild = context.Interaction.ContextType != InteractionContextType.Guild
                ? "DM"
                : context.Guild != null
                    ? $"{context.Guild.Name} ({context.Guild.Id}) #{context.Channel.Name} ({context.Channel.Id})"
                    : $"User Context {context.Interaction.GuildId} {context.Interaction.ChannelId}";
            Log.Information(
                $"[Command] {guild} {Format.UsernameAndDiscriminator(context.User, false)} ({context.User.Id}) ran /{(string.IsNullOrEmpty(command.Module.Parent?.SlashGroupName) ? string.Empty : command.Module.Parent.SlashGroupName + ' ')}{(string.IsNullOrEmpty(command.Module.SlashGroupName) ? string.Empty : command.Module.SlashGroupName + ' ')}{command.Name} {ParseArgs(((SocketSlashCommandData)context.Interaction.Data).Options)}");
        }
    }

    private static string ParseArgs(IEnumerable<SocketSlashCommandDataOption> data)
    {
        var args = new List<string>();

        foreach (var option in data)
        {
            switch (option.Type)
            {
                case ApplicationCommandOptionType.SubCommand:
                case ApplicationCommandOptionType.SubCommandGroup:
                    args.Add(ParseArgs(option.Options));
                    break;
                case ApplicationCommandOptionType.Channel:
                case ApplicationCommandOptionType.Role:
                case ApplicationCommandOptionType.User:
                    args.Add($"{option.Name}:{((ISnowflakeEntity)option.Value).Id.ToString()}");
                    break;
                default:
                    args.Add($"{option.Name}:{option.Value}");
                    break;
            }
        }

        return string.Join(' ', args);
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
            return false;
#endif
    }
}
