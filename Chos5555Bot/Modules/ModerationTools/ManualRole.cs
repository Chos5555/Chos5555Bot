using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Misc;
using Discord.Rest;
using Discord.WebSocket;
using Chos5555Bot.Misc;

namespace Chos5555Bot.Modules.ModerationTools
{
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
        private async Task setRoleDescriptionCommand(IRole discordRole, [Remainder] string desc)
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
        private async Task setRoleEmoteCommand(IRole discordRole, string emote)
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
        private async Task setRoleResettableCommand(IRole discordRole, bool value)
        {
            var role = await _repo.FindRole(discordRole);

            role.Resettable = value;
            await _repo.UpdateRole(role);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addChannelToRole")]
        private async Task AddChannelToRoleCommand(IRole role, [Remainder] string gameName)
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
        private async Task resetRoleCommand(IRole discordRole)
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
