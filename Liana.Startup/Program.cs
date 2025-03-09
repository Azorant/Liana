using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Liana.Bot.HostedServices;
using Liana.Bot.Jobs;
using Liana.Bot.Services;
using Liana.Database;
using Liana.Startup;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    var builder = WebApplication.CreateBuilder(args);

    var redisConfiguration = Environment.GetEnvironmentVariable("REDIS");
    ArgumentException.ThrowIfNullOrEmpty(redisConfiguration);

    #region Common

    builder.Services
        .AddDbContext<DatabaseContext>(options => DatabaseContextFactory.CreateDbOptions(options), ServiceLifetime.Transient)
        .AddSingleton(ConnectionMultiplexer.Connect(redisConfiguration))
        .AddSerilog();

    #endregion

    #region Bot

    builder.Services
        .AddSingleton(new InteractiveConfig { ReturnAfterSendingPaginator = true })
        .AddSingleton<InteractiveService>()
        .AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        })
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<InteractionService>(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
        .AddHostedService<DiscordClientHost>()
        .AddTransient<AuditLogService>();

    #endregion

    #region Website

    builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Any, 5123));

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    #endregion

    #region Jobs

    builder.Services.AddHangfire((provider, configuration) => configuration.UseRedisStorage(provider.GetRequiredService<ConnectionMultiplexer>(), new RedisStorageOptions
        {
            Prefix = $"{Environment.GetEnvironmentVariable("PREFIX") ?? "liana"}:hangfire:"
        }))
        .AddHangfireServer()
        .AddSingleton<StatusJob>()
        .AddSingleton<ReminderJob>();

    #endregion

    var host = builder.Build();

    host.MapOpenApi();
    host.MapScalarApiReference();
    host.UseAuthorization();
    host.MapControllers();
    host.UseHangfireDashboard(options: new DashboardOptions
    {
        Authorization = new[] { new DashboardNoAuthorizationFilter() },
        IgnoreAntiforgeryToken = true
    });

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await db.ApplyMigrations();
    }

    RecurringJob.AddOrUpdate<StatusJob>("client_status", x => x.SetStatus(), "0,15,30,45 * * * * *");
    RecurringJob.AddOrUpdate<ReminderJob>("reminders", x=> x.CheckReminders(), "0,15,30,45 * * * * *");

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