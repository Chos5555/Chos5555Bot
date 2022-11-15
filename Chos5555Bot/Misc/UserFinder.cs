using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace Chos5555Bot.Misc
{
    public static class UserFinder
    {
        public async static Task<IUser> FindUserByName(string name, IGuild guild)
        {
            name = name.ToLower();
            var users = await guild.GetUsersAsync();
            return users
                .Where(u => (u.Username is not null && u.Username.ToLower() == name) ||
                    (u.Nickname is not null && u.Nickname.ToLower() == name) ||
                    (u.DisplayName is not null && u.DisplayName.ToLower() == name))
                .SingleOrDefault();
        }
    }
}
