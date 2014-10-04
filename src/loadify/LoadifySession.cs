using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Events;
using SpotifySharp;

namespace loadify
{
    public class LoadifySession : SpotifySessionListener
    {
        private IEventAggregator _EventAggregator;
        private SpotifySession _Session { get; set; }
        private SynchronizationContext Synchronization { get; set; }

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
            if (_Session == null) return;
            _Session.Logout();
            _Session.FlushCaches();
            _Session.ForgetMe();
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

            Synchronization = SynchronizationContext.Current;
            _Session = SpotifySession.Create(config);
        }

        public void Login(string username, string password)
        {
            if (Connected) return;
            _Session.Login(username, password, true, null);
        }

        private void InvokeProcessEvents()
        {
            Synchronization.Post(state =>
            {
                var timeout = 0;
                _Session.ProcessEvents(ref timeout);
            }, null);
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
    }
}
