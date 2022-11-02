using DAL;
using Chos5555Bot.Services;
using DAL.Model;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // TODO: ResetQuests, QuestLeaderboard, handle reactions to quests (take, complete, cancel)
        [Command("addQuest")]
        [Summary("Adds a new quest.")]
        public async Task AddQuest(
            [Summary("Text of the quest")][Remainder] string text)
        {
            // Find a game for the category this channel is in
            var categoryId = (Context.Channel as INestedChannel).CategoryId.Value;
            var game = await _repo.FindGameByCategoryId(categoryId);

            await Context.Message.DeleteAsync();

            var message = await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} has added a new quest:\n {text}\n" +
                $"Press ✋ down below to claim this quest.");
            await message.AddReactionAsync(new Emoji("✋"));

            var quest = new Quest()
            {
                Game = game
            };
            await _repo.AddQuest(quest);
        }
    }
}
