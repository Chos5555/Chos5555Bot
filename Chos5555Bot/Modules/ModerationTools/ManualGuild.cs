using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Chos5555Bot.Exceptions;

namespace Chos5555Bot.Modules.ModerationTools
{
    /// <summary>
    /// Module class containing commands for managing guilds
    /// </summary>
    [Name("Manual Guild Management")]
    public class ManualGuild : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualGuild(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGuild")]
        [Summary("Adds a guild into the bots database.")]
        private async Task AddGuildCommand()
        {
            var guild = await _repo.FindGuild(Context.Guild);
            if (guild is not null)
            {
                await Context.Channel.SendMessageAsync("This guild is already in the database.");
                return;
            }
            guild = new Guild() { DiscordId = Context.Guild.Id };
            await _repo.AddGuild(guild);

            await _log.Log($"Added guild {Context.Guild.Name} to the DB.", LogSeverity.Info);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setRuleText")]
        [Summary("Sets the text of rule channel (if this message is a reply to a message, it will take that messages text.).")]
        private async Task SetRuleTextCommand(
            [Name("Text")][Summary("Text if this command is not a reply to a message (optional).")][Remainder] string text = null)
        {
            // If this is a response to some other message, take that messages content
            if (Context.Message.ReferencedMessage is not null)
                text = Context.Message.ReferencedMessage.Content;

            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.RuleMessageText = text;

            if (guild.RuleRoom is null)
                return;

            var ruleRoom = Context.Guild.GetChannel(guild.RuleRoom.DiscordId) as SocketTextChannel;

            // If there already is en existing message, modify it, otherwise send a new one
            if (guild.RuleMessageId != 0)
            {
                var message = await ruleRoom.GetMessageAsync(guild.RuleMessageId);
                await (message as IUserMessage).ModifyAsync(m => { m.Content = text; });
            }
            else
            {
                await SendRuleMessage(ruleRoom, guild);
            }

            await _repo.UpdateGuild(guild);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setRuleChannel")]
        [Alias("setRuleRoom")]
        [Summary("Set the rule channel.")]
        private async Task SetRuleRoomCommand(
            [Name("Channel")][Summary("Channel to be set as rule channel, if not provided, will take the channel the command is used in.")] IChannel discordChannel = null)
        {
            // If there is no channel provided, take the channel the command was used in
            discordChannel ??= Context.Channel;

            // Try to find the channel in the DB, if its not there, create a new Room and add it into DB
            var channel = await _repo.FindRoom(discordChannel);
            if (channel is null)
            {
                channel = new Room()
                {
                    DiscordId = discordChannel.Id,
                };
                await _repo.AddRoom(channel);
            }

            var guild = await _repo.FindGuild(Context.Guild.Id);

            // If there already is en existing message, delete it
            if (guild.RuleMessageId != 0)
            {
                var oldRuleRoom = Context.Guild.GetTextChannel(guild.RuleRoom.DiscordId);
                var oldMessage = await (oldRuleRoom as SocketTextChannel).GetMessageAsync(guild.RuleMessageId);
                await oldMessage.DeleteAsync();
            }

            // Send the rule message into the new channel
            guild.RuleRoom = channel;
            await SendRuleMessage(Context.Guild.GetTextChannel(channel.DiscordId), guild);

            await _repo.UpdateGuild(guild);
        }

        private async static Task SendRuleMessage(ITextChannel ruleRoom, Guild guild)
        {
            var message = await ruleRoom.SendMessageAsync(guild.RuleMessageText);
            guild.RuleMessageId = message.Id;

            await message.AddReactionAsync(new Emoji("✅"));
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setMemberRole")]
        [Summary("Sets member role for this guild.")]
        private async Task SetMemberRoleCommand(
            [Name("Role")][Summary("Role to be set as member role (needs to be a mention).")] IRole discordRole)
        {
            var role = new Role()
            {
                DisordId = discordRole.Id,
                Name = discordRole.Name,
                Resettable = false
            };

            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.MemberRole = role;

            await _repo.AddRole(role);
            await _repo.UpdateGuild(guild);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setArchiveCategory")]
        [Summary("Sets a category to which channels are archived when deleted.")]
        private async Task SetArchiveCategoryCommand(
            [Name("Category Id")][Summary("Id of the category channel.")] ulong channelId)
        {
            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.ArchiveCategoryId = channelId;
            await _repo.UpdateGuild(guild);
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("removeChannel")]
        [Alias("deleteChannel")]
        [Summary("Deletes the channel the command was used in (archives it).")]
        private async Task AddChannelToRoleCommand()
        {
            var room = await _repo.FindRoom(Context.Channel);
            var game = await _repo.FindGameByRoom(room);
            var guild = await _repo.FindGuild(Context.Guild);

            game.Rooms.Remove(room);
            await _repo.UpdateGame(game);

            // Put channel into archive category
            await (Context.Channel as INestedChannel).ModifyAsync(c => { c.CategoryId = guild.ArchiveCategoryId; });
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setUserLeftMessageChannel")]
        [Summary("Sets channel in which comamnd is used as the channel to which messages will be sent if user leaves the server.")]
        private async Task SetUserLeftMessageChannel()
        {
            var guild = await _repo.FindGuild(Context.Guild);

            guild.UserLeaveMessageRoomId = Context.Channel.Id;

            await _repo.UpdateGuild(guild);

            await _log.Log($"Set {Context.Channel.Name} as UserLeftMessageChannel for guild {Context.Guild.Name}.", LogSeverity.Info);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setBotPrefix")]
        [Summary("Sets bots prefix for this guild")]
        private async Task SetBotPrefixCommand(
            [Name("New prefix")][Summary("New prefix to be set for this guild.")]string prefix)
        {
            var guild = await _repo.FindGuild(Context.Guild);

            if (guild == null)
                throw new GuildNotFoundException();

            guild.Prefix = prefix;

            await _repo.UpdateGuild(guild);

            await _log.Log($"Updated prefix for guild {Context.Guild.Name} to \"{prefix}\"", LogSeverity.Info);
        }
    }
}
