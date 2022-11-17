using DAL;
using Chos5555Bot.Services;
using DAL.Model;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chos5555Bot.Misc;

namespace Chos5555Bot.Modules.Quests
{
    /// <summary>
    /// Module class containing commands for the quest feature
    /// </summary>
    public class Quests : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public Quests(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        /// <summary>
        /// Initializes quest feature for a game in whose channel the command was used.
        /// Sets ModQuestRoom of the game to a channel with the id provided, if no id was provided,
        /// creates a new one.
        /// </summary>
        /// <param name="modRoomId">Id of the channel to be set as ModQeustRoom (Optional).</param>
        /// <returns>Nothing</returns>
        [Command("InitializeQuest")]
        [Summary("Creates a mod quest room in this game's category only accessible by mod accept roles.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task InitQuest(
            [Name("ModQuestRoom Id")][Summary("Sets this channel as the mod quest room, if no channel id is given, creates new channel (optional")] ulong modRoomId = 0)
        {
            // Check that channel is in a category and belongs to a game
            var nestedChannel = (Context.Channel as INestedChannel);
            if (!nestedChannel.CategoryId.HasValue)
            {
                await ReplyAsync("This channel is not in a category.");
                return;
            }

            var game = await _repo.FindGameByCategoryId(nestedChannel.CategoryId.Value);
            if (game is null)
            {
                await ReplyAsync("This category doesn't belong to a game.");
                return;
            }

            ITextChannel modQuestChannel = Context.Guild.GetTextChannel(modRoomId);

            // Create a new channel if no id was provided
            if (modRoomId == 0)
            {
                modQuestChannel = await Context.Guild.CreateTextChannelAsync("mod quest room",
                    r =>
                       {
                           r.CategoryId = nestedChannel.CategoryId;
                       });
                // Set permission to only enable mod roles to see
                var modRoles = new List<IRole>();
                foreach (var role in game.ModAcceptRoles)
                    modRoles.Add(Context.Guild.GetRole(role.DisordId));
                await PermissionSetter.EnableViewOnlyForRoles(modRoles, Context.Guild.EveryoneRole, modQuestChannel);
            }

            // Create new room in DB if not stored yet
            var modRoom = await _repo.FindRoom(modQuestChannel.Id);
            if (modRoom is null)
            {
                modRoom = new Room()
                {
                    DiscordId = modQuestChannel.Id,
                };
            }

            // Set games ModQuestRoom
            game.ModQuestRoom = modRoom;

            await _log.Log($"Initialized quest feature for {game.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }

        /// <summary>
        /// Creates a new quest with given text. Deletes the command message and sends a quest message,
        /// adds raised hand reaction.
        /// </summary>
        /// <param name="text">Text of the quest</param>
        /// <returns>Nothing</returns>
        [Command("addQuest")]
        [Summary("Adds a new quest.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddQuest(
            [Name("Text")][Summary("Text of the quest")][Remainder] string text)
        {
            // Find a game for the category this channel is in
            var categoryId = (Context.Channel as INestedChannel).CategoryId.Value;
            var game = await _repo.FindGameByCategoryId(categoryId);

            // Delete command message
            await Context.Message.DeleteAsync();

            // Send quest message
            var message = await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} has added a new quest:\n{text}\n" +
                $"Press ✋ down below to claim this quest.");
            await message.AddReactionAsync(new Emoji("✋"));

            // Create new quest and store it in DB
            var quest = new Quest()
            {
                GameName = game.Name,
                Text = text,
                AuthorId = Context.User.Id,
                QuestMessage = message.Id,
                QuestMessageChannelId = Context.Channel.Id
            };

            await _repo.AddQuest(quest);
        }

        /// <summary>
        /// Resets completed quest amounts for all users who completed quests for a game in whose channel the command was used
        /// </summary>
        /// <returns>Nothing</returns>
        [Command("resetQuests")]
        [Summary("Resets completed quest amount for all users.")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task ResetQuests()
        {
            // Check that channel is in a category and belongs to a game
            // TODO: Add to helper method
            var nestedChannel = (Context.Channel as INestedChannel);
            if (!nestedChannel.CategoryId.HasValue)
            {
                await ReplyAsync("This channel is not in a category.");
                return;
            }

            var game = await _repo.FindGameByCategoryId(nestedChannel.CategoryId.Value);
            if (game is null)
            {
                await ReplyAsync("This category doesn't belong to a game.");
                return;
            }

            // Find all users who have completed a quest for given game
            var users = await _repo.FindUsersWithQuestsForGame(game);

            // Reset users QuestCount for the game to 0
            foreach(var user in users)
            {
                user.CompletedQuests.Where(c => c.GameName == game.Name).Single().QuestCount = 0;
                await _repo.UpdateUser(user);
            }
        }

        /// <summary>
        /// Posts a message with the top 10 users who have completed the most quests for game
        /// </summary>
        /// <returns>Nothing</returns>
        [Command("questLeaderboard")]
        [Summary("Shows the top 10 users with the most completed quests for this game.")]
        public async Task QuestLeaderboard()
        {
            // TODO: Rework to make embed and nicer
            // Check that channel is in a category and belongs to a game
            // TODO:
            var nestedChannel = (Context.Channel as INestedChannel);
            if (!nestedChannel.CategoryId.HasValue)
            {
                await ReplyAsync("This channel is not in a category.");
                return;
            }

            var game = await _repo.FindGameByCategoryId(nestedChannel.CategoryId.Value);
            if (game is null)
            {
                await ReplyAsync("This category doesn't belong to a game.");
                return;
            }

            // Get the 10 users with the most quests completed for given game
            var users = (await _repo.FindUsersWithQuestsForGame(game))
                .Select(u => (u.DiscordId, u.CompletedQuests.Where(q => q.GameName == game.Name).Single().QuestCount))
                .OrderByDescending(x => x.Item2)
                .Take(10);

            // Create leaderboard text
            var content = $"These are the 10 users with the most completions of quests for game {game.Name}:\n";
            foreach(var user in users)
            {
                content += $"{Context.Guild.GetUser(user.DiscordId)} : {user.Item2}\n";
            }

            await ReplyAsync(content);
        }

        /// <summary>
        /// Shows the amount of quests the user, that has used this command, completed for a game in whose channel the command has been used
        /// </summary>
        /// <returns>Nothing</returns>
        [Command("quests")]
        [Summary("Shows the amount of quests you have completed for this game.")]
        public async Task QuestsCommand()
        {
            // Check that channel is in a category and belongs to a game
            // TODO:
            var nestedChannel = (Context.Channel as INestedChannel);
            if (!nestedChannel.CategoryId.HasValue)
            {
                await ReplyAsync("This channel is not in a category.");
                return;
            }

            var game = await _repo.FindGameByCategoryId(nestedChannel.CategoryId.Value);
            if (game is null)
            {
                await ReplyAsync("This category doesn't belong to a game.");
                return;
            }

            // Get user from DB
            var user = await _repo.FindUser(Context.User.Id);

            // Find how many quests user has completed for this game, or 0
            var count = 0;
            if (user is not null && user.CompletedQuests.Where(c => c.GameName == game.Name).Any())
                count = user.CompletedQuests.Where(c => c.GameName == game.Name).Single().QuestCount;
            await ReplyAsync($"You have completed {count} quests for {game.Name}.");
        }

        /// <summary>
        /// Shows the amount of quests given user completed for a game in whose channel the command has been used
        /// </summary>
        /// <param name="userName">Name of the user</param>
        /// <returns></returns>
        [Command("quests")]
        [Summary("Shows the amount of quests given user has completed for this game.")]
        public async Task QuestsCommand(
            [Name("User name")][Summary("Name of the user.")] string userName)
        {
            // Check that channel is in a category and belongs to a game
            var nestedChannel = (Context.Channel as INestedChannel);
            if (!nestedChannel.CategoryId.HasValue)
            {
                await ReplyAsync("This channel is not in a category.");
                return;
            }

            var game = await _repo.FindGameByCategoryId(nestedChannel.CategoryId.Value);
            if (game is null)
            {
                await ReplyAsync("This category doesn't belong to a game.");
                return;
            }

            // Find user with given name
            var discordUser = await UserFinder.FindUserByName(userName, Context.Guild);

            if (discordUser is null)
            {
                await Context.Channel.SendMessageAsync("Sorry, I couldn't find that user on this server, make sure you wrote the name correctly.");
                return;
            }

            // Get user from DB
            var user = await _repo.FindUser(discordUser.Id);

            // Find how many quests user has completed for this game, or 0
            var count = 0;
            if (user is not null && user.CompletedQuests.Where(c => c.GameName == game.Name).Any())
                count = user.CompletedQuests.Where(c => c.GameName == game.Name).Single().QuestCount;
            await ReplyAsync($"{discordUser.Username} has completed {count} quests for {game.Name}.");
        }
    }
}
