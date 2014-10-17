using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using loadify.Spotify;
using loadify.View;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase, IHandle<DataRefreshAuthorizedEvent>,
                                                     IHandle<DownloadRequestEvent>,
                                                     IHandle<AddPlaylistReplyEvent>
    {
        private ObservableCollection<PlaylistViewModel> _Playlists = new ObservableCollection<PlaylistViewModel>();
        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get
            {
                if (SearchTerm.Length == 0) return _Playlists;

                var matchingPlaylists = new ObservableCollection<PlaylistViewModel>();
                foreach (var playlist in _Playlists)
                {
                    var matchingTracks =
                        new ObservableCollection<TrackViewModel>(playlist.Tracks
                                                                .Where(track => track.ToString()
                                                                .Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
                    if (matchingTracks.Count != 0)
                    {
                        matchingPlaylists.Add(new PlaylistViewModel(playlist)
                        {
                            Tracks = matchingTracks
                        });
                    }
                }

                return matchingPlaylists;
            }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        private string _SearchTerm = "";
        public string SearchTerm
        {
            get { return _SearchTerm; }
            set
            {
                if (_SearchTerm == value) return;
                _SearchTerm = value;
                NotifyOfPropertyChange(() => SearchTerm);
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public PlaylistsViewModel(IEnumerable<PlaylistViewModel> playlistCollection, IEventAggregator eventAggregator) :
            base(eventAggregator)
        {
            _Playlists = new ObservableCollection<PlaylistViewModel>(playlistCollection);
        }

        public PlaylistsViewModel(IEventAggregator eventAggregator) :
            this(new ObservableCollection<PlaylistViewModel>(), eventAggregator)
        { }

        public void AddPlaylist()
        {
            _EventAggregator.PublishOnUIThread(new AddPlaylistEvent());
        }

        public void RemovePlaylist(object dataContext)
        {
            var playlist = (dataContext as PlaylistViewModel);
            Playlists.Remove(playlist);
        }

        public void RefreshData()
        {
            _EventAggregator.PublishOnUIThread(new DataRefreshRequestEvent());
        }

        public async void Handle(DataRefreshAuthorizedEvent message)
        {
            var playlists = await message.Session.GetPlaylists();
            Playlists = new ObservableCollection<PlaylistViewModel>(playlists.Select(playlist => new PlaylistViewModel(playlist, _EventAggregator)));
        }

        public void Handle(DownloadRequestEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DownloadStartedEvent(message.Session, _Playlists.SelectMany(playlist => playlist.SelectedTracks)));
        }

        public async void Handle(AddPlaylistReplyEvent message)
        {
            if (String.IsNullOrEmpty(message.Url)) return;

            var invalidUrlEvent = new ErrorOcurredEvent("Add Playlist",
                                                        "The playlist could not be added because the url" +
                                                        " does not point to a valid Spotify playlist." +
                                                        "\n" +
                                                        " Url: " + message.Url);
            if (!Regex.IsMatch(message.Url,
                @"((?:(?:http|https)://open.spotify.com/user/[a-zA-Z]+/playlist/[a-zA-Z0-9]+)|(?:spotify:user:[a-zA-Z]+:playlist:[a-zA-Z0-9]+))"))
            {
                _EventAggregator.PublishOnUIThread(invalidUrlEvent);
            }
            else
            {
                try
                {
                    var playlist = await message.Session.GetPlaylist(message.Url);
                    Playlists.Add(new PlaylistViewModel(playlist, _EventAggregator));
                }
                catch (InvalidPlaylistUrlException)
                {
                    _EventAggregator.PublishOnUIThread(invalidUrlEvent);
                }
            }
        }
    }
}
