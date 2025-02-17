using Discord;
using Discord.Interactions;
using Liana.Database;
using Liana.Models;
using Liana.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YamlDotNet.Core;

namespace Liana.Bot.Modules.Management;

[Group("config", "Get or set config")]
public class ConfigModule(DatabaseContext db) : BaseModule(db)
{
    [SlashCommand("get", "Get server config")]
    public async Task GetCommand()
    {
        await DeferAsync();
        var config = await GetConfigAsync();
        if (!AssertAdminRole(config, RoleEnum.Admin))
        {
            await SendErrorAsync("Missing permission to view config");
            return;
        }

        // TODO: Check if yaml is greater than character limit and send as file
        await FollowupAsync(embed: new EmbedBuilder()
            .WithTitle("Current Config")
            .WithDescription(Format.Code(ConfigParser.Serialize(config), "yaml"))
            .WithColor(Color.Purple)
            .Build());
    }

    [SlashCommand("set", "Set server config")]
    public async Task SetCommand()
    {
        var config = await GetConfigAsync();
        if (!AssertAdminRole(config, RoleEnum.Admin))
        {
            await SendErrorAsync("Missing permission to view config");
            return;
        }

        await Context.Interaction.RespondWithModalAsync<ConfigModal>("config_modal",
            modifyModal: options => options.UpdateTextInput("config", input => input.Value = ConfigParser.Serialize(config)));
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
            var guild = await db.Guilds.FirstAsync(g => g.Id == Context.Guild.Id);
            guild.Config = ConfigParser.Deserialize(modal.Config);
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