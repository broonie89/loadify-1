using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Caliburn.Micro;
using loadify.Audio;
using loadify.Event;
using loadify.Model;
using SpotifySharp;

namespace loadify.Spotify
{
    public class LoadifySession : SpotifySessionListener
    {
        private readonly IEventAggregator _EventAggregator;
        private SpotifySession _Session { get; set; }
        private SynchronizationContext _Synchronization { get; set; }
        private TrackDownloadService _TrackDownloadService { get; set; }

        private Action<SpotifyError> _LoggedInCallback = error => { };    

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

        public void Login(string username, string password, Action<SpotifyError> callback)
        {
            if (Connected) return;
            _LoggedInCallback = callback;
            _Session.Login(username, password, false, null);
        }

        public async Task<IEnumerable<PlaylistModel>> GetPlaylists()
        {
            var playlists = new List<PlaylistModel>();
            if (_Session == null) return playlists;

            var container = _Session.Playlistcontainer();
            if (container == null) return playlists;
            await SpotifyObject.WaitForInitialization(container.IsLoaded);

            for (var i = 0; i < container.NumPlaylists(); i++)
            {
                var unmanagedPlaylist = container.Playlist(i);
                if (unmanagedPlaylist == null) continue;

                var managedPlaylistModel = await PlaylistModel.FromLibrary(unmanagedPlaylist, this);
                playlists.Add(managedPlaylistModel);
            }

            return playlists;
        }

        public Image GetImage(ImageId imageId)
        {
            return Image.Create(_Session, imageId);
        }

        public void DownloadTrack(TrackModel track, TrackDownloadService trackDownloadService)
        {
            _TrackDownloadService = trackDownloadService;
            _TrackDownloadService.Start(track);
            _Session.PlayerLoad(track.UnmanagedTrack);
            _Session.PlayerPlay(true);
        }

        public async Task<PlaylistModel> GetPlaylist(string url)
        {
            var link = Link.CreateFromString(url);
            if (link == null) throw new InvalidSpotifyUrlException(url);

            var unmanagedPlaylist = Playlist.Create(_Session, link);
            if (unmanagedPlaylist == null) throw new InvalidSpotifyUrlException(url);

            var managedPlaylist = await PlaylistModel.FromLibrary(unmanagedPlaylist, this);
            return managedPlaylist;
        }

        public async Task<TrackModel> GetTrack(string url)
        {
            var link = Link.CreateFromString(url);
            if (link == null) throw new InvalidSpotifyUrlException(url);

            var track = link.AsTrack();
            if (track == null) throw new InvalidSpotifyUrlException(url);

            var managedTrack = await TrackModel.FromLibrary(track, this);
            return managedTrack;
        }

        private void InvokeProcessEvents()
        {
            _Synchronization.Post(state => ProcessEvents(), null);
        }

        void ProcessEvents()
        {
            var timeout = 0;
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
                await SpotifyObject.WaitForInitialization(session.User().IsLoaded);
                _Session.PreferredBitrate(BitRate._320k);
            }

            _LoggedInCallback(error);
        }

        public override int MusicDelivery(SpotifySession session, AudioFormat format, IntPtr frames, int num_frames)
        {
            if (num_frames == 0) return num_frames;

            if (_TrackDownloadService != null)
            {
                if (_TrackDownloadService.Active)
                    _TrackDownloadService.ProcessInput(format, frames, num_frames);
            }

            return num_frames;
        }

        public override void PlayTokenLost(SpotifySession session)
        {
            if (_TrackDownloadService != null)
            {
                if (_TrackDownloadService.Active)
                    _TrackDownloadService.Cancel(TrackDownloadService.CancellationReason.PlayTokenLost);
            }
        }

        public override void EndOfTrack(SpotifySession session)
        {
            _Session.PlayerPlay(false);
            _TrackDownloadService.Finish();
        }
    }
}
