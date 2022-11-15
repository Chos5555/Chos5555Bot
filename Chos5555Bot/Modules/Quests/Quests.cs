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
    public class Quests : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public Quests(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [Command("InitializeQuest")]
        [Summary("Creates a mod quest room in this game's category only accessible by mod accept roles.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task InitQuest(
            [Name("ModQuestRoom Id")][Summary("Sets this channel as the mod quest room, if no channel id is given, creates new channel (optional")] ulong modRoomId = 0)
        {
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

            var modRoom = await _repo.FindRoom(modQuestChannel.Id);
            if (modRoom is null)
            {
                modRoom = new Room()
                {
                    DiscordId = modQuestChannel.Id,
                };
            }

            game.ModQuestRoom = modRoom;

            await _log.Log($"Initialized quest feature for {game.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }

        [Command("addQuest")]
        [Summary("Adds a new quest.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddQuest(
            [Name("Text")][Summary("Text of the quest")][Remainder] string text)
        {
            // Find a game for the category this channel is in
            var categoryId = (Context.Channel as INestedChannel).CategoryId.Value;
            var game = await _repo.FindGameByCategoryId(categoryId);

            await Context.Message.DeleteAsync();

            var message = await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} has added a new quest:\n{text}\n" +
                $"Press ✋ down below to claim this quest.");
            await message.AddReactionAsync(new Emoji("✋"));

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

        [Command("resetQuests")]
        [Summary("Resets completed quest amount for all users.")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task ResetQuests()
        {
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

            var users = await _repo.FindUsersWithQuestsForGame(game);

            foreach(var user in users)
            {
                user.CompletedQuests.Where(c => c.GameName == game.Name).Single().QuestCount = 0;
                await _repo.UpdateUser(user);
            }
        }

        [Command("questLeaderboard")]
        [Summary("Shows the top 10 users with the most completed quests for this game.")]
        public async Task QuestLeaderboard()
        {
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

        [Command("quests")]
        [Summary("Shows the amount of quests you have completed for this game.")]
        public async Task QuestsCommand()
        {
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

            var user = await _repo.FindUser(Context.User.Id);
            var count = 0;
            if (user is not null && user.CompletedQuests.Where(c => c.GameName == game.Name).Any())
                count = user.CompletedQuests.Where(c => c.GameName == game.Name).Single().QuestCount;
            await ReplyAsync($"You have completed {count} quests for {game.Name}.");
        }

        [Command("quests")]
        [Summary("Shows the amount of quests given user has completed for this game.")]
        public async Task QuestsCommand(
            [Name("User name")][Summary("Name of the user.")] string userName)
        {
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

            var discordUser = await UserFinder.FindUserByName(userName, Context.Guild);

            if (discordUser is null)
            {
                await Context.Channel.SendMessageAsync("Sorry, I couldn't find that user on this server, make sure you wrote the name correctly.");
                return;
            }

            var user = await _repo.FindUser(discordUser.Id);
            var count = 0;
            if (user is not null && user.CompletedQuests.Where(c => c.GameName == game.Name).Any())
                count = user.CompletedQuests.Where(c => c.GameName == game.Name).Single().QuestCount;
            await ReplyAsync($"{discordUser.Username} has completed {count} quests for {game.Name}.");
        }
    }
}
