using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bot.Template.HostedServices;

internal sealed class ClientStatus(DiscordSocketClient client) : IHostedService, IDisposable
{
    private int lastStatus;
    private readonly string[] statuses = ["/help", "eris.gg"];
    private Timer? timer;

    public void Dispose()
    {
        timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(SetStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void SetStatus(object? state)
    {
        try
        {
            await client.SetCustomStatusAsync(statuses[lastStatus]);
            lastStatus++;
            if (lastStatus == statuses.Length) lastStatus = 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to set status");
        }
    }
}