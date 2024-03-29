﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Chos5555Bot.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Chos5555Bot.Modules.Voice;
using Chos5555Bot.Modules;
using DAL;
using Victoria;
using Chos5555Bot.Services;
using Config;
using Chos5555Bot.Misc;

public class Program
{
    // TODO: Add music feature
    // TODO: Add user joined voice tracking feature with _client.UserVoiceStateUpdated
    // TODO: ADD README
    // TODO: Documentation: Add parameters into <paramsref>
    // TODO: Add handler to auto remove reaction from active messages when there is a active role removed from a user
    // TODO: Check out sharded client

    static void Main(string[] args = null)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private DiscordSocketClient _client;
    private Configuration _config = Configuration.GetConfig();

    // TODO: Move Startup to it's own file
    public async Task MainAsync()
    {
        // Setup config for caching
        var socketConfig = new DiscordSocketConfig()
        {
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All
        };

        // Setup config for command service
        var commandServiceConfig = new CommandServiceConfig()
        {
            DefaultRunMode = RunMode.Sync,
            CaseSensitiveCommands = false,
        };

        // Setup config for lavalink
        Action<LavaConfig> lavaConfig = x =>
        {
            x.SelfDeaf = false;
            x.Hostname = _config.LavalinkHostname;
            x.Port = _config.LavalinkPort;
            x.Authorization = _config.LavalinkPassword;
        };

        // Setup services
        var services = ConfigureServices(socketConfig, commandServiceConfig, lavaConfig);

        // Assign client and commands to local variables
        _client = services.GetRequiredService<DiscordSocketClient>();

        // Get token from config file
        _config = services.GetRequiredService<Configuration>();

        // Initialize GameAnnouncer
        GameAnnouncer.InitAnnouncer(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize MessageFinder
        MessageFinder.InitFinder(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize GameFinder
        GameFinder.InitFinder(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Configure event handlers
        ConfigureHandlers(_client, services);

        // Log in to Discord
        await _client.LoginAsync(TokenType.Bot, _config.Token);

        // Start log service
        services.GetRequiredService<LogService>();

        // Start connection logic
        await _client.StartAsync();

        // Start CommandHandler
        await services.GetRequiredService<Commands>().SetupAsync();

        // Initialize MusicService
        //await services.GetRequiredService<MusicService>().InitializeAsync();

        // Block this task until the program is closed
        await Task.Delay(-1);
    }

    private ServiceProvider ConfigureServices(DiscordSocketConfig discordConfig, CommandServiceConfig commandServiceConfig, Action<LavaConfig> lavaConfig)
    {
        // Setup services and dependency injection
        var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(discordConfig))
            .AddSingleton(_config)
            .AddSingleton(new CommandService(commandServiceConfig))
            .AddSingleton<Commands>()
            // TODO: Use AddDbContextPool for higher performance if number of requests to DB ever gets >2000/s
            .AddDbContext<BotDbContext>()
            .AddSingleton<BotRepository>()
            .AddSingleton<Queue>()
            //.AddLavaNode(lavaConfig)
            .AddSingleton<MusicService>()
            .AddSingleton<LogService>()
            .AddSingleton<Reactions>()
            .AddSingleton<Roles>()
            .AddSingleton<Users>();

        // Setup provider
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    private void ConfigureHandlers(DiscordSocketClient client, ServiceProvider services)
    {
        // Initialize Reaction handlers
        Reactions.InitReactions(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize Role handlers
        Roles.InitRoles(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize User handlers
        Users.InitUsers(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize Channel handlers
        Channels.InitChannels(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Initialize Guild handlers
        Guilds.InitGuilds(services.GetRequiredService<BotRepository>(),
            services.GetRequiredService<LogService>());

        // Handle added/removed emote to message
        client.ReactionAdded += Reactions.ReactionAdded;
        client.ReactionRemoved += Reactions.ReactionRemoved;

        // Handle user updates
        client.UserLeft += Users.UserLeft;

        // Handle role updates
        client.RoleUpdated += Roles.RoleUpdated;
        client.RoleDeleted += Roles.RoleDeleted;

        // Handle channel updates
        client.ChannelDestroyed += Channels.ChannelDestroyed;

        // Handle guild updates
        client.LeftGuild += Guilds.LeftGuild;

        // Handle when users voice state updates
        client.UserVoiceStateUpdated += Users.UserVoiceStateUpdated;
    }
}