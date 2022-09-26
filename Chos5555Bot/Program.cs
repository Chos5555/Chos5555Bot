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

public class Program
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private Config.ConfigService _configService;
    private Config.Config _config;


    static void Main(string[] args = null)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public async Task MainAsync()
    {
        // Setup config for caching
        var socketConfig = new DiscordSocketConfig { MessageCacheSize = 100 };

        // Setup services
        var services = ConfigureServices(socketConfig);

        // Assign client and commands to local variables
        var client = services.GetRequiredService<DiscordSocketClient>();
        _client = client;

        var commands = services.GetRequiredService<CommandService>();
        _commands = commands;

        // Get token from config file
        _configService = new();
        _config = _configService.GetConfig();

        Console.WriteLine($"{_config.ConnectionString}");

        // Setup GameAnnouncer
        GameAnnouncer.InitAnnouncer(services.GetRequiredService<BotRepository>());

        // Log information to the console
        client.Log += Log;

        // Log in to Discord
        await client.LoginAsync(TokenType.Bot, _config.Token);

        // Start connection logic
        await client.StartAsync();

        // Start CommandHandler
        await services.GetRequiredService<CommandHandler>().SetupAsync();

        // Initialize MusicService
        await services.GetRequiredService<MusicService>().InitializeAsync();

        // React upond added emoji to message
        client.ReactionAdded += Reactions.AddHandler;
        client.ReactionRemoved += Reactions.RemoveHandler;

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
            .AddDbContext<BotDbContext>()
            .AddSingleton<BotRepository>()
            .AddSingleton<Queue>()
            .AddLavaNode(x => { x.SelfDeaf = false; })
            .AddSingleton<MusicService>();

        // Setup provider
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    //TODO: Improve log, log errors into a file, log successes to console
    private static Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }
}