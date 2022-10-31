using Discord.WebSocket;
using System.Threading.Tasks;

namespace Chos5555Bot.Misc
{
    public class UserVoicePropertiesSetter
    {
        public async static Task UpdateMute(SocketGuildUser user, bool value)
        {
            // Unmute user
            await user.ModifyAsync(p =>
            {
                p.Mute = value;
            });
        }
    }
}
