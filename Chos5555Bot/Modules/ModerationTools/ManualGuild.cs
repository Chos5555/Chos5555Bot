using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.Modules.ModerationTools
{
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

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRuleText")]
        [Summary("Sets the text of rule channel (if this message is a reply to a message, it will take that messages text.).")]
        private async Task setRuleTextCommand(
            [Name("Text")][Summary("Text if this command is not a reply to a message (optional).")][Remainder] string text = null)
        {
            // If this is a response to some other message, take that messages content
            if (Context.Message.ReferencedMessage is not null)
                text = Context.Message.ReferencedMessage.Content;

            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.RuleMessageText = text;

            await _repo.UpdateGuild(guild);

            if (guild.RuleRoom is null)
                return;

            var ruleRoom = Context.Guild.GetChannel(guild.RuleRoom.DiscordId) as SocketTextChannel;

            if (guild.RuleMessageId != 0)
            {
                var message = (ruleRoom as SocketTextChannel).GetMessageAsync(guild.RuleMessageId);
                await (message as IUserMessage).ModifyAsync(m => { m.Content = text; });
            }
            else
            {
                guild.RuleMessageId = (await ruleRoom.SendMessageAsync(guild.RuleMessageText)).Id;
            }
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRuleChannel")]
        [Alias("setRuleRoom")]
        [Summary("Set the rule channel.")]
        private async Task setRuleRoomCommand(
            [Name("Channel")][Summary("Channel to be set as rule channel, if not provided, will take the channel the command is used in.")] IChannel discordChannel = null)
        {
            // If there is no channel provided, take the channel the command was used in
            if (discordChannel is null)
            {
                discordChannel = Context.Channel;
            }

            var channel = await _repo.FindRoom(discordChannel);

            if (channel is null)
            {
                channel = new Room()
                {
                    DiscordId = discordChannel.Id,
                };
            }

            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.RuleRoom = channel;
            await _repo.UpdateGuild(guild);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setMemberRole")]
        [Summary("Sets member role for this guild.")]
        private async Task setMemberRoleCommand(
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

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setArchiveCategory")]
        [Summary("Sets a category to which channels are archived when deleted.")]
        private async Task setArchiveCategoryCommand(
            [Name("Category Id")][Summary("Id of the category channel.")] ulong channelId)
        {
            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.ArchiveCategoryId = channelId;
            await _repo.UpdateGuild(guild);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
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
    }
}
