using Bot.Template;
using Bot.Template.HostedServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    var builder = new HostApplicationBuilder();

    builder.Services
        .AddSingleton(new InteractiveConfig { ReturnAfterSendingPaginator = true })
        .AddSingleton<InteractiveService>()
        .AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        })
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<InteractionService>(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
        .AddSingleton<Events>()
        .AddHostedService<DiscordClientHost>()
        .AddHostedService<ClientStatus>()
        .AddSerilog();

    builder.Build().Run();
}
catch (Exception error)
{
    Log.Error(error, "Error in main");
}
finally
{
    Log.CloseAndFlush();
}