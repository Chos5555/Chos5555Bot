using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;

namespace Chos5555Bot.Services
{
    /// <summary>
    /// Class containing methods used for logging
    /// </summary>
    public class LogService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private string LogDir { get; }
        private string LogFile => Path.Combine(LogDir, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public LogService(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;

            LogDir = Path.Combine(AppContext.BaseDirectory, "logs");

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private async Task OnLogAsync(LogMessage message)
        {
            EnsureLogFileExists();

            var textToLog = $"{DateTime.UtcNow:yyyy-MM-dd} [{message.Severity}] {message.Source}: {message.Exception?.ToString() ?? message.Message}";
            await File.AppendAllTextAsync(LogFile, textToLog + "\n");

            Console.Out.WriteLine(textToLog);
            return;
        }

        /// <summary>
        /// Logs message into a file and console
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="severity">Message severity</param>
        /// <returns>Nothing</returns>
        public async Task Log(string message, LogSeverity severity)
        {
            EnsureLogFileExists();

            var textToLog = $"{DateTime.UtcNow:yyyy-MM-dd} [{severity}]: {message}";
            await File.AppendAllTextAsync(LogFile, textToLog + "\n");

            Console.Out.WriteLine(textToLog);
            return;
        }

        private void EnsureLogFileExists()
        {
            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);

            if (!File.Exists(LogFile))
                File.Create(LogFile);
        }
    }
}
