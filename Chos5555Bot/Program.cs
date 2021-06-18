using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;
using Chos5555Bot;
using Chos5555Bot.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static DiscordSocketClient _client;

    static void Main(string[] args = null)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public async Task MainAsync()
    {
        var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();
        _client = client;

        // Log information to the console
        client.Log += Log;

        // Read the token from file
        var token = File.ReadAllText("token.txt");

        // Log in to Discord
        await client.LoginAsync(TokenType.Bot, token);

        // Start connection logic
        await client.StartAsync();

        // Start CommandHandler
        await services.GetRequiredService<CommandHandler>().SetupAsync();    

        // React upond added emoji to message
        client.ReactionAdded += Reactions.Handler;

        // Block this task until the program is closed
        await Task.Delay(-1);
    }

    private ServiceProvider ConfigureServices()
    {
        // Setup services and dependency injection
        var services = new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .AddDbContext<DAL.BotDbContext>();

        // Setup provider
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    private static Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }
}