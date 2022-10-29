using Discord.Commands;
using System;
using System.Threading.Tasks;
using static DAL.Misc.EmoteParser;

namespace Chos5555Bot.TypeReaders
{
    internal class IEmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                var result = ParseEmote(input);
                return Task.FromResult(TypeReaderResult.FromSuccess(result.Out()));
            }
            catch (EmoteNotParsedException ex)
            {
                return Task.FromResult(TypeReaderResult.FromError(ex));
            }
        }
    }
}
