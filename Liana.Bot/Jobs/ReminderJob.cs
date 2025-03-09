using Discord;
using Discord.WebSocket;
using Hangfire;
using Liana.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Liana.Bot.Jobs;

public class ReminderJob(IServiceProvider serviceProvider)
{
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task CheckReminders()
    {
        var db = serviceProvider.GetRequiredService<DatabaseContext>();
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();

        await foreach (var reminder in db.Reminders.Where(r => r.RemindAt <= DateTime.UtcNow).AsAsyncEnumerable())
        {
            db.Remove(reminder);
            try
            {
                var channel = await client.GetChannelAsync(reminder.ChannelId);
                if (channel is not ITextChannel textChannel) continue;
                var bot = await textChannel.Guild.GetCurrentUserAsync();
                var missing = bot?.GetPermissions(textChannel).Missing(ChannelPermission.ViewChannel, ChannelPermission.SendMessages, ChannelPermission.EmbedLinks);
                if (missing is null || missing.Count != 0) continue;

                await textChannel.SendMessageAsync(MentionUtils.MentionUser(reminder.UserId), embed: new EmbedBuilder()
                    .WithTitle($"You asked me {TimestampTag.FormatFromDateTime(reminder.CreatedAt.SpecifyUtc(), TimestampTagStyles.Relative)} to remind you")
                    .WithDescription(reminder.Content)
                    .WithColor(Color.Teal)
                    .WithFooter("Created")
                    .WithTimestamp(reminder.CreatedAt.SpecifyUtc())
                    .Build());
            }
            catch (Exception e)
            {
                Log.Error(e, "An error occured during reminder execution");
            }
        }

        await db.SaveChangesAsync();
    }
}