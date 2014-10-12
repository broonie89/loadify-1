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
using NAudio.Lame;
using NAudio.Wave;
using SpotifySharp;

namespace loadify
{
    public class LoadifySession : SpotifySessionListener
    {
        private class TrackDownloader
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

            public TrackDownloader()
            {
                Cancellation = CancellationReason.None;
                BitRate = 16;
            }
        }

        private IEventAggregator _EventAggregator;
        private SpotifySession _Session { get; set; }
        private SynchronizationContext _Synchronization { get; set; }
        private TrackDownloader _TrackDownloader { get; set; }

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
            _TrackDownloader = new TrackDownloader();
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

        public async Task DownloadTrack(Track track, string location)
        {
            await Task.Run(() =>
            {
                _Session.PlayerLoad(track);
                _Session.PlayerPlay(true);
                _TrackDownloader = new TrackDownloader();
                _TrackDownloader.AudioBuffer = new byte[1024];

                while (true)
                {
                    if (_TrackDownloader.Finished)
                    {
                        using (var wavWriter = new WaveFileWriter(location + ".wav",
                            new WaveFormat(_TrackDownloader.SampleRate, _TrackDownloader.BitRate,
                                _TrackDownloader.Channels)))
                        {
                            wavWriter.Write(_TrackDownloader.AudioBuffer, 0, _TrackDownloader.AudioBuffer.Length);
                        }

                        using (var wavReader = new WaveFileReader(location + ".wav"))
                            using (var mp3Writer = new LameMP3FileWriter(location + ".mp3", wavReader.WaveFormat, 128))
                                wavReader.CopyTo(mp3Writer);
                        break;
                    }

                    if (_TrackDownloader.Cancellation == TrackDownloader.CancellationReason.PlayTokenLost)
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
            if (num_frames == 0) return 0;
            _TrackDownloader.Active = true;
            _TrackDownloader.SampleRate = format.sample_rate;
            _TrackDownloader.Channels = format.channels;

            var size = num_frames * format.channels * 2;
            var buffer = new byte[size];
            Marshal.Copy(frames, buffer, 0, size);
            _TrackDownloader.AudioBuffer = _TrackDownloader.AudioBuffer.Concat(buffer).ToArray();

            return num_frames;
        }

        public override void PlayTokenLost(SpotifySession session)
        {
            if (_TrackDownloader.Active)
            {
                _TrackDownloader.Active = false;
                _TrackDownloader.Cancellation = TrackDownloader.CancellationReason.PlayTokenLost;
            }
        }

        public override void EndOfTrack(SpotifySession session)
        {
            _Session.PlayerPlay(false);
            _TrackDownloader.Finished = true;
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
