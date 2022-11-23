using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Chos5555Bot.Misc;
using System;
using System.Linq;

namespace Chos5555Bot.Modules.ActivityTracker
{
    /// <summary>
    /// Module class containing commands for activity tracker
    /// </summary>
    public class ActivityTracker : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ActivityTracker(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [Command("InititalizeTracking")]
        [Alias("InitTracking")]
        [Summary("Initializes tracking feature for game in which category the commands has been used.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task InitTracking()
        {
            // Check that channel is in a category and belongs to a game
            var ((result, exception), game, _) = await GameFinder.TryFindGameForChannel(Context.Channel);
            if (!result)
            {
                await ReplyAsync(exception.Message);
                return;
            }

            game.TrackActivity = true;
            await _repo.UpdateGame(game);

            await _log.Log($"Initialized tracking for {game.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }

        [Command("SetTrackingDuration")]
        [Summary("Sets tracking duration after which main active role is supposed to be removed for game in whose category the command has been used in.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetTrackingDuratin(
            [Name("Time")][Summary("Time in format of \"x seconds/minutes/hours/days/weeks\"")][Remainder]string stringDuration)
        {
            // Check that channel is in a category and belongs to a game
            var ((result, exception), game, _) = await GameFinder.TryFindGameForChannel(Context.Channel);
            if (!result)
            {
                await ReplyAsync(exception.Message);
                return;
            }

            // Split input
            var split = stringDuration.Split(" ");

            // Return if the input doesn't have the right length
            if (split.Length != 2)
            {
                await ReplyAsync("Could'n understand your input, check you use the right format.");
                return;
            }

            TimeSpan duration;

            var number = int.Parse(split[0]);
            var unit = split[1];

            // Parse input string into TimeSpan or tell user it couldn't be parsed
            switch (unit.ToLower())
            {
                case "seconds":
                    duration = TimeSpan.FromSeconds(number);
                    break;
                case "minutes":
                    duration = TimeSpan.FromMinutes(number);
                    break;
                case "hours":
                    duration = TimeSpan.FromHours(number);
                    break;
                case "days":
                    duration = TimeSpan.FromDays(number);
                    break;
                case "weeks":
                    duration = TimeSpan.FromDays(number * 7);
                    break;
                default:
                    await ReplyAsync("Could'n understand your input, check you use the right format.");
                    return;
            }

            // Update game with new duration
            game.RemoveAfter = duration;
            await _repo.UpdateGame(game);

            await _log.Log($"Set tracking duration for {game.Name} in {Context.Guild.Name} to {stringDuration}.", LogSeverity.Info);
        }

        [Command("ResetTracking")]
        [Summary("Resets tracking for all users that are playing the game in whose channel the command was used in.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task ResetTracking()
        {
            // Check that channel is in a category and belongs to a game
            var ((result, exception), game, _) = await GameFinder.TryFindGameForChannel(Context.Channel);
            if (!result)
            {
                await ReplyAsync(exception.Message);
                return;
            }

            // Get all users that have activity for this game
            var users = _repo.FindAllUsersActivityForGame(game);

            // Remove activity of game for user and update in DB
            foreach (var (user, activity) in users)
            {
                user.GameActivities.Remove(activity);
                await _repo.UpdateUser(user);
                await _repo.RemoveGameActivity(activity);
            }

            game.LastActivityCheck = DateTime.UtcNow;
            await _repo.UpdateGame(game);

            await _log.Log($"Reset tracking for {game.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }

        [Command("ListTracking")]
        [Summary("Lists all users that are tracked and time when they last connected to a games voice channel.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ListTracking()
        {
            // Check that channel is in a category and belongs to a game
            var ((result, exception), game, _) = await GameFinder.TryFindGameForChannel(Context.Channel);
            if (!result)
            {
                await ReplyAsync(exception.Message);
                return;
            }

            // Get all users that have activity for this game
            var users = _repo.FindAllUsersActivityForGame(game);

            var content = $"**Tracked users for {game.Name} (in UTC time):**\n";

            // Put all users and their last appearance into message content
            foreach (var (user, activity) in users)
            {
                content += $"{Context.Guild.GetUser(user.DiscordId).DisplayName} : {activity.LastAppearance}\n";
            }

            // Send message
            await ReplyAsync(content);
        }

        [Command("TrackUser")]
        [Summary("Shows when user last connected to a games voice channel and how much time remains until they get the games main active role removed.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task TrackUser(
            [Name("User")][Summary("Name of the user to get info about.")][Remainder] string username)
        {
            // Check that channel is in a category and belongs to a game
            var ((result, exception), game, _) = await GameFinder.TryFindGameForChannel(Context.Channel);
            if (!result)
            {
                await ReplyAsync(exception.Message);
                return;
            }

            var discordUser = await UserFinder.FindUserByName(username, Context.Guild);
            if (discordUser is null)
            {
                await ReplyAsync("Couldn't find this user, make sure you wrote the name correctly.");
                return;
            }

            var user = await _repo.FindUser(discordUser.Id);
            if (user is null)
            {
                await ReplyAsync($"This user isn't in the database, so he is not tracked.");
                return;
            }

            var gameActivity = user.GameActivities.Where(g => g.GameName == game.Name).SingleOrDefault();
            if (gameActivity is null)
            {
                await ReplyAsync($"This user doesn't have {game.MainActiveRole.Name} role, so he is not tracked.");
                return;
            }

            var timeUntilRemove = game.RemoveAfter - (DateTime.UtcNow - gameActivity.LastAppearance);
            var formattedTime = $"{timeUntilRemove.Days} days {timeUntilRemove.Hours} hours {timeUntilRemove.Minutes} minutes {timeUntilRemove.Seconds} seconds";
            

            await ReplyAsync($"{discordUser.Mention} has last joined {game.Name}s voice channel at {gameActivity.LastAppearance} (UTC). " +
                $"This user will have his **{game.MainActiveRole.Name}** role removed if he doesn't connect to a voice channel in **{formattedTime}**");
        }
    }
}
