using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Event;
using loadify.Spotify;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase, IHandle<DataRefreshAuthorizedEvent>,
                                                     IHandle<DownloadContractRequestEvent>,
                                                     IHandle<AddPlaylistReplyEvent>,
                                                     IHandle<AddTrackReplyEvent>,
                                                     IHandle<DownloadContractCompletedEvent>,
                                                     IHandle<TrackSelectedChangedEvent>,
                                                     IHandle<TrackDownloadComplete>
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

                foreach (var track in _Playlists.SelectMany(playlist => playlist.Tracks))
                {
                    var audioProcessorPath = String.Format("{0}/{1}.{2}",
                        _SettingsManager.DirectorySetting.DownloadDirectory,
                        track.Name,
                        _SettingsManager.BehaviorSetting.AudioProcessor.TargetFileExtension);

                    var audioConverterPath = String.Format("{0}/{1}.{2}",
                        _SettingsManager.DirectorySetting.DownloadDirectory,
                        track.Name,
                        _SettingsManager.BehaviorSetting.AudioConverter.TargetFileExtension);

                    if (File.Exists(audioProcessorPath) || File.Exists(audioConverterPath))
                        track.ExistsLocally = true;
                }


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
            _EventAggregator.PublishOnUIThread(new AddPlaylistRequestEvent("Add Playlist", "Please insert the link to the Spotify playlist"));
        }

        public void RemovePlaylist(object dataContext)
        {
            var playlist = (dataContext as PlaylistViewModel);
            Playlists.Remove(playlist);
        }

        public void AddTrack(object dataContext)
        {
            var playlist = (dataContext as PlaylistViewModel);
            if (playlist == null) return;

            _EventAggregator.PublishOnUIThread(
                new AddTrackRequestEvent(String.Format("Add Track to Playlist {0}", playlist.Name),
                    "Please insert the link to the Spotify track", playlist));
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

        public void Handle(DownloadContractRequestEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DownloadContractStartedEvent(message.Session, _Playlists.SelectMany(playlist => playlist.SelectedTracks)));
            Enabled = false;
        }

        public async void Handle(AddPlaylistReplyEvent message)
        {
            if (String.IsNullOrEmpty(message.Content)) return;

            var invalidUrlEvent = new ErrorOcurredEvent("Add Playlist",
                                                        "The playlist could not be added because the url" +
                                                        " does not point to a valid Spotify playlist." +
                                                        "\n" +
                                                        " Url: " + message.Content);
            if (!Regex.IsMatch(message.Content,
                @"((?:(?:http|https)://open.spotify.com/user/[a-zA-Z]+/playlist/[a-zA-Z0-9]+)|(?:spotify:user:[a-zA-Z]+:playlist:[a-zA-Z0-9]+))"))
            {
                _EventAggregator.PublishOnUIThread(invalidUrlEvent);
            }
            else
            {
                try
                {
                    var playlist = await message.Session.GetPlaylist(message.Content);
                    Playlists.Add(new PlaylistViewModel(playlist, _EventAggregator));
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

            var invalidUrlEvent = new ErrorOcurredEvent("Add Track",
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
                    var track = await message.Session.GetTrack(message.Content);
                    message.Playlist.Tracks.Add(new TrackViewModel(track, _EventAggregator));
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
            NotifyOfPropertyChange(() => Playlists);
        }
    }
}
