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
    /// <summary>
    /// Module class containing commands for managing roles
    /// </summary>
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

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("setRoleDescription")]
        [Alias("setRoleDesc")]
        [Summary("Sets a description for a role and updates its select message.")]
        private async Task SetRoleDescriptionCommand(
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
            var newMessageContent = "";
            if (oldDesc.Equals(""))
            {
                newMessageContent = message.Content + desc;
            }
            else
            {
                newMessageContent = message.Content.Replace(oldDesc, role.Description);
            }

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("deleteRoleDescription")]
        [Alias("deleteRoleDesc, delRoleDesc")]
        [Summary("Completely deletes roles description from the message and database, in case setRoleDescription doesn't work.")]
        private async Task DeleteRoleDescriptionCommand(
            [Name("Role")][Summary("Role to be updated (needs to be a mention).")] IRole discordRole)
        {
            var role = await _repo.FindRole(discordRole);

            var oldDesc = role.Description;

            role.Description = "";
            await _repo.UpdateRole(role);

            // Update text on announce message
            var game = await _repo.FindGameByRole(role);
            var message = await MessageFinder.FindAnnouncedMessage(role, Context.Guild.GetTextChannel(game.ActiveCheckRoom.DiscordId));
            var newMessageContent = message.Content.Replace(oldDesc, "");

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("setRoleEmote")]
        [Summary("Sets emote of a role and updates its select message.")]
        private async Task SetRoleEmoteCommand(
            [Name("Role")][Summary("Role to be updated (needs to be a mention.)")] IRole discordRole,
            [Name("Emote")][Summary("Emote to be used.")] IEmote emote)
        {
            var role = await _repo.FindRole(discordRole);

            var parsedEmote = EmoteParser.ParseEmote(emote.ToString());

            var oldEmote = role.ChoiceEmote.Out();

            role.ChoiceEmote = parsedEmote;
            await _repo.UpdateRole(role);

            // Update emote on announce message
            var game = await _repo.FindGameByRole(role);
            var message = await MessageFinder.FindAnnouncedMessage(role, Context.Guild.GetTextChannel(game.ActiveCheckRoom.DiscordId));

            var newMessageContent = message.Content.Replace(oldEmote.ToString(), role.ChoiceEmote.Out().ToString());

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("setRoleResettable")]
        [Alias("setRoleReset")]
        [Summary("Sets whether role should be resettable.")]
        private async Task SetRoleResettableCommand(
            [Name("Role")][Summary("Role to be updated (needs to be a mention).")] IRole discordRole,
            [Name("Is resettable")][Summary("Whether role should be resettable (true/false).")] bool value)
        {
            var role = await _repo.FindRole(discordRole);

            role.Resettable = value;
            await _repo.UpdateRole(role);
        }

        /// <summary>
        /// Assings channel to a role, making it only visible by the role.
        /// </summary>
        /// <param name="discordRole">Discord role</param>
        /// <returns>Nothing</returns>
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("addChannelToRole")]
        [Summary("Add channel in which the command is used to a role and hides it from other roles.")]
        private async Task AddChannelToRoleCommand(
            [Name("Role")][Summary("Role to which the channel is added (Needs to be a mention).")] IRole discordRole)
        {
            var role = await _repo.FindRole(discordRole);
            var game = await _repo.FindGameByRole(role);

            // Try to find the channel in DB, if it's not there, create it
            var room = await _repo.FindRoom(Context.Channel);
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
            await PermissionSetter.EnableViewOnlyForRole(discordRole, Context.Guild.GetRole(game.GameRole.DisordId), Context.Channel as IGuildChannel);

            await _repo.UpdateGame(game);
        }

        /// <summary>
        /// Reset given role, remove the role from all users that have it (other than game moderators),
        /// remove corresponding reactions on roles selection message
        /// </summary>
        /// <param name="discordRole">Discord role</param>
        /// <returns>Nothing</returns>
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("resetRole")]
        [Summary("Removes this role from all users, removes reactions from its selection message.")]
        private async Task ResetRoleCommand(
            [Name("Role")][Summary("Role to be reset (needs to be a mention).")] IRole discordRole)
        {
            await _log.Log($"{Context.User.Username} initiated reset of role {discordRole.Name} on {Context.Guild.Name}.", LogSeverity.Info);
            var role = await _repo.FindRole(discordRole);
            var game = await _repo.FindGameByRole(role);

            // Send a message if role is not resettable
            if (!role.Resettable)
            {
                await _log.Log($"Role {role.Name} is not resettable.", LogSeverity.Verbose);
                await Context.Channel.SendMessageAsync($"Role {role.Name} cannot be reset.");
                return;
            }

            var resetRoles = new List<Role>();

            // If the role to be reset is MainActiveRole, reset all resettable roles, else reset just the one role
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

            // Remove role from all (besides moderatiors) users that have that role and their reactions on announce message
            foreach (var currRole in resetRoles)
            {
                var activeChannel = Context.Guild.GetChannel(game.ActiveCheckRoom.DiscordId) as ITextChannel;
                var message = await MessageFinder.FindAnnouncedMessage(currRole, activeChannel);
                var users = Context.Guild.GetRole(currRole.DisordId).Members;

                foreach (var user in users)
                {
                    // If current user has one of the games ModAcceptRoles, keep his role and reaction
                    if (user.Roles.SelectMany(r1 => game.ModAcceptRoles.Where(r2 => r1.Id == r2.DisordId)).Any())
                    {
                        continue;
                    }
                    // Remove role from user and remove users reaction 
                    await user.RemoveRoleAsync(currRole.DisordId);
                    await message.RemoveReactionAsync(currRole.ChoiceEmote.Out(), user);
                }

                await _log.Log($"Removed reactions and roles from all users that had role {role.Name}", LogSeverity.Verbose);
            }
        }
    }
}
