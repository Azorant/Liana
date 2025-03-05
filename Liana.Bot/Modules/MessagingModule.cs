using Discord;
using Discord.Interactions;
using Discord.Rest;
using Humanizer;
using Liana.Bot.Modules.Management;
using Liana.Database;
using Liana.Models.Enums;
using Serilog;

namespace Liana.Bot.Modules;

[Group("message", "Create or edit messages")]
public class MessagingModule(DatabaseContext db) : BaseModule(db)
{
    [SlashCommand("create", "Creates a message")]
    public async Task CreateCommand(ITextChannel channel)
    {
        var config = await GetConfigAsync();
        if (!AssertConfigRole(config, RoleEnum.Staff))
        {
            await SendPermissionErrorAsync("You must have a role with Staff permission run this command.");
            return;
        }

        var missingPermissions = Context.Guild.CurrentUser.GetPermissions(channel)
            .Missing(ChannelPermission.ViewChannel, ChannelPermission.SendMessages, ChannelPermission.EmbedLinks);
        if (missingPermissions.Any())
        {
            await SendPermissionErrorAsync(
                $"Please make sure I have the {"permission".ToQuantity(missingPermissions.Count, ShowQuantityAs.None)} {string.Join(", ", missingPermissions.Select(p => $"**{p}**"))} in {channel.Mention}");
            return;
        }

        await Context.Interaction.RespondWithModalAsync<MessageModal>($"create_message_{channel.Id}");
    }

    [SlashCommand("edit", "Edit a message")]
    public async Task EditCommand(ITextChannel channel, string messageId)
    {
        var config = await GetConfigAsync();
        if (!AssertConfigRole(config, RoleEnum.Staff))
        {
            await SendPermissionErrorAsync("You must have a role with Staff permission run this command.");
            return;
        }

        var missingPermissions = Context.Guild.CurrentUser.GetPermissions(channel)
            .Missing(ChannelPermission.ViewChannel, ChannelPermission.ReadMessageHistory);
        if (missingPermissions.Any())
        {
            await SendPermissionErrorAsync(
                $"Please make sure I have the {"permission".ToQuantity(missingPermissions.Count, ShowQuantityAs.None)} {string.Join(", ", missingPermissions.Select(p => $"**{p}**"))} in {channel.Mention}");
            return;
        }

        await Context.Interaction.RespondWithModalAsync<MessageModal>($"edit_message_{channel.Id}_{messageId}");
    }

    public class MessageModal : IModal
    {
        public string Title => "Create Message";

        [InputLabel("Content")]
        [ModalTextInput("content", TextInputStyle.Paragraph), RequiredInput(false)]
        public string? Content { get; set; }

        [InputLabel("Embed JSON")]
        [ModalTextInput("embed", TextInputStyle.Paragraph), RequiredInput(false)]
        public string? Embed { get; set; }
    }

    [ModalInteraction("create_message_*", true)]
    public async Task CreateModalResponse(ulong channelId, MessageModal modal)
    {
        try
        {
            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null) return; // Should never be null cause channel is checked before modal is sent
            if (string.IsNullOrEmpty(modal.Content) && string.IsNullOrEmpty(modal.Embed))
            {
                await SendErrorAsync("No content or embed provided.");
                return;
            }

            await channel.SendMessageAsync(modal.Content, embed: string.IsNullOrEmpty(modal.Embed) ? null : EmbedBuilderUtils.Parse(modal.Embed).Build());

            await SendSuccessAsync("Message created");
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while creating a message");
            await SendErrorAsync("Encountered an error while creating the message");
        }
    }

    [ModalInteraction("edit_message_*_*", true)]
    public async Task EditModalResponse(ulong channelId, ulong messageId, MessageModal modal)
    {
        try
        {
            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null) return; // Should never be null cause channel is checked before modal is sent
            var message = await channel.GetMessageAsync(messageId);
            if (message == null)
            {
                await SendErrorAsync("Unknown message.");
                return;
            }

            if (message.Author.Id != Context.Client.CurrentUser.Id)
            {
                await SendErrorAsync("Can't edit messages sent by other users.");
                return;
            }

            if (string.IsNullOrEmpty(modal.Content) && string.IsNullOrEmpty(modal.Embed))
            {
                await SendErrorAsync("No content or embed provided.");
                return;
            }

            await channel.ModifyMessageAsync(messageId, m =>
            {
                m.Content = modal.Content;
                m.Embeds = string.IsNullOrEmpty(modal.Embed) ? Optional.Create<Embed[]>([]) : Optional.Create<Embed[]>([EmbedBuilderUtils.Parse(modal.Embed).Build()]);
            });

            await SendSuccessAsync("Message edited");
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while editing a message");
            await SendErrorAsync("Encountered an error while editing the message");
        }
    }
}