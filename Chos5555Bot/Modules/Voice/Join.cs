﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Modules.Voice
{
    class Join : ModuleBase<SocketCommandContext>
    {/*
        [Command("join", RunMode = RunMode.Async)]
        public async Task Command(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel ??= (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();
        }*/
    }
}
