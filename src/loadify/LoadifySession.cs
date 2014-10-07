using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            _Session.Playlistcontainer().Release();
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
            _Session.Login(username, password, false, null);
        }

        public IEnumerable<PlaylistModel> GetPlaylists()
        {
            var playlists = new List<PlaylistModel>();
            if (_Session == null) return playlists;

            var container = _Session.Playlistcontainer();
            if (container == null) return playlists;

            for (var i = 0; i < container.NumPlaylists(); i++)
            {
                var unmanagedPlaylist = container.Playlist(i);
                var managedPlaylistModel = new PlaylistModel(unmanagedPlaylist);

                if (unmanagedPlaylist == null) continue;
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
                    var managedTrack = new TrackModel();

                    if (unmanagedTrack == null) continue;
                    managedTrack.Name = unmanagedTrack.Name();
                    managedTrack.Duration = unmanagedTrack.Duration();
                    managedTrack.Rating = unmanagedTrack.Popularity();
                    managedTrack.Album.Name = unmanagedTrack.Album().Name();
                    managedTrack.Album.ReleaseYear = unmanagedTrack.Album().Year();
                    managedTrack.Album.AlbumType = unmanagedTrack.Album().Type();

                    for (var k = 0; k < unmanagedTrack.NumArtists(); k++)
                    {
                        var unmanagedArtist = unmanagedTrack.Artist(k);
                        if (unmanagedArtist == null) continue;
 
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
    }
}
