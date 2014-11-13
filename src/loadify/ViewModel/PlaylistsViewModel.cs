using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Navigation;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Event;
using loadify.Model;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase, IHandle<DataRefreshAuthorizedEvent>,
                                                     IHandle<DownloadContractRequestEvent>,
                                                     IHandle<AddPlaylistReplyEvent>,
                                                     IHandle<AddTrackReplyEvent>,
                                                     IHandle<DownloadContractCompletedEvent>,
                                                     IHandle<TrackSelectedChangedEvent>,
                                                     IHandle<TrackDownloadComplete>,
                                                     IHandle<RemovePlaylistReplyEvent>
    {
        private ObservableCollection<PlaylistViewModel> _Playlists = new ObservableCollection<PlaylistViewModel>();
        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return _Playlists; }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;

                NotifyOfPropertyChange(() => Playlists);
                NotifyOfPropertyChange(() => SelectedTracks);
                NotifyOfPropertyChange(() => EstimatedDownloadTime);
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

                // Filter playlists and only list them if at least one of their tracks match the search pattern
                var playlistsCollectionView = CollectionViewSource.GetDefaultView(_Playlists);
                playlistsCollectionView.Filter = o =>
                {
                    return (o == null || !(o is PlaylistViewModel))
                            ? false
                            : (o as PlaylistViewModel).Tracks.Any(track => track.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
                };
                
                // After filtering the playlists, only list tracks that match the search pattern
                foreach (var playlist in _Playlists)
                {
                    var trackCollectionView = CollectionViewSource.GetDefaultView(playlist.Tracks);
                    trackCollectionView.Filter = o =>
                    {
                        return (o == null || !(o is TrackViewModel))
                                ? false
                                : (o as TrackViewModel).ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
                    };
                }

                NotifyOfPropertyChange(() => SearchTerm);
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        private bool _Enabled = true;
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                if (_Enabled == value) return;
                _Enabled = value;
                NotifyOfPropertyChange(() => Enabled);
            }
        }

        public IEnumerable<TrackViewModel> SelectedTracks
        {
            get { return new ObservableCollection<TrackViewModel>(Playlists.SelectMany(playlist => playlist.SelectedTracks)); }
        }

        public string EstimatedDownloadTime
        {
            get
            {
                var totalDuration = new TimeSpan();
                totalDuration = SelectedTracks.Aggregate(totalDuration, (current, selectedTrack) => current + selectedTrack.Duration);
                var estimatedTime = new TimeSpan((long) (((double) 100/165)*totalDuration.Ticks));
                return String.Format("{0}:{1}:{2}",
                                    ((int) estimatedTime.TotalHours).ToString("00"),
                                    estimatedTime.Minutes.ToString("00"),
                                    estimatedTime.Seconds.ToString("00"));
            }
        }

        public PlaylistsViewModel(IEnumerable<PlaylistViewModel> playlistCollection, IEventAggregator eventAggregator, ISettingsManager settingsManager) :
            base(eventAggregator, settingsManager)
        {
            _Playlists = new ObservableCollection<PlaylistViewModel>(playlistCollection);
        }

        public PlaylistsViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager) :
            this(new ObservableCollection<PlaylistViewModel>(), eventAggregator, settingsManager)
        { }

        public void AddPlaylist()
        {
            _EventAggregator.PublishOnUIThread(new AddPlaylistRequestEvent());
        }

        public void RemovePlaylist(object dataContext)
        {
            var playlist = (dataContext as PlaylistViewModel);
            _EventAggregator.PublishOnUIThread(new RemovePlaylistRequestEvent(playlist));
        }

        public void AddTrack(object dataContext)
        {
            var playlist = (dataContext as PlaylistViewModel);
            if (playlist == null) return;

            _EventAggregator.PublishOnUIThread(new AddTrackRequestEvent(playlist));
        }

        public void RefreshData()
        {
            _EventAggregator.PublishOnUIThread(new DataRefreshRequestEvent());
        }

        public async void Handle(DataRefreshAuthorizedEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DisplayProgressEvent("Retrieving Playlists...", "Please wait while Loadify is retrieving playlists from your Spotify account."));
            var playlistCollection = await message.Session.GetPlaylistCollection();
            var playlists = new List<Playlist>(await playlistCollection.GetPlaylists());
            var playlistViewModels = new ObservableCollection<PlaylistViewModel>();
            foreach(var playlist in playlists)
                playlistViewModels.Add(new PlaylistViewModel(await PlaylistModel.FromLibrary(playlist, message.Session), _EventAggregator, _SettingsManager));

            Playlists = playlistViewModels;
            _EventAggregator.PublishOnUIThread(new HideProgressEvent());
        }

        public void Handle(DownloadContractRequestEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DownloadContractStartedEvent(message.Session, _Playlists.SelectMany(playlist => playlist.SelectedTracks)));
            Enabled = false;
        }

        public async void Handle(AddPlaylistReplyEvent message)
        {
            if (String.IsNullOrEmpty(message.Content)) return;

            var invalidUrlEvent = new NotificationEvent("Add Playlist",
                                                        "The playlist could not be added because the url" +
                                                        " does not point to a valid Spotify playlist." +
                                                        "\n" +
                                                        " Url: " + message.Content);
            if (!Regex.IsMatch(message.Content,
                @"((?:(?:http|https)://open.spotify.com/user/[a-zA-Z0-9]+/playlist/[a-zA-Z0-9]+)|(?:spotify:user:[a-zA-Z0-9]+:playlist:[a-zA-Z0-9]+))"))
            {
                _EventAggregator.PublishOnUIThread(invalidUrlEvent);
            }
            else
            {
                try
                {
                    _EventAggregator.PublishOnUIThread(new DisplayProgressEvent("Adding Playlist...", 
                                                        "Please wait while Loadify is adding the playlist to your playlist collection"));
                    var playlist = await message.Session.GetPlaylist(message.Content);
                    Playlists.Add(new PlaylistViewModel(playlist, _EventAggregator, _SettingsManager));

                    if (message.Permanent)
                    {
                        var playlistCollection = await message.Session.GetPlaylistCollection();
                        await playlistCollection.Add(playlist.UnmanagedPlaylist);
                    }
                    _EventAggregator.PublishOnUIThread(new HideProgressEvent());
                }
                catch (InvalidSpotifyUrlException)
                {
                    _EventAggregator.PublishOnUIThread(invalidUrlEvent);
                }
            }
        }

        public async void Handle(AddTrackReplyEvent message)
        {
            if (String.IsNullOrEmpty(message.Content)) return;

            var invalidUrlEvent = new NotificationEvent("Add Track",
                                                        "The track could not be added because the url" +
                                                        " does not point to a valid Spotify track." +
                                                        "\n" +
                                                        " Url: " + message.Content);
            if (!Regex.IsMatch(message.Content,
                @"((?:(?:http|https)://open.spotify.com/track/[a-zA-Z0-9]+)|(?:spotify:track:[a-zA-Z0-9]+))"))
            {
                _EventAggregator.PublishOnUIThread(invalidUrlEvent);
            }
            else
            {
                try
                {
                    _EventAggregator.PublishOnUIThread(new DisplayProgressEvent("Adding Track...",
                                                        String.Format("Please wait while Loadify is adding the track to playlist {0}", message.Playlist.Name)));
                    var track = await message.Session.GetTrack(message.Content);
                    track.Playlist = message.Playlist.Playlist;
                    message.Playlist.Tracks.Add(new TrackViewModel(track, _EventAggregator));
                    _EventAggregator.PublishOnUIThread(new HideProgressEvent());
                }
                catch (InvalidSpotifyUrlException)
                {
                    _EventAggregator.PublishOnUIThread(invalidUrlEvent);
                }
            }
        }

        public void Handle(DownloadContractCompletedEvent message)
        {
            Enabled = true;
        }

        public void Handle(TrackSelectedChangedEvent message)
        {
            NotifyOfPropertyChange(() => SelectedTracks);
            NotifyOfPropertyChange(() => EstimatedDownloadTime);
        }

        public void Handle(TrackDownloadComplete message)
        {
            message.Track.ExistsLocally = true;
            NotifyOfPropertyChange(() => Playlists);
        }

        public async void Handle(RemovePlaylistReplyEvent message)
        {
            Playlists.Remove(message.Playlist);
            if (message.Permanent)
            {
                _EventAggregator.PublishOnUIThread(new DisplayProgressEvent("Removing Playlist...",
                                                    String.Format("Please wait while Loadify is removing playlist {0} from your account", message.Playlist.Name)));
                var playlistCollection = await message.Session.GetPlaylistCollection();
                await playlistCollection.Remove(message.Playlist.Playlist.UnmanagedPlaylist);
                _EventAggregator.PublishOnUIThread(new HideProgressEvent());
            }
        }
    }
}
