using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using SpotifySharp;

namespace loadify.Spotify
{
    public class LoadifySession : SpotifySessionListener
    {
        private class TrackCaptureService
        {
            private class CaptureStatistic
            {
                public int TargetDuration { get; set; }
                public uint Processings { get; set; }
                public double AverageFrameSize { get; set; }

                public CaptureStatistic(int targetDuration = 0)
                {
                    TargetDuration = targetDuration;
                }
            }


            public enum CancellationReason
            {
                None,
                PlayTokenLost,
                Unknown,
                ConnectionLost
            };

            public bool Active { get; set; }
            public bool Finished { get; set; }
            public CancellationReason Cancellation { get; set; }
            public AudioMetaData AudioMetaData { get; set; }
            public AudioProcessor AudioProcessor { get; set; }
            private CaptureStatistic _Statistic = new CaptureStatistic();

            public double Progress
            {
                get { return (double)100 / _Statistic.TargetDuration * (46.4 * _Statistic.Processings); }
            }

            public TrackCaptureService()
            {
                Cancellation = CancellationReason.None;
                AudioMetaData = new AudioMetaData();
            }

            public void Start(TrackModel track, AudioProcessor audioProcessor)
            {
                _Statistic = new CaptureStatistic(track.Duration);
                AudioProcessor = audioProcessor;
                Active = true;
            }

            public void Stop()
            {
                AudioProcessor.Release();
                Active = false;
            }

            public void ProcessInput(AudioFormat format, IntPtr frames, int num_frames)
            {
                AudioMetaData.SampleRate = format.sample_rate;
                AudioMetaData.Channels = format.channels;

                var size = num_frames * format.channels * 2;
                var buffer = new byte[size];
                Marshal.Copy(frames, buffer, 0, size);
                AudioProcessor.Process(buffer);

                _Statistic.Processings++;
                _Statistic.AverageFrameSize = (_Statistic.AverageFrameSize + size) / 2;
            }
        }

        private IEventAggregator _EventAggregator;
        private SpotifySession _Session { get; set; }
        private SynchronizationContext _Synchronization { get; set; }
        private TrackCaptureService _TrackCaptureService { get; set; }

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
            _TrackCaptureService = new TrackCaptureService();
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

        public async Task DownloadTrack(TrackModel track, AudioProcessor audioProcessor, AudioConverter audioConverter)
        {
            await Task.Run(() =>
            {
                _TrackCaptureService = new TrackCaptureService();
                _TrackCaptureService.Start(track, audioProcessor);
                _Session.PlayerLoad(track.UnmanagedTrack);
                _Session.PlayerPlay(true);

                while (true)
                {
                    if (_TrackCaptureService.Finished)
                    {
                        if (audioConverter != null)
                            audioConverter.Convert(_TrackCaptureService.AudioProcessor.OutputFilePath);
                        break;
                    }

                    if (_TrackCaptureService.Cancellation == TrackCaptureService.CancellationReason.PlayTokenLost)
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
            if (num_frames != 0 && _TrackCaptureService.Active)
            {
                _TrackCaptureService.ProcessInput(format, frames, num_frames);
                _EventAggregator.PublishOnUIThread(new DownloadProgressUpdatedEvent(_TrackCaptureService.Progress));
            }

            return num_frames;
        }

        public override void PlayTokenLost(SpotifySession session)
        {
            if (_TrackCaptureService.Active)
            {
                _TrackCaptureService.Stop();
                _TrackCaptureService.Cancellation = TrackCaptureService.CancellationReason.PlayTokenLost;
            }
        }

        public override void EndOfTrack(SpotifySession session)
        {
            _Session.PlayerPlay(false);
            _TrackCaptureService.Stop();
            _TrackCaptureService.Finished = true;
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
