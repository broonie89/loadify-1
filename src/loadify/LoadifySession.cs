using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using SpotifySharp;

namespace loadify
{
    public class LoadifySession : SpotifySessionListener
    {
        private IEventAggregator _EventAggregator;
        private SpotifySession _Session { get; set; }
        private SynchronizationContext _Synchronization { get; set; }

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
            _Session.FlushCaches();
            _Session.Dispose();
        }

        private void Setup()
        {
            var cachePath = Properties.Settings.Default.CachePath;
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
            _Session.Login(username, password, true, null);
        }

        public List<PlaylistModel> GetPlaylists()
        {
            var playlists = new List<PlaylistModel>();
            if (_Session == null) return playlists;

            var container = _Session.Playlistcontainer();
            if (container == null) return playlists;

            for (int i = 0; i < container.NumPlaylists(); i++)
                playlists.Add(new PlaylistModel(container.Playlist(i)));

            container.Release();
            return playlists;
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

        public override void LoggedIn(SpotifySession session, SpotifyError error)
        {
            if(error == SpotifyError.Ok)
                _EventAggregator.PublishOnUIThread(new LoginSuccessfulEvent());
            else
                _EventAggregator.PublishOnUIThread(new LoginFailedEvent(error));
            
            base.LoggedIn(session, error);
        }

        /// <summary>
        /// Empty callback for safe session release, don't touch.
        /// See the answer from paddy here: http://stackoverflow.com/questions/14246304/libspotify-logging-out-or-releasing-session-causes-crash
        /// </summary>
        public override void OfflineStatusUpdated(SpotifySession session)
        {
            base.OfflineStatusUpdated(session);
        }

        /// <summary>
        /// Empty callback for safe session release, don't touch.
        /// See the answer from paddy here: http://stackoverflow.com/questions/14246304/libspotify-logging-out-or-releasing-session-causes-crash
        /// </summary>
        public override void CredentialsBlobUpdated(SpotifySession session, string blob)
        {
            base.CredentialsBlobUpdated(session, blob);
        }
    }
}
