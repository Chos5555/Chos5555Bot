using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;

namespace Chos5555Bot.Services
{
    public class LogService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        private string _logDir { get; }
        private string _logFile => Path.Combine(_logDir, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

        public LogService(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;

            _logDir = Path.Combine(AppContext.BaseDirectory, "logs");

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private async Task OnLogAsync(LogMessage message)
        {
            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);

            if (!File.Exists(_logFile))
                File.Delete(_logFile);

            string textToLog = $"{DateTime.UtcNow.ToString("yyyy-MM-dd")} [{message.Severity}] {message.Source}: {message.Exception?.ToString() ?? message.Message}";
            await File.AppendAllTextAsync(_logFile, textToLog + "\n");

            Console.Out.WriteLine(textToLog);
            return;
        }
    }
}
