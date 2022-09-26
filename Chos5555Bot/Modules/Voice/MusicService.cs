using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Responses.Search;
using Victoria.Enums;

namespace Chos5555Bot.Modules.Voice
{
    public class MusicService
    {
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;

        public MusicService(LavaNode lavaNode, DiscordSocketClient client)
        {
            _client = client;
            _lavaNode = lavaNode;
        }

        public Task InitializeAsync()
        {
            _client.Ready += ClientReadyAsync;
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += TrackFinished;

            return Task.CompletedTask;
        }

        public async Task ConnectAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            await _lavaNode.JoinAsync(voiceChannel, textChannel);
        }

        public async Task LeaveAsync(SocketVoiceChannel voiceChannel)
            => await _lavaNode.LeaveAsync(voiceChannel);

        public async Task<string> PlayAsync(string query, IGuild guild)
        {
            var _player = _lavaNode.GetPlayer(guild);
            var results = await _lavaNode.SearchYouTubeAsync(query);

            if (results.Status == SearchStatus.NoMatches || results.Status == SearchStatus.LoadFailed)
            {
                return "No matches found.";
            }

            var track = results.Tracks.FirstOrDefault();

            if (_player.PlayerState == PlayerState.Playing)
            {
                _player.Queue.Enqueue(track);
                return $"{track.Title} has been added to the queue.";
            }
            else
            {
                await _player.PlayAsync(track);
                return $"Now Playing: {track.Title}";
            }
        }

        public async Task<string> StopAsync(IGuild guild)
        {
            var _player = _lavaNode.GetPlayer(guild);
            if (_player is null)
                return "Error with Player";
            await _player.StopAsync();
            return "Music Playback Stopped.";
        }

        public async Task<string> SkipAsync(IGuild guild)
        {
            var _player = _lavaNode.GetPlayer(guild);
            if (_player is null || _player.Queue.Count == 0)
                return "Nothing in queue.";

            var oldTrack = _player.Track;
            await _player.SkipAsync();
            return $"Skiped: {oldTrack.Title} \nNow Playing: {_player.Track.Title}";
        }

        public async Task<string> SetVolumeAsync(ushort vol, IGuild guild)
        {
            var _player = _lavaNode.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (vol > 150 || vol <= 2)
            {
                return "Please use a number between 2 - 150";
            }

            await _player.UpdateVolumeAsync(vol);
            return $"Volume set to: {vol}";
        }

        public async Task<string> PauseOrResumeAsync(IGuild guild)
        {
            var _player = _lavaNode.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (_player.PlayerState != PlayerState.Paused)
            {
                await _player.PauseAsync();
                return "Player is Paused.";
            }
            else
            {
                await _player.ResumeAsync();
                return "Playback resumed.";
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            var _player = _lavaNode.GetPlayer(guild);
            if (_player is null)
                return "Player isn't playing.";

            if (_player.PlayerState == PlayerState.Paused)
            {
                await _player.ResumeAsync();
                return "Playback resumed.";
            }

            return "Player is not paused.";
        }


        private async Task ClientReadyAsync()
        {
            // Avoid calling ConnectAsync again if it's already connected 
            // (It throws InvalidOperationException if it's already connected).
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private async Task TrackFinished(TrackEndedEventArgs args)
        {
            if (args.Reason != TrackEndReason.Finished)
                return;

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await player.TextChannel.SendMessageAsync("There are no more tracks in the queue.");
                return;
            }

            await player.PlayAsync(nextTrack);
        }

        private static Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.ToString());
            return Task.CompletedTask;
        }
    }
}