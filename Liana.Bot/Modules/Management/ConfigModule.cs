using Discord;
using Discord.Interactions;
using Liana.Database;
using Liana.Models;
using Liana.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YamlDotNet.Core;
using Parser = Liana.Models.Parser;

namespace Liana.Bot.Modules.Management;

[Group("config", "Get or set config")]
public class ConfigModule(DatabaseContext db) : BaseModule(db)
{
    [SlashCommand("get", "Get server config")]
    public async Task GetCommand()
    {
        await DeferAsync();
        var raw = await GetRawConfigAsync();
        var config = Parser.DeserializeConfig(raw);
        if (!AssertConfigRole(config, RoleEnum.Admin))
        {
            await SendErrorAsync("Missing permission to view config");
            return;
        }

        // TODO: Check if yaml is greater than character limit and send as file
        await FollowupAsync(embed: new EmbedBuilder()
            .WithTitle("Current Config")
            .WithDescription(Format.Code(raw, "yaml"))
            .WithColor(Color.Purple)
            .Build());
    }

    [SlashCommand("set", "Set server config")]
    public async Task SetCommand()
    {
        var raw = await GetRawConfigAsync();
        var config = Parser.DeserializeConfig(raw);
        if (!AssertConfigRole(config, RoleEnum.Admin))
        {
            await SendErrorAsync("Missing permission to view config");
            return;
        }

        await Context.Interaction.RespondWithModalAsync<ConfigModal>("config_modal",
            modifyModal: options => options.UpdateTextInput("config", input => input.Value = raw));
    }

    [SlashCommand("reactions", "Setup reactions")]
    public async Task ReactionCommand()
    {
        await DeferAsync();
        var config = await GetConfigAsync();
        if (!AssertConfigRole(config, RoleEnum.Admin))
        {
            await SendErrorAsync("Missing permission");
            return;
        }

        if (config.ReactionRoles == null)
        {
            await SendErrorAsync("Reaction role module not setup");
            return;
        }

        await FollowupAsync(embed: new EmbedBuilder()
            .WithDescription("Setting up reactions, this might take some time.")
            .WithColor(Color.Blue)
            .Build());

        var fails = new Dictionary<string, List<string>>();

        foreach (var (channelId, messages) in config.ReactionRoles)
        {
            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null) continue;
            foreach (var (messageId, emotes) in messages)
            {
                var message = await channel.GetMessageAsync(messageId);
                if (message == null) continue;
                foreach (var (raw, _) in emotes)
                {
                    IEmote emote = Parser.DeserializeEmote(raw);
                    try
                    {
                        await message.AddReactionAsync(emote);
                    }
                    catch (Exception e)
                    {
                        var key = $"{channelId}/{messageId}";
                        if (fails.TryGetValue(key, out var list))
                        {
                            list.Add(emote.ToString()!);
                        }
                        else
                        {
                            fails.Add(key, [emote.ToString()!]);
                        }
                    }
                }
            }
        }

        await SendSuccessAsync(
            $"Reactions setup{(fails.Count == 0 ? string.Empty : $"\n\nFailed Reactions:\n{string.Join('\n', fails.Select(v => $"https://discord.com/channels/{Context.Guild.Id}/{v.Key} - {string.Join(", ", v.Value)}"))}")}",
            true);
    }

    public class ConfigModal : IModal
    {
        public string Title => "Update Config";

        [InputLabel("Config")]
        [ModalTextInput("config", TextInputStyle.Paragraph)]
        public required string Config { get; set; }
    }

    [ModalInteraction("config_modal", true)]
    public async Task ModalResponse(ConfigModal modal)
    {
        try
        {
            Parser.DeserializeConfig(modal.Config);
            var guild = await db.Guilds.FirstAsync(g => g.Id == Context.Guild.Id);
            guild.Config = modal.Config;
            db.Update(guild);
            await db.SaveChangesAsync();
            await SendSuccessAsync("Config updated");
        }
        catch (SemanticErrorException ex)
        {
            Log.Error(ex, "Failed to parse config");
            await SendErrorAsync($"{ex.Message}\nLine {ex.End.Line} Column {ex.End.Column}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse config");
            await SendErrorAsync(ex.Message);
        }
    }
}