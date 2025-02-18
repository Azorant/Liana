using Liana.Bot;
using Liana.Bot.HostedServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Liana.Bot.Services;
using Liana.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    var builder = new HostApplicationBuilder();

    builder.Services
        .AddDbContext<DatabaseContext>(options => DatabaseContextFactory.CreateDbOptions(options), ServiceLifetime.Transient)
        .AddSingleton(new InteractiveConfig { ReturnAfterSendingPaginator = true })
        .AddSingleton<InteractiveService>()
        .AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            AlwaysDownloadUsers = true
        })
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<InteractionService>(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
        .AddTransient<AuditLogService>()
        .AddHostedService<DiscordClientHost>()
        .AddHostedService<ClientStatus>()
        .AddSerilog();

    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        db.ApplyMigrations();
    }

    host.Run();
}
catch (Exception error)
{
    Log.Error(error, "Error in main");
}
finally
{
    Log.CloseAndFlush();
}