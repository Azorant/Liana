using Discord;
using Discord.Interactions;
using Liana.Database;
using Liana.Database.Entities;
using Liana.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Liana.Bot.Modules.User;

[Group("reminder", "Reminders")]
public class ReminderModule(DatabaseContext db) : BaseModule(db)
{
    [SlashCommand("add", "Add a reminder")]
    public async Task AddCommand(string time, string content)
    {
        await DeferAsync();
        var now = DateTime.UtcNow;
        if (!Parser.TryParseTime(time, now, out var parsed))
        {
            await SendErrorAsync("Unable to parse time, please use one of the following formats: `1h 3m`, `1 h 3 m`, or `1 hour 3 minutes`.");
            return;
        }

        await db.AddAsync(new ReminderEntity
        {
            UserId = Context.User.Id,
            ChannelId = Context.Channel.Id,
            Content = content,
            RemindAt = parsed
        });

        await db.SaveChangesAsync();

        await SendSuccessAsync($"I will remind you {TimestampTag.FormatFromDateTime(parsed.SpecifyUtc(), TimestampTagStyles.Relative)} about `{content.Sanitize()}`.");
    }

    [SlashCommand("remove", "Remove a reminder")]
    public async Task RemoveCommand([Autocomplete(typeof(ReminderAutocompleteHandler))] string id)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            await SendErrorAsync("Unknown reminder.");
            return;
        }
        await DeferAsync();
        var reminder = await db.Reminders.FindAsync(guid);
        if (reminder == null || reminder.UserId != Context.User.Id)
        {
            await SendErrorAsync("Unknown reminder.");
            return;
        }

        db.Remove(reminder);
        await db.SaveChangesAsync();
        await SendSuccessAsync("Reminder removed.");
    }
}

public class ReminderAutocompleteHandler(DatabaseContext db) : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        var text = autocompleteInteraction.Data.Current.Value as string;

        var reminders = await db.Reminders.Where(r => r.UserId == context.User.Id && (string.IsNullOrEmpty(text) || r.Content.Contains(text))).Take(10).AsNoTracking().ToListAsync();

        return AutocompletionResult.FromSuccess(reminders.Select(r =>
            new AutocompleteResult($"{r.Content.Substring(0, r.Content.Length >= 125 ? 125 : r.Content.Length)}{(r.Content.Length > 125 ? "..." : "")}", r.Id.ToString())));
    }
}