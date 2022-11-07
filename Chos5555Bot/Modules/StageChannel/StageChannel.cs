using Chos5555Bot.Exceptions;
using Chos5555Bot.Misc;
using Chos5555Bot.Services;
using DAL;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Game = DAL.Model.Game;

namespace Chos5555Bot.Modules.StageChannel
{
    public class StageChannel : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public StageChannel(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [Command("createStage")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Creates a stage channel in the same category this command is used in, mentioned role is used as speaker for this channel")]
        private async Task CreateStage(
            [Name("Role")][Summary("Mention of the speaker role.")] IRole speakerRole)
        {
            var guild = await _repo.FindGuild(Context.Guild);

            if (guild is null)
                throw new GuildNotFoundException();

            // Try to find the game this channel belongs to
            // TODO: Find by category when quest feature is done
            var commandChannel = await _repo.FindRoom(Context.Channel);
            Game game = null;
            if (commandChannel is not null)
                game = await _repo.FindGameByRoom(commandChannel);

            // Get category Id
            var categoryId = (Context.Channel as INestedChannel).CategoryId;

            var stageName = $"{(game is null ? Context.Guild.Name : game.Name)}-stage-#{guild.StageChannels.Count + 1}";

            // Create new voice channel under the category
            var discordStageChannel = await Context.Guild.CreateVoiceChannelAsync(stageName, p =>
                {
                    p.CategoryId = categoryId;
                });

            // TODO: Don't create a text channel and use Text-In-Voice voice channel when supported by Discord.Net
            var discordTextChanelForStage = await Context.Guild.CreateTextChannelAsync($"text-{stageName}", p =>
                {
                    p.CategoryId = categoryId;
                });

            // Create a Room for the channel and add it into the DB
            var stageChannel = new Room()
            {
                DiscordId = discordStageChannel.Id,
                SpeakerRoleId = speakerRole.Id,
                TextForStageId = discordTextChanelForStage.Id
            };
            await _repo.AddRoom(stageChannel);

            // Set permission so only speakerRole can speak
            await PermissionSetter.EnableSpeakOnlyForRole(speakerRole, Context.Guild.EveryoneRole, discordStageChannel);
            // Set sendMessage permission on TiV channel to false
            await PermissionSetter.EnableSendMessagesOnlyForRole(speakerRole, Context.Guild.EveryoneRole, discordStageChannel);
            await (await discordTextChanelForStage
                .SendMessageAsync($"Type {guild.Prefix}speak to ask for permission to speak."))
                .PinAsync();

            guild.StageChannels.Add(stageChannel);

            await _repo.UpdateGuild(guild);

            await _log.Log($"Created stage channel {discordStageChannel.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }

        [Command("deleteStage")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("Deletes stage channel the command is used in.")]
        private async Task DeleteStage()
        {
            var guild = await _repo.FindGuildByStageChannel(Context.Channel.Id);

            if (guild is null)
            {
                await Context.Channel.SendMessageAsync("This channel is not a stage channel.");
                return;
            }

            var stageChannel = await _repo.FindRoomByTextOfStage(Context.Channel.Id);

            // Delete text stage channel and voice stage channel from discord
            await (Context.Channel as IGuildChannel).DeleteAsync();
            await Context.Guild.GetChannel(stageChannel.DiscordId).DeleteAsync();

            // Remove from DB
            guild.StageChannels.Remove(stageChannel);
            await _repo.RemoveRoom(stageChannel);
            await _repo.UpdateGuild(guild);

            await _log.Log($"Deleted stage channel {Context.Channel.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }

        [Command("speak")]
        [RequireUserPermission(GuildPermission.Speak)]
        [Summary("Asks speakers for permission to talk.")]
        private async Task Speak()
        {
            if (await _repo.FindGuildByStageChannel(Context.Channel.Id) is null)
            {
                await Context.Channel.SendMessageAsync("This is not a stage channel, you can't ask for permission to speak in here.");
                return;
            }

            await Context.Message.AddReactionAsync(new Emoji("🔊"));
            await _log.Log($"{Context.User.Username} asked to speak in {Context.Channel.Name}.", LogSeverity.Verbose);
        }
    }
}
