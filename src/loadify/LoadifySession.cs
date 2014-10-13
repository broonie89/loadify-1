using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using loadify.Spotify;
using NAudio.Lame;
using NAudio.Wave;
using SpotifySharp;

namespace loadify
{
    public class LoadifySession : SpotifySessionListener
    {
        private class AudioService
        {
            public enum CancellationReason
            {
                None,
                PlayTokenLost,
                Unknown,
                ConnectionLost
            };

            public byte[] AudioBuffer { get; set; }
            public bool Active { get; set; }
            public bool Finished { get; set; }
            public CancellationReason Cancellation { get; set; }
            public int BitRate { get; set; }
            public int SampleRate { get; set; }
            public int Channels { get; set; }

            public AudioService()
            {
                Cancellation = CancellationReason.None;
                BitRate = 16;
            }

            public void ProcessInput(AudioFormat format, IntPtr frames, int num_frames)
            {
                SampleRate = format.sample_rate;
                Channels = format.channels;

                var size = num_frames * format.channels * 2;
                var buffer = new byte[size];
                Marshal.Copy(frames, buffer, 0, size);
                AudioBuffer = AudioBuffer.Concat(buffer).ToArray();
            }

            public void Reset()
            {
                AudioBuffer = new byte[1024];
                Active = false;
                Finished = false;
                Cancellation = CancellationReason.None;
                BitRate = 16;
                SampleRate = 0;
                Channels = 0;
            }
        }

        private IEventAggregator _EventAggregator;
        private SpotifySession _Session { get; set; }
        private SynchronizationContext _Synchronization { get; set; }
        private AudioService _AudioService { get; set; }

        public bool Connected
        {
            get
            {
                if (_Session == null) return false;
                return (_Session.Connectionstate() == ConnectionState.LoggedIn);
            }
        }

        public LoadifySession(IEventAggregator eventAggregator)
        {
            _AudioService = new AudioService();
            _EventAggregator = eventAggregator;
            Setup();
        }

        ~LoadifySession()
        {
            Release();
        }

        public void Release()
        {
            if (_Session == null) return;
            _Session.Logout();
            _Session.Playlistcontainer().Release();
        }

        private void Setup()
        {
            var cachePath = Properties.Settings.Default.CacheDirectory;
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            var config = new SpotifySessionConfig()
            {
                ApiVersion = 12,
                CacheLocation = cachePath,
                SettingsLocation = cachePath,
                ApplicationKey = Properties.Resources.spotify_appkey,
                UserAgent = "Loadify",
                Listener = this
            };

            _Synchronization = SynchronizationContext.Current;
            _Session = SpotifySession.Create(config);
        }

        public void Login(string username, string password)
        {
            if (Connected) return;
            _Session.Login(username, password, false, null);
        }

        public async Task<IEnumerable<PlaylistModel>> GetPlaylists()
        {
            var playlists = new List<PlaylistModel>();
            if (_Session == null) return playlists;

            var container = _Session.Playlistcontainer();
            if (container == null) return playlists;
            await WaitForCompletion(container.IsLoaded);

            for (var i = 0; i < container.NumPlaylists(); i++)
            {
                var unmanagedPlaylist = container.Playlist(i);
                var managedPlaylistModel = new PlaylistModel(unmanagedPlaylist);
                if (unmanagedPlaylist == null) continue;
                await WaitForCompletion(unmanagedPlaylist.IsLoaded);

                managedPlaylistModel.Name = unmanagedPlaylist.Name();
                managedPlaylistModel.Subscribers = unmanagedPlaylist.Subscribers().ToList();
                managedPlaylistModel.Creator = unmanagedPlaylist.Owner().DisplayName();
                managedPlaylistModel.Description = unmanagedPlaylist.GetDescription();

                var playlistImageId = unmanagedPlaylist.GetImage();
                if (playlistImageId != null)
                    managedPlaylistModel.Image = GetImage(playlistImageId).Data();

                for (var j = 0; j < unmanagedPlaylist.NumTracks(); j++)
                {
                    var unmanagedTrack = unmanagedPlaylist.Track(j);
                    var managedTrack = new TrackModel(unmanagedTrack);

                    if (unmanagedTrack == null) continue;
                    await WaitForCompletion(unmanagedTrack.IsLoaded);

                    managedTrack.Name = unmanagedTrack.Name();
                    managedTrack.Duration = unmanagedTrack.Duration();
                    managedTrack.Rating = unmanagedTrack.Popularity();

                    if (unmanagedTrack.Album() != null)
                    {
                        await WaitForCompletion(unmanagedTrack.Album().IsLoaded);
                        managedTrack.Album.Name = unmanagedTrack.Album().Name();
                        managedTrack.Album.ReleaseYear = unmanagedTrack.Album().Year();
                        managedTrack.Album.AlbumType = unmanagedTrack.Album().Type();
                    }

                    for (var k = 0; k < unmanagedTrack.NumArtists(); k++)
                    {
                        var unmanagedArtist = unmanagedTrack.Artist(k);
                        if (unmanagedArtist == null) continue;
                        await WaitForCompletion(unmanagedArtist.IsLoaded);

                        managedTrack.Artists.Add(new ArtistModel() { Name = unmanagedArtist.Name() });
                    }

                    managedPlaylistModel.Tracks.Add(managedTrack);
                }

                playlists.Add(managedPlaylistModel);
            }

            return playlists;
        }

        public Image GetImage(ImageId imageId)
        {
            return Image.Create(_Session, imageId);
        }

        public async Task DownloadTrack(TrackModel track, TrackDownloader trackDownloader)
        {
            await Task.Run(() =>
            {
                _Session.PlayerLoad(track.UnmanagedTrack);
                _Session.PlayerPlay(true);
                _AudioService.Reset();

                while (true)
                {
                    if (_AudioService.Finished)
                    {
                        trackDownloader.Download(track.UnmanagedTrack, new AudioData(_AudioService.AudioBuffer,
                                                                                _AudioService.SampleRate,
                                                                                _AudioService.BitRate,
                                                                                _AudioService.Channels));
                        break;
                    }

                    if (_AudioService.Cancellation == AudioService.CancellationReason.PlayTokenLost)
                        throw new PlayTokenLostException("Track could not be downloaded, the play token has been lost");
                }
            });
        }

        private void InvokeProcessEvents()
        {
            _Synchronization.Post(state => ProcessEvents(), null);
        }

        void ProcessEvents()
        {
            int timeout = 0;
            while (timeout == 0)
                _Session.ProcessEvents(ref timeout);
        }

        public override void NotifyMainThread(SpotifySession session)
        {
            InvokeProcessEvents();
            base.NotifyMainThread(session);
        }

        public override async void LoggedIn(SpotifySession session, SpotifyError error)
        {
            if (error == SpotifyError.Ok)
            {
                await WaitForCompletion(session.User().IsLoaded);
                _Session.PreferredBitrate(BitRate._320k);
                _EventAggregator.PublishOnUIThread(new LoginSuccessfulEvent());
            }
            else
                _EventAggregator.PublishOnUIThread(new LoginFailedEvent(error));

            base.LoggedIn(session, error);
        }

        public override int MusicDelivery(SpotifySession session, AudioFormat format, IntPtr frames, int num_frames)
        {
            if (num_frames != 0)
            {
                _AudioService.Active = true;
                _AudioService.ProcessInput(format, frames, num_frames);
            }

            return num_frames;
        }

        public override void PlayTokenLost(SpotifySession session)
        {
            if (_AudioService.Active)
            {
                _AudioService.Active = false;
                _AudioService.Cancellation = AudioService.CancellationReason.PlayTokenLost;
            }
        }

        public override void EndOfTrack(SpotifySession session)
        {
            _Session.PlayerPlay(false);
            _AudioService.Finished = true;
        }

        private Task<bool> WaitForCompletion(Func<bool> func)
        {
            return Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (func())
                        return true;
                };
            });
        }
    }
}
