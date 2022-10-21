using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Chos5555Bot;
using Chos5555Bot.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Chos5555Bot.Modules.Voice;
using Chos5555Bot.Modules;
using DAL;
using Victoria;
using Chos5555Bot.Services;
using System.Runtime.InteropServices;

public class Program
{
    // TODO: Add stage channel feature
    // TODO: Add quest feature

    static void Main(string[] args = null)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private DiscordSocketClient _client;
    private Config.ConfigService _configService;
    private Config.Config _config;

    // TODO: Move Startup to it's own file
    public async Task MainAsync()
    {
        // Setup config for caching
        var socketConfig = new DiscordSocketConfig()
        {
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All
        };

        // Setup services
        var services = ConfigureServices(socketConfig);

        // Assign client and commands to local variables
        var client = services.GetRequiredService<DiscordSocketClient>();
        _client = client;

        // Get token from config file
        _configService = new();
        _config = _configService.GetConfig();

        Console.WriteLine($"{_config.ConnectionString}");

        // Initialize GameAnnouncer
        GameAnnouncer.InitAnnouncer(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize Reaction handler
        Reactions.InitReactions(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize Role handler
        Roles.InitRoles(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Log in to Discord
        await client.LoginAsync(TokenType.Bot, _config.Token);

        // Start log service
        services.GetRequiredService<LogService>();

        // Start connection logic
        await client.StartAsync();

        // Start CommandHandler
        await services.GetRequiredService<CommandHandler>().SetupAsync();

        // Initialize MusicService
        await services.GetRequiredService<MusicService>().InitializeAsync();

        // Handle added/removed emote to message
        client.ReactionAdded += Reactions.AddHandler;
        client.ReactionRemoved += Reactions.RemoveHandler;

        // Handle role updates
        client.RoleUpdated += Roles.UpdateHandler;

        // TODO: Handle channel/role deletion to delete from DB

        // Block this task until the program is closed
        await Task.Delay(-1);
    }

    private ServiceProvider ConfigureServices(DiscordSocketConfig config)
    {
        // Setup services and dependency injection
        var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(config))
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            // TODO: Check if you should use AddDbContextPool
            //.AddDbContext<BotDbContext>( options => options.UseSqlServer(_config.ConnectionString))
            .AddDbContext<BotDbContext>()
            .AddSingleton<BotRepository>()
            .AddSingleton<Queue>()
            .AddLavaNode(x => { x.SelfDeaf = false; })
            .AddSingleton<MusicService>()
            .AddSingleton<LogService>()
            .AddSingleton<Reactions>()
            .AddSingleton<Roles>();


        // Setup provider
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }
}