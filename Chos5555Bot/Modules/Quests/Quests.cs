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
using System.Runtime.InteropServices;
using Discord.Rest;
using Discord.WebSocket;

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

            var message = await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} has added a new quest:\n {text}\n" +
                $"Press ✋ down below to claim this quest.");
            await message.AddReactionAsync(new Emoji("✋"));

            var quest = new Quest()
            {
                Game = game,
                QuestMessage = message.Id
            };

            await _repo.AddQuest(quest);
        }
    }
}
