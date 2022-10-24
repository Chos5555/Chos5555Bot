using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Misc;
using Chos5555Bot.Misc;

namespace Chos5555Bot.Modules.ModerationTools
{
    [Name("Manual Role Management")]
    public class ManualRole : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualRole(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRoleDescription")]
        [Alias("setRoleDesc")]
        [Summary("Sets a description for a role and updates its select message.")]
        private async Task setRoleDescriptionCommand(
            [Name("Role")][Summary("Role to be updated (needs to be a mention).")] IRole discordRole,
            [Name("Description")][Summary("Description of the role.")][Remainder] string desc)
        {
            var role = await _repo.FindRole(discordRole);

            var oldDesc = role.Description;

            role.Description = desc;
            await _repo.UpdateRole(role);

            // Update text on announce message
            var game = await _repo.FindGameByRole(role);
            var message = await MessageFinder.FindAnnouncedMessage(role, Context.Guild.GetTextChannel(game.ActiveCheckRoom.DiscordId));

            var newMessageContent = message.Content.Replace(oldDesc, role.Description);

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRoleEmote")]
        [Summary("Sets emote of a role and updates its select message.")]
        private async Task setRoleEmoteCommand(
            [Name("Role")][Summary("Role to be updated (needs to be a mention.)")] IRole discordRole,
            [Name("Emote")][Summary("Emote to be used.")] string emote)
        {
            var role = await _repo.FindRole(discordRole);

            var parsedEmote = EmoteParser.ParseEmote(emote);

            var oldEmote = role.ChoiceEmote.Out();

            role.ChoiceEmote = parsedEmote;
            await _repo.UpdateRole(role);

            // Update emote on announce message
            var game = await _repo.FindGameByRole(role);
            var message = await MessageFinder.FindAnnouncedMessage(role, Context.Guild.GetTextChannel(game.ActiveCheckRoom.DiscordId));

            var newMessageContent = message.Content.Replace(oldEmote.ToString(), role.ChoiceEmote.Out().ToString());

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRoleResettable")]
        [Alias("setRoleReset")]
        [Summary("Sets whether role should be resettable.")]
        private async Task setRoleResettableCommand(
            [Name("Role")][Summary("Role to be updated (needs to be a mention).")] IRole discordRole,
            [Name("Is resettable")][Summary("Whether role should be resettable (true/false).")] bool value)
        {
            var role = await _repo.FindRole(discordRole);

            role.Resettable = value;
            await _repo.UpdateRole(role);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addChannelToRole")]
        [Summary("Add channel in which the command is used to a role and hides it from other roles.")]
        private async Task AddChannelToRoleCommand(
            [Name("Role")][Summary("Role to which the channel is added (Needs to be a mention).")] IRole discordRole)
        {
            var role = await _repo.FindRole(discordRole);
            var game = await _repo.FindGameByRole(role);

            var room = await _repo.FindRoom(Context.Channel);

            // If room was not found, create it
            if (room is null)
            {
                room = new Room()
                {
                    DiscordId = Context.Channel.Id
                };
                await _repo.AddRoom(room);
            }

            game.Rooms.Add(room);

            // Set only viewable by given role, hide for gameRole
            await PermissionSetter.SetShownOnlyForRole(discordRole, Context.Guild.GetRole(game.GameRole.DisordId), Context.Channel as IGuildChannel);

            await _repo.UpdateGame(game);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("resetRole")]
        [Summary("Removes this role from all users, removes reactions from its selection message.")]
        private async Task resetRoleCommand(
            [Name("Role")][Summary("Role to be reset (needs to be a mention).")] IRole discordRole)
        {
            await _log.Log($"{Context.User.Username} initiated reset of role {discordRole.Name} on {Context.Guild.Name}.", LogSeverity.Info);
            var role = await _repo.FindRole(discordRole);
            var game = await _repo.FindGameByRole(role);

            if (!role.Resettable)
            {
                await _log.Log($"Role {role.Name} is not resettable.", LogSeverity.Verbose);
                await Context.Channel.SendMessageAsync($"Role {role.Name} cannot be reset.");
            }

            var resetRoles = new List<Role>();

            // If the role that is to be reset is MainActiveRole, reset all resettable roles, else reset just the one role
            if (role.Id == game.MainActiveRole.Id)
            {
                foreach (var currRole in game.ActiveRoles.Where(r => r.Resettable))
                {
                    resetRoles.Add(currRole);
                }
            }
            else
            {
                resetRoles.Add(role);
            }

            // Remove all reactions from the announce message and remove the role from all members holding the role
            foreach (var currRole in resetRoles)
            {
                var activeChannel = Context.Guild.GetChannel(game.ActiveCheckRoom.DiscordId) as ITextChannel;
                var message = await MessageFinder.FindAnnouncedMessage(currRole, activeChannel);
                var users = Context.Guild.GetRole(currRole.DisordId).Members;

                await message.RemoveAllReactionsForEmoteAsync(currRole.ChoiceEmote.Out());

                foreach (var user in users)
                {
                    await user.RemoveRoleAsync(currRole.DisordId);
                }

                await _log.Log($"Removed reactions and roles from all users that had role {role.Name}", LogSeverity.Verbose);
            }
        }
    }
}
